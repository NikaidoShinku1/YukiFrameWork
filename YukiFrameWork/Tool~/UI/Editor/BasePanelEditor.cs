#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.UI
{
    [CustomEditor(typeof(BasePanel), true)]
    [CanEditMultipleObjects]
    public class BasePanelEditor : CustomInspectorEditor
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
                if(!Update_ScriptGenericScriptDataInfo(path, panel))
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
                Update_ScriptGenericScriptDataInfo(path, panel);
                AssetDatabase.Refresh();
            }

            if (panel.GetType() == typeof(BasePanel))
            {
                var genericInfo = Resources.Load<LocalConfigInfo>("LocalConfigInfo");
                panel.Data.ScriptNamespace = genericInfo != null ? genericInfo.nameSpace + ".UI" : "YukiFrameWork.Example.UI";
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
                if (data.type == null) continue;

                if (!data.type.IsSubclassOf(typeof(Component)))
                    fieldInfo.SetValue(target, data.target);
                else
                {
                    Component component = data.GetComponent();
                    fieldInfo.SetValue(target, component);
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
            GUILayout.Label(GenericScriptDataInfo.TitleTip, style);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            GUILayout.Label("EN");
            GenericScriptDataInfo.IsEN = EditorGUILayout.Toggle(GenericScriptDataInfo.IsEN);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(CodeManager.IsPlaying);
            EditorGUILayout.BeginHorizontal();
            var Data = panel.Data;
            GUILayout.Label(GenericScriptDataInfo.Email, GUILayout.Width(200));
            Data.CreateEmail = EditorGUILayout.TextField(Data.CreateEmail);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            Data.SystemNowTime = DateTime.Now.ToString();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.NameSpace, GUILayout.Width(200));
            Data.ScriptNamespace = EditorGUILayout.TextField(Data.ScriptNamespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.Name, GUILayout.Width(200));
            Data.ScriptName = EditorGUILayout.TextField(Data.ScriptName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.Path, GUILayout.Width(200));
            GUILayout.TextField(Data.ScriptPath);
            CodeManager.SelectFolder(Data);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            CodeManager.GenericPanelScripts(Data);
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                panel.SaveData();
            
            if (!target.GetType().Equals(typeof(BasePanel)))
            {
                CodeManager.BindInspector(panel, panel, GenericPartialScripts);
            }

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
            builder.AppendLine("using TMPro;");
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

        private bool Update_ScriptGenericScriptDataInfo(string path, BasePanel panel)
        {
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (monoScript == null) return false;
            var component = panel.gameObject.AddComponent(monoScript.GetClass());
            BasePanel currentController = component as BasePanel;

            currentController.Data = panel.Data;
            currentController.Level = panel.Level;
            DestroyImmediate(panel);
            return true;
        }

    }
}
#endif
