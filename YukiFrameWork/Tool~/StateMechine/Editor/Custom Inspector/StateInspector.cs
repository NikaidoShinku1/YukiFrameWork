using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AssemblyHelper = YukiFrameWork.Extension.AssemblyHelper;
using Object = UnityEngine.Object;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateInspectorHelper))]
    public class StateInspector : Editor
    {
        private string stateName;    

        private string filePath = @"Assets/Scripts";
        private string fileName = "NewStateBehaviour";

        private List<Type> behaviourTypes = new List<Type>();
        private List<string> fieldName = new List<string>();  
        private bool isRecomposeScript = false;
        private List<string> list = new List<string>();
        private StateManager manager;
        private void OnEnable()
        {
            StateInspectorHelper helper = (StateInspectorHelper)target;
            if (helper == null) return;

            if(helper.StateMechine != null)
                manager = helper.StateMechine.GetComponentInParent<StateManager>();

            var state = helper.node;
            fileName = state.name;
            for (int i = 0; i < state.dataBases.Count; i++)
            {
                Type type = AssemblyHelper.GetType(state.dataBases[i].typeName);
                if (type == null)
                    continue;
                Update_StateFieldInfo(type, state.dataBases[i]);
            }
            try
            {
                FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
                var types = AssemblyHelper.GetTypes(Assembly.Load(info.assembly));
                if (types != null)
                {
                    foreach (var type in types)
                        if (type.BaseType != null)
                            foreach (var baseInterface in type.BaseType.GetInterfaces())
                                if (baseInterface.ToString().Equals(typeof(IArchitecture).ToString()) && type.GetCustomAttribute<NoGenericArchitectureAttribute>() == null)
                                    list.Add(type.Name);

                    if (helper.StateMechine != null && string.IsNullOrEmpty(helper.StateMechine.architectureName))
                    {
                        helper.StateMechine.architectureName = list[0];
                        helper.StateMechine.architectureIndex = 0;
                    }
                }              
            }
            catch { }
        }

        public override void OnInspectorGUI()
        {
            StateInspectorHelper helper = (StateInspectorHelper)target;            
            if (helper == null) return;

            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("编译中>>>(编译完成后重新点击状态打开)", MessageType.Warning);
                return;
            }

            bool disabled = EditorApplication.isPlaying || helper.node.name.Equals(StateConst.entryState) || helper.node.name.StartsWith(StateConst.upState);

            EditorGUI.BeginDisabledGroup(disabled);
            bool child = helper.node.layerName != "BaseLayer";
            EditorGUI.BeginDisabledGroup(child);
            if (child)
                EditorGUILayout.HelpBox("子状态机中的状态不允许修改下标", MessageType.Warning);
            EditorGUILayout.BeginHorizontal();
           
            EditorGUILayout.LabelField("状态下标：", GUILayout.Width(80));
            int currentIndex = helper.node.index;
            helper.node.index = EditorGUILayout.IntField(helper.node.index);
            if (currentIndex != helper.node.index
                && helper.node.IsSubingState 
                && helper.StateMechine?.subStatesPair.ContainsKey(helper.node.name) == true)
            {
                foreach (var item in helper.StateMechine.subStatesPair[helper.node.name].stateBases)
                {
                    item.index = helper.node.index;
                }
            }    
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            if (manager != null && disabled == false)
            {
                helper.node.statePlayble.OnInspectorGUI(manager.StateExtension == StateExtension.Playable);
            }

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
            StateInspectorHelper helper = (StateInspectorHelper)target;
            if (helper == null) return;
            EditorGUILayout.Space(10);          
            for (int i = 0; i < state.dataBases.Count; i++)
            {
                EditorGUILayout.Space();
                var rect = EditorGUILayout.BeginHorizontal("Wizard Box");
                GUILayout.Label(EditorGUIUtility.IconContent("cs Script Icon"), GUILayout.Width(20), GUILayout.Height(20));
                state.dataBases[i].isActive = EditorGUILayout.ToggleLeft(state.dataBases[i].typeName, state.dataBases[i].isActive);
                SelectScriptMenu(rect, state.dataBases[i].typeName);
                if (GUILayout.Button("Delete","ToggleMixed"))
                {
                    state.dataBases.RemoveAt(i);
                    i--;
                    helper.StateMechine.SaveToMechine();
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
                        IEnumerable<string> paths = AssetDatabase.GetAllAssetPaths()
                        .Where(path => path.EndsWith(".cs"));                          
                         var script = paths.Where(path => path.EndsWith(".cs"))
                        .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                        .FirstOrDefault(target => target != null 
                        && target.GetClass() != null 
                        && target.GetClass().ToString().Equals(type.ToString())) 
                        ?? throw new Exception("打开脚本失败!请检查脚本是否单独新建了一个cs文件进行编写!The Script is None! --- Type:" + type);
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

            var helper = target as StateInspectorHelper;
            if (helper == null) return;
          
            SerializationStateField(type, dataBase);
            EditorGUILayout.Space();
            if(dataBase.metaDatas.Count > 0)
                EditorGUILayout.BeginVertical("OL box NoExpand");
            for (int i = 0; i < dataBase.metaDatas.Count; i++)
            {             
                EditorGUILayout.BeginHorizontal();
                PropertyField(dataBase.metaDatas[i]);              
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

            }
            if (dataBase.metaDatas.Count > 0)
                EditorGUILayout.EndVertical();
        }  

        private static void SetData(Metadata data,object value)
        {
            StateInspectorHelper helper = StateInspectorHelper.Instance;
            if (helper == null) return;       
            StateMechine stateMechine = helper.StateMechine;
            if (stateMechine == null) 
            {
                Selection.activeObject = null;
                return;
            }
            if (data.type == TypeCode.Object) 
            {
                if (data.value != value)
                {
                    data.value = value as Object;
                    stateMechine.SaveToMechine();
                }
            }           
            else if (value != null && data.value != null && data.value.ToString() != value.ToString())
            {
                data.value = value;
                stateMechine.SaveToMechine();
            }
        }              

        private void Update_StateFieldInfo(Type type, StateDataBase dataBase)
        {
            SerializedStateAttribute serializedState = type.GetCustomAttribute<SerializedStateAttribute>(true);
            if (serializedState == null)
            {
                dataBase.metaDatas.Clear();
                return;
            }
            fieldName.Clear();
            for (int i = 0; i < dataBase.metaDatas.Count; i++)
            {
                var fieldInfos = type.GetRuntimeFields().Where(x => (x.IsPublic || x.GetCustomAttribute<SerializeField>(true) != null) && x.GetCustomAttribute<HideFieldAttribute>(true) == null);
                foreach (var field in fieldInfos)
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

        private void SerializationStateField(Type type, StateDataBase dataBase)
        {
            SerializedStateAttribute serializedState = type.GetCustomAttribute<SerializedStateAttribute>();
            if (serializedState != null)
            {
                var fieldInfos = type.GetRuntimeFields().Where(x => x.IsPublic || x.GetCustomAttribute<SerializeField>() != null);          
                foreach (var field in fieldInfos)
                {
                    var tempData = dataBase.metaDatas.Find(x => !x.typeName.Equals(field.FieldType.ToString()) && x.name.Equals(field.Name));
                    if (tempData != null)
                    {
                        tempData.typeName = field.FieldType.ToString();
                    }       
                    if (dataBase.metaDatas.Find(x => x.name == field.Name) != null) continue;
                   
                    HideFieldAttribute hideField = field.GetCustomAttribute<HideFieldAttribute>();
                    if (hideField != null) continue;

                    var target = Activator.CreateInstance(type);//AssemblyHelper.DeserializeObject("{ }", type);                     
                    if (Type.GetTypeCode(field.FieldType) == System.TypeCode.Object)
                    {
                        if (field.FieldType.IsSubclassOf(typeof(Object)) || field.FieldType == typeof(Object))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Object, target, field));
                        else if (field.FieldType.IsGenericType)
                        {
                            var gta = field.FieldType.GenericTypeArguments;
                            if (gta.Length > 1)
                                continue;
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.GenericType, target, field));
                        }
                        else if (field.FieldType.IsArray)
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.FullName, TypeCode.Array, target, field));
                        else if (field.FieldType == typeof(Vector2))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector2, target, field));
                        else if (field.FieldType == typeof(Vector3))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector3, target, field));
                        else if (field.FieldType == typeof(Vector4))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector4, target, field));
                        else if (field.FieldType == typeof(Quaternion))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Quaternion, target, field));
                        else if (field.FieldType == typeof(Rect))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Rect, target, field));
                        else if (field.FieldType == typeof(Color))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Color, target, field));
                        else if (field.FieldType == typeof(Color32))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Color32, target, field));
                        else if (field.FieldType == typeof(AnimationCurve))
                            dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.AnimationCurve, target, field));
                    }
                    else if (field.FieldType.IsEnum)
                        dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Enum, target, field));
                    else
                    {
                        dataBase.metaDatas.Add(new Metadata(field.Name, field.FieldType.ToString(), (TypeCode)Type.GetTypeCode(field.FieldType), target, field));
                    }

                }

            }
        }

        private static void PropertyField(Metadata metadata)
        {
            if (metadata.type == TypeCode.Byte)
                SetData(metadata, (byte)EditorGUILayout.IntField(metadata.name, (byte)metadata.value));            
            else if (metadata.type == TypeCode.SByte)
                SetData(metadata, (sbyte)EditorGUILayout.IntField(metadata.name, (sbyte)metadata.value));
            else if (metadata.type == TypeCode.Boolean)
                SetData(metadata, EditorGUILayout.Toggle(metadata.name, (bool)metadata.value));
            else if (metadata.type == TypeCode.Int16)
                SetData(metadata, (short)EditorGUILayout.IntField(metadata.name, (short)metadata.value));
            else if (metadata.type == TypeCode.UInt16)
                SetData(metadata, (ushort)EditorGUILayout.IntField(metadata.name, (ushort)metadata.value));
            else if (metadata.type == TypeCode.Char)
                SetData(metadata, EditorGUILayout.TextField(metadata.name, metadata.data).ToCharArray().FirstOrDefault());
            else if (metadata.type == TypeCode.Int32)
                SetData(metadata, EditorGUILayout.IntField(metadata.name, (int)metadata.value));
            else if (metadata.type == TypeCode.UInt32)
                SetData(metadata, (uint)EditorGUILayout.IntField(metadata.name, (int)metadata.value));
            else if (metadata.type == TypeCode.Single)
                SetData(metadata, EditorGUILayout.FloatField(metadata.name, (float)metadata.value));
            else if (metadata.type == TypeCode.Int64)
                SetData(metadata, EditorGUILayout.LongField(metadata.name, (long)metadata.value));
            else if (metadata.type == TypeCode.UInt64)
                SetData(metadata, (ulong)EditorGUILayout.LongField(metadata.name, (long)metadata.value));
            else if (metadata.type == TypeCode.Double)
                SetData(metadata, EditorGUILayout.DoubleField(metadata.name, (double)metadata.value));
            else if (metadata.type == TypeCode.String)
                SetData(metadata, EditorGUILayout.TextField(metadata.name, metadata.data));
            else if (metadata.type == TypeCode.Enum)
                SetData(metadata, EditorGUILayout.EnumPopup(metadata.name, (Enum)metadata.value));
            else if (metadata.type == TypeCode.Vector2)
                SetData(metadata, EditorGUILayout.Vector2Field(metadata.name, (Vector2)metadata.value));
            else if (metadata.type == TypeCode.Vector3)
                SetData(metadata, EditorGUILayout.Vector3Field(metadata.name, (Vector3)metadata.value));
            else if (metadata.type == TypeCode.Vector4)
                SetData(metadata, EditorGUILayout.Vector4Field(metadata.name, (Vector4)metadata.value));
            else if (metadata.type == TypeCode.Quaternion)
            {
                Quaternion q = (Quaternion)metadata.value;
                var value = EditorGUILayout.Vector4Field(metadata.name, new Vector4(q.x, q.y, q.z, q.w));
                Quaternion q1 = new Quaternion(value.x, value.y, value.z, value.w);
                SetData(metadata, q1);
            }
            else if (metadata.type == TypeCode.Rect)
                SetData(metadata, EditorGUILayout.RectField(metadata.name, (Rect)metadata.value));
            else if (metadata.type == TypeCode.Color)
                SetData(metadata, EditorGUILayout.ColorField(metadata.name, (Color)metadata.value));
            else if (metadata.type == TypeCode.Color32)
                SetData(metadata, (Color32)EditorGUILayout.ColorField(metadata.name, (Color32)metadata.value));
            else if (metadata.type == TypeCode.AnimationCurve)
                SetData(metadata, EditorGUILayout.CurveField(metadata.name, (AnimationCurve)metadata.value));
            else if (metadata.type == TypeCode.Object)
                SetData(metadata, EditorGUILayout.ObjectField(metadata.name, (Object)metadata.value, metadata.Type, true));
            else if (metadata.type == TypeCode.GenericType | metadata.type == TypeCode.Array)
            {
                EditorGUILayout.BeginVertical();
                var rect = EditorGUILayout.GetControlRect();
                //rect.x += width;
                
                metadata.foldout = EditorGUI.BeginFoldoutHeaderGroup(rect, metadata.foldout, metadata.name);
                if (metadata.foldout)
                {
                    //EditorGUI.indentLevel = arrayBeginSpace;
                    EditorGUI.BeginChangeCheck();
                    
                    var arraySize = EditorGUILayout.DelayedIntField("Size", metadata.arraySize);
                    bool flag8 = EditorGUI.EndChangeCheck();
                    IList list = (IList)metadata.value;                  
                    if (flag8 | list.Count != metadata.arraySize)
                    {
                        metadata.arraySize = arraySize;
                        IList list1 = Array.CreateInstance(metadata.itemType, arraySize);
                        for (int i = 0; i < list1.Count; i++)
                            if (i < list.Count)
                                list1[i] = list[i];
                        if (metadata.type == TypeCode.GenericType)
                        {
                            IList list2 = (IList)Activator.CreateInstance(metadata.Type);
                            for (int i = 0; i < list1.Count; i++)
                                list2.Add(list1[i]);
                            list = list2;
                        }
                        else list = list1;
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i] = DrawingUtility.PropertyField("Element " + i, list[i], metadata.itemType);                        
                        metadata.value = list;
                    }                   
                }
                EditorGUI.EndFoldoutHeaderGroup();
                EditorGUILayout.EndVertical();
            }
        }          

        private void RecomposeScript(StateBase state)
        {
            StateInspectorHelper helper = (StateInspectorHelper)target;
            if (helper == null) return;
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
                                    name = state.name,
                                    index = state.index,
                                    isActive = true,
                                    layerName = state.layerName
                                };
                                state.dataBases.Add(stateDataBase);

                                helper.StateMechine.SaveToMechine();
                            }
                            isRecomposeScript = false;
                        }
                    }
                }


                EditorGUILayout.Space();

                var rect = EditorGUILayout.BeginHorizontal();
                GUILayout.Label("创建状态脚本路径:");
                filePath = EditorGUILayout.TextField(filePath,GUILayout.Width(350));
                CodeManager.DragObject(rect, out var newPath);
                if (!newPath.IsNullOrEmpty()) filePath = newPath;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("状态脚本名称:");
                fileName = EditorGUILayout.TextField(fileName,GUILayout.Width(350));               
                EditorGUILayout.EndHorizontal();
                if (list.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("架构选择");
                    helper.StateMechine.architectureIndex = EditorGUILayout.Popup(helper.StateMechine.architectureIndex, list.ToArray(), GUILayout.Width(350));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("创建状态脚本"))
                {
                    CreateBehaviourScript(helper.StateMechine.architectureIndex);
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
        private void CreateBehaviourScript(int selectIndex)
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
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");
            using (FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("///=====================================================");
                builder.AppendLine("/// - FileName:      " + fileName + ".cs");
                builder.AppendLine("/// - NameSpace:     " + info.nameSpace);
                builder.AppendLine("/// - Description:   框架状态类创建");
                builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
                builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
                builder.AppendLine("/// -  All Rights Reserved.");
                builder.AppendLine("///=====================================================");

                builder.AppendLine("using YukiFrameWork.States;");
                builder.AppendLine("using UnityEngine;");                
                builder.AppendLine($"namespace {info.nameSpace}");
                builder.AppendLine("{");
                if (list.Count != 0)
                    builder.AppendLine($"\t[RuntimeInitializeOnArchitecture(typeof({list[selectIndex]}),true)]");
                builder.AppendLine($"\tpublic class {fileName} : StateBehaviour");
                builder.AppendLine("\t{");
                builder.AppendLine("\t\tpublic override void OnInit()");
                builder.AppendLine("\t\t{");
                builder.AppendLine("\t\t\tbase.OnInit();");
                builder.AppendLine("\t\t}");
                builder.AppendLine("\t\tpublic override void OnEnter()");
                builder.AppendLine("\t\t{");
                builder.AppendLine("\t\t\tbase.OnEnter();");
                builder.AppendLine("\t\t}");
                builder.AppendLine("\t\tpublic override void OnUpdate()");
                builder.AppendLine("\t\t{");
                builder.AppendLine("\t\t\tbase.OnUpdate();");
                builder.AppendLine("\t\t}");
                builder.AppendLine("\t\tpublic override void OnFixedUpdate()");
                builder.AppendLine("\t\t{");
                builder.AppendLine("\t\t\tbase.OnFixedUpdate();");
                builder.AppendLine("\t\t}");
                builder.AppendLine("\t\tpublic override void OnLateUpdate()");
                builder.AppendLine("\t\t{");
                builder.AppendLine("\t\t\tbase.OnLateUpdate();");
                builder.AppendLine("\t\t}");
                builder.AppendLine("\t\tpublic override void OnExit()");
                builder.AppendLine("\t\t{");
                builder.AppendLine("\t\t\tbase.OnExit();");
                builder.AppendLine("\t\t}");
                builder.AppendLine("");
                builder.AppendLine("\t}");

                builder.AppendLine("}");              
               
                StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
                sw.Write(builder);
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
                bool disabled = EditorApplication.isPlaying || helper.node.name.Equals(StateConst.entryState) || helper.node.name.StartsWith(StateConst.upState);

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