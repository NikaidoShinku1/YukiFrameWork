#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YukiFrameWork;
using YukiFrameWork.Events;
using YukiFrameWork.Extension;

namespace YukiFrameWork.UI
{
    [CustomEditor(typeof(BasePanel), true)]
    [CanEditMultipleObjects]
    public class BasePanelEditor : OdinEditor
    {             
        private void Awake()
        {
            BasePanel panel = target as BasePanel;
            if (panel == null) return;
            panel.Data ??= new UICustomData();
            if (target.GetType().Equals(typeof(BasePanel)))                        
            {
                panel.Data.OnLoading = false;
                string path = panel.Data.ScriptPath + @"/" + panel.Data.ScriptName + ".cs";
                if(!Update_ScriptFrameWorkConfigData(path, panel))
                    panel.Data.ScriptName = target.name == "BasePanel" ? "BasePanelExample" : target.name;
            }

        }     
        protected override void OnEnable()
        {
            base.OnEnable();
            BasePanel panel = target as BasePanel;
            if (panel == null) return;           
            if (panel?.Data.OnLoading == false)
            {                
                
            }
            else
            {
                panel.Data.OnLoading = false;
                string path = panel.Data.ScriptPath + @"/" + panel.Data.ScriptName + ".cs";
                Update_ScriptFrameWorkConfigData(path, panel);
                AssetDatabase.Refresh();
            }  

            if (panel.GetType() == typeof(BasePanel) && panel.Data.ScriptNamespace.IsNullOrEmpty())
            {
                var genericInfo = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
                panel.Data.ScriptNamespace = genericInfo.nameSpace + ".UI";
            }

            if(panel.Data.IsPartialLoading)           
                EditorApplication.delayCall = () =>
                {
                    SerializedFieldBinderUtility.BindAllFields(panel, panel);
                    panel.Data.IsPartialLoading = false;
                };

        }  

        public override void OnInspectorGUI()
        {
            BasePanel panel = target as BasePanel;
            serializedObject.Update();
            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("Loading...", MessageType.Warning);
                return;
            }
            if (PrefabUtility.IsPartOfAnyPrefab(panel))
                EditorGUILayout.HelpBox("特殊警示:在预制件下生成脚本并不会自动进行挂载跟替换的操作，请自行处理。",MessageType.Warning);
            if(!panel.OnInspectorGUI())
                base.OnInspectorGUI();

            CodeManager.BindInspector(panel, panel, GenericPartialScripts, panel.Data);
            DrawScriptGenerationPanel(panel);
        }

        private void DrawScriptGenerationPanel(BasePanel panel)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("OL box NoExpand");
            GUIStyle style = new GUIStyle("AM HeaderStyle")
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
            };
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label(FrameWorkConfigData.TitleTip, style);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            GUILayout.Label("EN");
            FrameWorkConfigData.IsEN = EditorGUILayout.Toggle(FrameWorkConfigData.IsEN);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(CodeManager.IsPlaying);
            var Data = panel.Data;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(FrameWorkConfigData.Email, GUILayout.Width(200));
            Data.CreateEmail = EditorGUILayout.TextField(Data.CreateEmail);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            Data.SystemNowTime = DateTime.Now.ToString();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(FrameWorkConfigData.NameSpace, GUILayout.Width(200));
            Data.ScriptNamespace = EditorGUILayout.TextField(Data.ScriptNamespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(FrameWorkConfigData.Name, GUILayout.Width(200));
            Data.ScriptName = EditorGUILayout.TextField(Data.ScriptName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            var rect = EditorGUILayout.BeginHorizontal();

            GUILayout.Label(FrameWorkConfigData.Path, GUILayout.Width(200));
            GUILayout.TextField(Data.ScriptPath);
            CodeManager.SelectFolder(Data);
            CodeManager.DragObject(rect, out string path);
            if (!path.IsNullOrEmpty())
                Data.ScriptPath = path;
            EditorGUILayout.EndHorizontal();
            SetFolderCreated(Data);
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            CodeManager.GenericPanelScripts(Data,() => 
            {
                if (CodeManager.CheckViewBindder(panel, panel.GetComponentsInChildren<YukiBind>()))
                {
                    GenericPartialScripts();
                }
            });
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                panel.SaveData();
        }

        private string[] folderTip = new string[] { "开启", "关闭" };
        private void SetFolderCreated(UICustomData Data)
        {
            EditorGUILayout.HelpBox("开启后会在构建脚本时自动生成保存该脚本的文件夹,并同时同步路径", MessageType.Info);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(400));

            EditorGUILayout.LabelField(FrameWorkConfigData.IsEN ? "Folder Separation:" : "文件夹分离:", GUILayout.Width(120));
            Data.IsFolderCreateScripts = EditorGUILayout.Popup(Data.IsFolderCreateScripts ? 0 : 1, folderTip) == 0;
            EditorGUILayout.EndHorizontal();
        }

        private void GenericPartialScripts()
        {
            BasePanel panel = target as BasePanel;
            if (panel == null) return;

            ViewControllerPartialCodeGenerator.Generate(
                panel.Data,
                panel,
                panel.GetComponentsInChildren<YukiBind>(),
                () => panel.Data.IsPartialLoading = true,
                "UnityEngine.UI");
        }

        private bool Update_ScriptFrameWorkConfigData(string path, BasePanel panel)
        {
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (monoScript == null || PrefabUtility.IsPartOfAnyPrefab(panel)) return false;
            var component = panel.gameObject.AddComponent(monoScript.GetClass());
            BasePanel currentController = component as BasePanel;
            foreach (var item in (panel as ISerializedFieldInfo).GetSerializeFields())
            {
                (currentController as ISerializedFieldInfo).AddFieldData(item);
            }
            currentController.Data = panel.Data;
            currentController.name = currentController.Data.ScriptName;        
            DestroyImmediate(panel);
            return true;
        }

    }
}
#endif
