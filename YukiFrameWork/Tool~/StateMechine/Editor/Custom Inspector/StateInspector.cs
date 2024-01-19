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
using System.Collections;
#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateInspectorHelper))]
    public class StateInspector : Editor
    {
        private string stateName;
        private readonly List<string> animClipsName = new List<string>();       

        private string filePath = @"Assets/Scripts/";
        private string fileName = "NewStateBehaviour";

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

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("动画模式:");
            OnSwitchAnimState(helper.node, helper.node.animData.type);
            helper.node.animData.type = (StateAnimType)EditorGUILayout.EnumPopup(helper.node.animData.type,GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            if (helper.node.animData.type != StateAnimType.None)
            {
                helper.node.animData.isActiveDefaultAnim = EditorGUILayout.ToggleLeft("是否需要当前状态拥有默认动画", helper.node.animData.isActiveDefaultAnim);

                if (helper.node.animData.isActiveDefaultAnim)
                {
                    ModifyAnimData(helper.node, helper.node.animData.type);
                }
            }           
            LoadScriptData(helper.node);

            EditorGUI.EndDisabledGroup();          
        }

        private void LoadScriptData(StateBase state)
        {
            EditorGUILayout.Space(10);          
            for (int i = 0; i < state.dataBases.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                state.dataBases[i].isActive = EditorGUILayout.ToggleLeft(state.dataBases[i].typeName, state.dataBases[i].isActive);

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

        private void SerializationStateField(StateDataBase dataBase)
        {
            Type type = AssemblyHelper.GetType(dataBase.typeName);
            if (type == null)
                return;
            Update_StateFieldInfo(type, dataBase);
            SerializationStateField(type, dataBase);

            EditorGUILayout.Space();
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
                    isRecomposeScript = true;
                }
            }
            else
            {
                //Type[] types = Assembly.GetAssembly(typeof(StateBehaviour)).GetTypes();
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.FullName.StartsWith("UnityEditor") || assembly.FullName.StartsWith("UnityEngine")
                        || assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Microsoft"))
                        continue;
                    Type[] types = assembly.GetTypes();

                    foreach (var infoType in types)
                    {
                        if (infoType.BaseType == typeof(StateBehaviour))
                        {
                            string typeName = infoType.Namespace + "." + infoType.Name;
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
            string targetPath = filePath + fileName + ".cs";
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

                if (textAsset == null)
                {
                    Debug.LogError("配置文件丢失请重新下载框架或自行配置！配置文件名称：" + "StateBehaviourScripts.txt");
                    return;
                }

                string content = textAsset.text;
                content = content.Replace("#SCRIPTNAME#", fileName);              
                StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
                sw.Write(content);

                sw.Close();
                fileStream.Close();
                AssetDatabase.Refresh();
                
            }
        }

        /// <summary>
        /// 修改状态所关联的动画数据
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="type">状态的动画类型</param>
        private void ModifyAnimData(StateBase state, StateAnimType type)
        {
            animClipsName.Clear();
            switch (type)
            {
                case StateAnimType.None:
                    {
                        state.animData.layer = 0;
                        state.animData.animSpeed = 1;
                        state.animData.animLength = 100f;
                    }
                    break;
                case StateAnimType.Animator:
                    {
                        if (state.animData.animator != null && state.animData.animator.runtimeAnimatorController != null)
                        {
                            var animState = state.animData.animator.runtimeAnimatorController;
                            var animClips = animState.animationClips;

                            for (int i = 0; i < animClips.Length; i++)
                            {
                                animClipsName.Add(animClips[i].name);
                            }                 
                        }
                        if (animClipsName.Count == 0)
                        {
                            if (state.animData.animator == null)
                                GUILayout.Label("当前动画没有设置该组件!");
                            else if (state.animData.animator.runtimeAnimatorController == null)
                                GUILayout.Label("当前动画没有设置runtimeAnimatorController!");
                            else GUILayout.Label("当前动画没有设置正确的动画剪辑");
                        }
                    }
                    break;
                case StateAnimType.Animation:
                    {
                        if (state.animData.animation != null)
                        {
                            foreach (AnimationState clip in state.animData.animation)
                            {
                                Debug.Log(clip);
                                animClipsName.Add(clip.clip.name);
                            }
                        }
                        if(animClipsName.Count == 0)
                        {
                            if(state.animData.animation == null)
                                 GUILayout.Label("当前状态没有设置该组件");
                            else GUILayout.Label("当前状态没有设置正确的动画剪辑");
                        }
                    }
                    break;
            }
            EditorGUILayout.Space();
            if (type != StateAnimType.None)
            {
                SetAnim(state);
                state.animData.layer = EditorGUILayout.IntField("动画默认图层", state.animData.layer);
                state.animData.animSpeed = EditorGUILayout.FloatField("动画默认速度", state.animData.animSpeed);
                state.animData.animLength = EditorGUILayout.FloatField("动画默认长度", state.animData.animLength);
            }
        }

        private void SetAnim(StateBase state)
        {
            if (animClipsName.Count > 0)
            {
                state.animData.clipIndex = EditorGUILayout.Popup("默认动画选择", state.animData.clipIndex, animClipsName.ToArray());
                if (animClipsName.Count > 0 && state.animData.clipIndex != -1 && !string.IsNullOrEmpty(animClipsName[state.animData.clipIndex]))
                    state.animData.clipName = animClipsName[state.animData.clipIndex];
            }
        }

        private void OnSwitchAnimState(StateBase state,StateAnimType type)
        {          
            switch (type)
            {
                case StateAnimType.None:
                    {
                        GUILayout.Label("当前模式不会使用Unity动画系统");
                    }
                   
                    break;
                case StateAnimType.Animator:
                    {
                        GUILayout.Label("新版动画:");
                        state.animData.animator = (Animator)EditorGUILayout.ObjectField(state.animData.animator, typeof(Animator), true);                     
                    }
                    break;
                case StateAnimType.Animation:
                    {
                        GUILayout.Label("旧版动画:");
                        state.animData.animation = (Animation)EditorGUILayout.ObjectField(state.animData.animation, typeof(Animation), true);                      
                    }
                    break;              
            }
            EditorGUILayout.Space();
           
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