#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
                EditorApplication.delayCall = () => Bind_AllFieldInfo(panel);            

        }  
        private void Bind_AllFieldInfo(BasePanel panel)
        {
            if (Application.isPlaying) return;

            panel.Data.IsPartialLoading = false;

            IEnumerable<FieldInfo> fieldInfos = panel.GetType().GetRuntimeFields();

            ISerializedFieldInfo serialized = panel as ISerializedFieldInfo;
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                SerializeFieldData data = serialized.GetSerializeFields().FirstOrDefault(x => x.fieldName.Equals(fieldInfo.Name));

                if (data == null) continue;
                if (data.target == null) continue;

                if (!fieldInfo.FieldType.IsSubclassOf(typeof(Component)))
                    fieldInfo.SetValue(target, data.target);
                else
                {
                    Component component = data.GetComponent(fieldInfo.FieldType);
                    fieldInfo.SetValue(target, component);
                }
            }

            YukiBind[] binds = panel.GetComponentsInChildren<YukiBind>();
            if (binds != null && binds.Length > 0)
            {
                foreach (FieldInfo fieldInfo in fieldInfos)
                {
                    var b = binds.FirstOrDefault(x => x._fields.fieldName.Equals(fieldInfo.Name));
                    if (b == null) continue;
                    SerializeFieldData data = b._fields;
                    if (data == null) continue;
                    if (data.target == null) continue;
                    if (!fieldInfo.FieldType.IsSubclassOf(typeof(Component)))
                        fieldInfo.SetValue(target, data.target);
                    else
                    {
                        Component component = data.GetComponent(fieldInfo.FieldType);
                        fieldInfo.SetValue(target, component);
                    }
                }
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

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
            EditorGUILayout.BeginHorizontal();
            var Data = panel.Data;
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
            CodeManager.BindInspector(panel, panel, GenericPartialScripts);

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
            StringBuilder builder = new StringBuilder();
            
            string examplePath = panel.Data.ScriptPath + "/" + panel.Data.ScriptName + ".Example.cs";

            bool intited = File.Exists(examplePath);
            FileMode fileMode = intited ? FileMode.Open : FileMode.Create;
            if (intited)
            {
                File.WriteAllText(examplePath, string.Empty);
                AssetDatabase.Refresh();
            }

            builder.AppendLine("///=====================================================");
            builder.AppendLine("///这是由代码工具生成的代码文件,请勿手动改动此文件!");
            builder.AppendLine("///如果在代码里命名空间进行了变动,请在编辑器设置也对命名空间作出相同修改!");
            builder.AppendLine("///=====================================================");

            builder.AppendLine("using YukiFrameWork;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using UnityEngine.UI;");
            //builder.AppendLine("using TMPro;");
            builder.AppendLine();

            builder.AppendLine($"namespace {panel.Data.ScriptNamespace}");
            builder.AppendLine("{");

            builder.AppendLine($"\tpublic partial class {panel.Data.ScriptName}");
            builder.AppendLine("\t{");
            ISerializedFieldInfo serialized = panel as ISerializedFieldInfo;
            foreach (var info in serialized.GetSerializeFields())
            {
                builder.AppendLine($"\t\t{(info.fieldLevelIndex != info.fieldLevel.Length - 1 ? "[SerializeField]" : "")}{info.fieldLevel[info.fieldLevelIndex]} {info.Components[info.fieldTypeIndex]} {info.fieldName};");
            }

            YukiBind[] binds = panel.GetComponentsInChildren<YukiBind>();

            foreach (var b in binds)
            {
                var info = b._fields;
                builder
                    .AppendLine($"\t\t{(info.fieldLevelIndex != info.fieldLevel.Length - 1 ? "[SerializeField]" : "")}{info.fieldLevel[info.fieldLevelIndex]} {info.Components[info.fieldTypeIndex]} {info.fieldName};//Des:{(b.description.IsNullOrEmpty() ? string.Empty : b.description)}");
            }

            builder.AppendLine("\t}");

            builder.AppendLine("}");
         
            using (FileStream stream = new FileStream(examplePath, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StreamWriter streamWriter = new StreamWriter(stream);

                streamWriter.Write(builder);

                streamWriter.Close(); 
                stream.Close();
                panel.Data.IsPartialLoading = true;            
                AssetDatabase.Refresh();
            }                   

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
