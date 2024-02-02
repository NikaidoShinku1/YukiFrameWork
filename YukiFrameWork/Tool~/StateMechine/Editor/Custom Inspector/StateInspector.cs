using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AssemblyHelper = YukiFrameWork.Extension.AssemblyHelper;
using Object = UnityEngine.Object;
using System.Linq;
#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateInspectorHelper))]
    public class StateInspector : Editor
    {
        private string stateName;    

        private string filePath = @"Assets/Scripts";
        private string fileName = "NewStateBehaviour";

        private List<Type> behaviourTypes = new List<Type>();

        private bool isRecomposeScript = false;      
        public override void OnInspectorGUI()
        {
            StateInspectorHelper helper = (StateInspectorHelper)target;            
            if (helper == null) return;

            bool disabled = EditorApplication.isPlaying || helper.node.name.Equals(StateConst.entryState);

            EditorGUI.BeginDisabledGroup(disabled);


            EditorGUILayout.BeginHorizontal();           
            EditorGUILayout.EndHorizontal();           
            EditorGUILayout.BeginHorizontal();         
            EditorGUILayout.LabelField("状态下标：", GUILayout.Width(80));
            helper.node.index = EditorGUILayout.IntField(helper.node.index);
            EditorGUILayout.EndHorizontal();

            if (!helper.node.name.Equals(stateName))
            {
                stateName = helper.node.name;
                EditorUtility.SetDirty(helper.StateMechine);
            }
            GUILayout.Space(10);             
            
            LoadScriptData(helper.node);         

            EditorGUI.EndDisabledGroup();        
        }

        private void LoadScriptData(StateBase state)
        {
            EditorGUILayout.Space(10);          
            for (int i = 0; i < state.dataBases.Count; i++)
            {
                EditorGUILayout.Space();
                var rect = EditorGUILayout.BeginHorizontal();
                state.dataBases[i].isActive = EditorGUILayout.ToggleLeft(state.dataBases[i].typeName, state.dataBases[i].isActive);
                SelectScriptMenu(rect, state.dataBases[i].typeName);
                if (GUILayout.Button("Delete","ToggleMixed"))
                {
                    state.dataBases.RemoveAt(i);
                    i--;
                    continue;
                }                

                EditorGUILayout.EndHorizontal();                               
                SerializationStateField(state.dataBases[i]);
            }
            EditorGUILayout.Space();
            RecomposeScript(state);
        }

        private void SelectScriptMenu(Rect rect,string typeName)
        {
            Event e = Event.current;
            if (rect.Contains(e.mousePosition))
            {              
                if (e.type == EventType.MouseDown && e.button == 1)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("编辑脚本"), false, () => 
                    {
                        Type type = AssemblyHelper.GetType(typeName);
                        if (type == null) return;

                        IEnumerable<string> paths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".cs"));           
                        var script = paths.Where(path => path.EndsWith(".cs"))
                        .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                        .FirstOrDefault(target => target != null && target.GetClass() != null && target.GetClass().ToString().Equals(type.ToString())) 
                        ?? throw LogKit.Exception("打开脚本失败!请检查脚本是否单独新建了一个cs文件进行编写!The Script is None! --- Type:" + type);
                        AssetDatabase.OpenAsset(script);                                                           
                    });
                    e.Use();

                    menu.ShowAsContext();
                }
            }
        }

        private void SerializationStateField(StateDataBase dataBase)
        {
            Type type = AssemblyHelper.GetType(dataBase.typeName);
            if (type == null)
                return;
            Update_StateFieldInfo(type, dataBase);
            SerializationStateField(type, dataBase);

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("OL box NoExpand");
            for (int i = 0; i < dataBase.metaDatas.Count; i++)
            {             
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(dataBase.metaDatas[i].name, GUILayout.Width(100));
                Type fieldType = AssemblyHelper.GetType(dataBase.metaDatas[i].typeName);
                switch (dataBase.metaDatas[i].dataType)
                {
                    case DataType.Object:                                            
                        dataBase.metaDatas[i].Value = EditorGUILayout.ObjectField(dataBase.metaDatas[i].Value, fieldType, true);
                        break;
                    case DataType.Int16:
                        dataBase.metaDatas[i].Data = EditorGUILayout.IntField((short)Convert.ChangeType(dataBase.metaDatas[i].Data, typeof(short)));                    
                        break;
                    case DataType.Int32:                  
                        dataBase.metaDatas[i].Data = EditorGUILayout.IntField((int)Convert.ChangeType(dataBase.metaDatas[i].Data,typeof(int)));
                        break;
                    case DataType.Int64:
                        dataBase.metaDatas[i].Data = EditorGUILayout.LongField((long)Convert.ChangeType(dataBase.metaDatas[i].Data, typeof(long)));
                        break;
                    case DataType.Single:
                        dataBase.metaDatas[i].Data = EditorGUILayout.FloatField((float)Convert.ChangeType(dataBase.metaDatas[i].Data, typeof(float)));
                        break;
                    case DataType.Double:
                        dataBase.metaDatas[i].Data = EditorGUILayout.DoubleField((double)Convert.ChangeType(dataBase.metaDatas[i].Data, typeof(double)));
                        break;                 
                    case DataType.Boolan:
                        dataBase.metaDatas[i].Data = EditorGUILayout.Toggle((bool)Convert.ChangeType(dataBase.metaDatas[i].Data, typeof(bool)));
                        break;
                    case DataType.String:
                        dataBase.metaDatas[i].Data = EditorGUILayout.TextField((string)Convert.ChangeType(dataBase.metaDatas[i].Data, typeof(string)));
                        break;
                    case DataType.Enum:
                        dataBase.metaDatas[i].Data = EditorGUILayout.EnumPopup((Enum)dataBase.metaDatas[i].Data);
                        break;                 
                }              

                EditorGUILayout.EndHorizontal();
                
            }

            EditorGUILayout.EndVertical();
        }
        public void OnEnable()
        {
            StateInspectorHelper helper = (StateInspectorHelper)target;           
            if (helper == null) return;

            ///对数据进行初始化操作
            foreach (var dataBase in helper.node.dataBases)
            {
                foreach (var data in dataBase.metaDatas)
                {
                    InitMetaDataValue(data);
                }
            }
        }

        private void InitMetaDataValue(MetaData data)
        {
            if (!string.IsNullOrEmpty(data.value))
            {
                switch (data.dataType)
                {
                    case DataType.Int16:
                        data.data = short.Parse(data.value);
                        break;
                    case DataType.Int32:
                        data.data = int.Parse(data.value);
                        break;
                    case DataType.Int64:
                        data.data = long.Parse(data.value);
                        break;
                    case DataType.UInt16:
                        data.data = ushort.Parse(data.value);
                        break;
                    case DataType.UInt32:
                        data.data = uint.Parse(data.value);
                        break;
                    case DataType.UInt64:
                        data.data = ulong.Parse(data.value);
                        break;
                    case DataType.Single:
                        data.data = float.Parse(data.value);
                        break;
                    case DataType.Double:
                        data.data = double.Parse(data.value);
                        break;
                    case DataType.Boolan:
                        data.data = bool.Parse(data.value);
                        break;
                    case DataType.String:
                        data.data = data.value;
                        break;
                    case DataType.Enum:
                        {
                            try
                            {
                                data.data = Enum.Parse(AssemblyHelper.GetType(data.typeName), data.value);
                            }
                            catch
                            {
                                Debug.LogError("枚举转换失败，请重试！value : " + data.value);
                            }
                        }
                        break;                                       
                }
            }
        }

        private void Update_StateFieldInfo(Type type, StateDataBase dataBase)
        {
            foreach (var attribute in type.GetCustomAttributes())
            {
                if (attribute is SerializedStateAttribute)
                {
                    List<string> fieldName = new List<string>();
                    for (int i = 0; i < dataBase.metaDatas.Count; i++)
                    {                      
                        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                        {                         
                            if (dataBase.metaDatas.Find(x => x.name.Equals(field.Name) && x.typeName.Equals(field.FieldType.ToString())) != null)
                            {
                                fieldName.Add(field.Name);
                            }
                        }                                            
                    }

                    for (int i = 0; i < dataBase.metaDatas.Count; i++)
                    {
                        bool contains = false;
                        for (int j = 0; j < fieldName.Count; j++)
                        {
                            if (dataBase.metaDatas[i].name.Equals(fieldName[j]))
                            {
                                contains = true;
                            }
                        }
                        if (!contains)
                        {
                            dataBase.metaDatas.RemoveAt(i);
                            i--;
                        }
                    }

                }
            }
        }

        private void SerializationStateField(Type type, StateDataBase dataBase)
        {
            foreach (var attribute in type.GetCustomAttributes())
            {
                if (attribute is SerializedStateAttribute)
                {
                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var tempData = dataBase.metaDatas.Find(x => !x.typeName.Equals(field.FieldType.ToString()) && x.name.Equals(field.Name));
                        if (tempData != null)
                        {
                            tempData.typeName = field.FieldType.ToString();
                        }
                        if (dataBase.metaDatas.Find(x => x.name.Equals(field.Name)) != null) continue;

                        HideFieldAttribute hideField = field.GetCustomAttribute<HideFieldAttribute>();
                        if (hideField != null) continue;                     
                        if (field.FieldType.Equals(typeof(short)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.Int16, 0));
                        else if (field.FieldType.Equals(typeof(int)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.Int32, 0));
                        else if (field.FieldType.Equals(typeof(long)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.Int64, 0));
                        else if (field.FieldType.Equals(typeof(ushort)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.UInt16, 0));
                        else if (field.FieldType.Equals(typeof(uint)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.UInt32, 0));
                        else if (field.FieldType.Equals(typeof(ulong)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.UInt64, 0));
                        else if (field.FieldType.Equals(typeof(float)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.Single, 0f));
                        else if (field.FieldType.Equals(typeof(double)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.Double, 0d));
                        else if (field.FieldType.Equals(typeof(bool)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.Boolan, false));
                        else if (field.FieldType.Equals(typeof(string)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.String, string.Empty));
                        else if (field.FieldType.IsSubclassOf(typeof(Enum)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.Enum, Enum.GetValues(field.FieldType).GetValue(0)));
                        else if (field.FieldType.IsSubclassOf(typeof(Object)))
                            dataBase.metaDatas.Add(new MetaData(field.Name, field.FieldType.ToString(), DataType.Object, null));                       
                    }
                }
            }
        }

        private void RecomposeScript(StateBase state)
        {
            if (!isRecomposeScript)
            {
                if (GUILayout.Button("添加状态脚本"))
                {
                    behaviourTypes.Clear();
                    isRecomposeScript = true;
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                    foreach (var assembly in assemblies)                    
                    {
                        if (assembly.FullName.StartsWith("UnityEditor") || assembly.FullName.StartsWith("UnityEngine")
                        || assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Microsoft"))
                            continue;

                        foreach (var infoType in assembly.GetTypes())
                        {
                            if (infoType.BaseType == typeof(StateBehaviour))
                            {
                                behaviourTypes.Add(infoType);
                            }
                        }
                    }
                   
                }
            }
            else
            {                              
                foreach (var infoType in behaviourTypes)
                {
                    if (infoType.BaseType == typeof(StateBehaviour))
                    {
                        string typeName = infoType.FullName;
                        if (GUILayout.Button(infoType.Name))
                        {
                            if (state.dataBases.Find(x => x.typeName.Equals(typeName)) == null)
                            {
                                StateDataBase stateDataBase = new StateDataBase()
                                {
                                    typeName = typeName,
                                    index = state.index,
                                    isActive = true
                                };
                                state.dataBases.Add(stateDataBase);
                            }
                            isRecomposeScript = false;
                        }
                    }
                }


                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("创建状态脚本路径:");
                filePath = EditorGUILayout.TextField(filePath,GUILayout.Width(350));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("状态脚本名称:");
                fileName = EditorGUILayout.TextField(fileName,GUILayout.Width(350));
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("创建状态脚本"))
                {
                    CreateBehaviourScript();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("取消"))
                {
                    isRecomposeScript = false;
                }
            }
        }

        /// <summary>
        /// 创建脚本文件
        /// </summary>
        private void CreateBehaviourScript()
        {
            string regix = "^[a-zA-Z][a-zA-Z0-9_]*$";
            if (!Regex.Match(fileName, regix).Success)
            {
                Debug.LogError("文件名称含有不对的字符，无法作为文件名生成请重试！");
                return;
            }
            string targetPath = filePath + "/" + fileName + ".cs";
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(filePath);
                AssetDatabase.Refresh();
            }

            int i = 1;
            while (File.Exists(targetPath))
            {
                fileName += i;
                targetPath = filePath + fileName + ".cs";
            }

            using (FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                TextAsset textAsset = Resources.Load<TextAsset>("StateBehaviourScripts");

                FrameworkEasyConfig config = Resources.Load<FrameworkEasyConfig>("frameworkConfig");               

                if (textAsset == null)
                {
                    Debug.LogError("配置文件丢失请重新下载框架或自行配置！配置文件名称：" + "StateBehaviourScripts.txt");
                    return;
                }

                string content = textAsset.text;
                content = content.Replace("#SCRIPTNAME#", fileName);
                content = content.Replace("YukiFrameWork.Project", config != null && !string.IsNullOrEmpty(config.NameSpace) ? config.NameSpace : "YukiFrameWork.Project");
                StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
                sw.Write(content);

                sw.Close();
                fileStream.Close();
                Selection.activeObject = StateInspectorHelper.Instance.StateMechine.GetComponentInParent<StateManager>();
                AssetDatabase.Refresh();
                
            }
        }
              
        protected override void OnHeaderGUI()
        {
            StateInspectorHelper helper = (StateInspectorHelper)target;
            if (helper == null) return;

            string name;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(EditorGUIUtility.IconContent("icons/processed/unityeditor/animations/animatorstate icon.asset"), GUILayout.Width(30), GUILayout.Height(30));
                EditorGUILayout.LabelField("状态名称：", GUILayout.Width(60));
                bool disabled = EditorApplication.isPlaying || helper.node.name.Equals(StateConst.entryState);

                EditorGUI.BeginDisabledGroup(disabled);
                name = EditorGUILayout.DelayedTextField(helper.node.name);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                StateNodeFactory.Rename(helper.StateMechine, helper.node, name);
            }
            EditorGUILayout.Space();
            var rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
          
    }
}
#endif