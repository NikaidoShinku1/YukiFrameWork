///=====================================================
/// - FileName:      CustomInspector.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 20:42:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif
namespace YukiFrameWork
{
#if UNITY_EDITOR	
    /// <summary>
    /// 参数信息数据
    /// </summary>
    public class InfoData
    {
        public MemberInfo member;
        public SerializedProperty property;       
        public bool active;
        public LocalConfigInfo config;
        public bool foldOut
        {
            get
            {
                CheckPair();
                return config.elementFoldOutPairs[member.DeclaringType.FullName][property.displayName];
            }
            set
            {
                CheckPair();
                config.elementFoldOutPairs[member.DeclaringType.FullName][property.displayName] = value;
            }
        }

        private void CheckPair() 
        {
            if (!config.elementFoldOutPairs.ContainsKey(member.DeclaringType.FullName))
            {
                config.elementFoldOutPairs[member.DeclaringType.FullName] = new YDictionary<string, bool>();
            }

            if (!config.elementFoldOutPairs[member.DeclaringType.FullName].ContainsKey(property.displayName))
            {
                config.elementFoldOutPairs[member.DeclaringType.FullName][property.displayName] = false;
            }
        }
    }

    /// <summary>
    /// 方法信息数据
    /// </summary>
    public class MethodData
    {
        public MethodInfo method;
        public SerializedObject serializedObject;    
        public MethodButtonAttribute methodButton;
        public object[] args;
        public bool foldOut;
        public List<string> displayNames = new List<string>();
        public int displayIndex;
        public List<ReoderableInfo> reoderableInfos = new List<ReoderableInfo>();
        public class ReoderableInfo
        {
            public ReorderableList reorderableList;
            public int index;
            public Type type;
            public bool foldOut;
            public List<string> displayNames = new List<string>();
            public int displayIndex;
            public List<ReoderableInfo> childs = new List<ReoderableInfo>();
        }
    }
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class CustomInspectorEditor : Editor
    {
        private readonly Dictionary<string, List<InfoData>> infoDataPairs = new Dictionary<string, List<InfoData>>();
        private readonly Dictionary<string, ReorderableList> listPairs = new Dictionary<string, ReorderableList>();
        private readonly List<string> enumDisplayNames = new List<string>();      
        private SerializedProperty script;
        private List<MethodData> methodDatas = new List<MethodData>();
        private LocalConfigInfo config;
        protected virtual void OnEnable()
        {
            infoDataPairs.Clear();
            listPairs.Clear();
            enumDisplayNames.Clear();
            methodDatas.Clear();
            config = Resources.Load<LocalConfigInfo>("LocalConfigInfo");          
            infoDataPairs.Add("Default", new List<InfoData>());           
            try
            {
                IEnumerable<FieldInfo> memberInfos = target.GetType().GetRuntimeFields()
                .Where(x => this.serializedObject.FindProperty(x.Name) != null && x.GetCustomAttribute<HideInInspector>() == null);
                List<SerializedProperty> setProperties = new List<SerializedProperty>();
                foreach (var field in memberInfos)
                {
                    var group = field.GetCustomAttribute<GUIGroupAttribute>(true);

                    SerializedProperty property = serializedObject.FindProperty(field.Name);

                    CustomPropertySettingAttribute settingAttribute = field.FieldType.GetCustomAttribute<CustomPropertySettingAttribute>(true);
                    if (settingAttribute != null)
                    {
                        property = property.FindPropertyRelative(settingAttribute.ItemName);
                    }
                    setProperties.Add(property);
                    group ??= new GUIGroupAttribute("Default");
                    SetGroup(group, field, property, field.FieldType);
                }             

                script = serializedObject.FindProperty("m_Script");

                MethodInfo[] methodInfos = target.GetType().GetRuntimeMethods().ToArray();

                for (int i = 0; i < methodInfos.Length; i++)
                {           
                    var button = methodInfos[i].GetCustomAttribute<MethodButtonAttribute>(true);
                    if (button == null) continue;

                    methodDatas.Add(InitMethodData(methodInfos[i].GetParameters(), methodInfos[i],button));
                }

                string[] noSerializeNames = config.elementFoldOutPairs[target.GetType().FullName].Keys.Where(x => setProperties.Find(y => y.displayName == x) == null).ToArray();

                foreach (var n in noSerializeNames)
                {
                    config.elementFoldOutPairs[target.GetType().FullName].Remove(n);
                }
            }
            catch { }
            
        }

        private MethodData InitMethodData(ParameterInfo[] parameters, MethodInfo methodInfo, MethodButtonAttribute method)
        {
            MethodData methodData = new MethodData()
            {
                method = methodInfo,
                args = new object[parameters.Length],
                methodButton = method
            };
            for (int j = 0; j < parameters.Length; j++)
            {
                Type type = parameters[j].ParameterType;
                IList list = null;               
                if (type.IsArray)
                    list = Array.CreateInstance(type, 0);
                else if (type.IsGenericType)
                    list = Activator.CreateInstance(type) as IList;            
                if (list == null) continue;
                var elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                var info = new MethodData.ReoderableInfo()
                {
                    index = j,
                    type = parameters[j].ParameterType
                };

                var reoderableList = CreateReorderableList(list, type, elementType, info, parameters[j].Name);
                info.reorderableList = reoderableList;
                methodData.reoderableInfos.Add(info);
            }
            return methodData;
        }
    
        private ReorderableList CreateReorderableList(IList list,Type type,Type elementType,MethodData.ReoderableInfo info,string name)
        {
            var reoderableList = new ReorderableList(list, elementType, true, true, false, false);
            reoderableList.headerHeight = 0;
            reoderableList.drawElementBackgroundCallback = (v, index, b, c) =>
            {
                EditorGUI.DrawRect(new Rect(v) { width = v.width + 1, height = v.height + 6, y = v.y - 3 }, new Color(0.2f, 0.2f, 0.2f, 1));
            };
            reoderableList.drawElementCallback = (v, index, b, c) =>
            {
                v.width -= 20;
                if (elementType.IsSubclassOf(typeof(UnityEngine.Object)) || elementType.Equals(typeof(UnityEngine.Object)))
                {                  
                    reoderableList.list[index] = EditorGUI.ObjectField(v, (UnityEngine.Object)reoderableList.list[index], elementType, true);
                }
                else
                {                   
                    if (elementType.IsArray || elementType.IsGenericType)
                    {
                        EditorGUI.HelpBox(v, "方法参数不支持对循环嵌套的数组/列表序列化!", MessageType.Warning);
                    }
                    else if (elementType.GetCustomAttribute<SerializableAttribute>() != null && elementType.IsClass)
                    {
                        EditorGUI.HelpBox(v, "方法参数不支持对标记Serializable的类的序列化!", MessageType.Warning);
                    }
                    else if (elementType.IsSubclassOf(typeof(Enum)) || elementType.Equals(typeof(Enum)))
                    {
                        info.displayNames.Clear();
                        foreach (var field in elementType.GetRuntimeFields())
                        {
                            if (field.Name.StartsWith("value_")) continue;
                            var enumName = field.Name;

                            var enumType = field.FieldType.GetField(enumName);
                            var labelAttributes = enumType.GetCustomAttributes(typeof(LabelAttribute), false).ToArray();
                            info.displayNames.Add(labelAttributes.Length > 0 ? (labelAttributes[0] as LabelAttribute).Label : enumName);
                        }
                        int selectIndex = (int)Enum.Parse(elementType, reoderableList.list[index].ToString());
                        selectIndex = EditorGUI.Popup(v,string.Empty, selectIndex, info.displayNames.ToArray());
                        reoderableList.list[index] = Enum.GetValues(elementType).GetValue(selectIndex);
                    }
                    else
                        reoderableList.list[index] = Convert.ChangeType(DrawingUtility.PropertyField(string.Empty, reoderableList.list[index], elementType, v), elementType);
                }
                var removeRect = new Rect(v)
                {
                    x = v.xMax + 8,
                    width = 20,                  
                };
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleLeft;
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 14;
                GUI.Box(new Rect(removeRect) { x = removeRect.x - 5, width = 20 }, string.Empty, "GroupBox");
                style.normal.textColor = removeRect.Contains(Event.current.mousePosition) ? Color.white : Color.grey;
                if (GUI.Button(removeRect, "X", style))
                {
                    bool subObject = elementType.IsSubclassOf(typeof(UnityEngine.Object)) || elementType.Equals(typeof(UnityEngine.Object));
                    if (type.IsArray)
                    {
                        if (reoderableList.list.Count > 0)
                        {
                            for (int i = index; i < reoderableList.list.Count; i++)
                            {
                                if (i + 1 < reoderableList.list.Count)
                                {
                                    reoderableList.list[i] = reoderableList.list[i + 1];
                                }
                            }
                            var tempList = Array.CreateInstance(elementType, reoderableList.list.Count - 1);
                            Array.Copy((Array)reoderableList.list, tempList, reoderableList.list.Count - 1);
                            reoderableList.list = tempList;
                        }
                    }
                    else if (type.IsGenericType)
                    {
                        reoderableList.list.RemoveAt(index);
                    }
                }
            };          
            return reoderableList;
        }


        public override void OnInspectorGUI()
        {                   
            serializedObject.Update();         
            if (script != null)
            {               
                MonoScript monoScript = script.objectReferenceValue as MonoScript;
                GUI.enabled = false;
                if (monoScript == null)
                {
                    GUI.enabled = true;
                    EditorGUILayout.HelpBox("脚本组件丢失请重新挂载!", MessageType.Error);
                }              
                EditorGUILayout.PropertyField(script);
                GUI.enabled = true;
            }

            foreach (var key in infoDataPairs.Keys)
            {
                var pair = infoDataPairs[key];

                bool changeTip = false;
                foreach (var item in pair)
                {
                    if (item.active)
                    {
                        changeTip = true;
                    }
                }
                if (!key.Equals("Default") && changeTip)
                {
                    GUIStyle style = new GUIStyle("OL box NoExpand");
                    style.fontSize = 14;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Bold;
                    GUILayout.Label(key, style, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25));
                    EditorGUILayout.BeginVertical("Wizard Box");
                    EditorGUILayout.Space();
                }                    
                foreach (var item in pair)
                {                 
                    SerializedField(item);
                }
                if (!key.Equals("Default") && changeTip)
                    EditorGUILayout.EndVertical();
            }
            
            if (methodDatas?.Count > 0)
            {
                EditorGUILayout.Space(15);
                for(int i = 0;i < methodDatas.Count; i++)
                {
                    SerializeMethod(methodDatas[i]);
                    EditorGUILayout.Space(5);
                }
            }
            try
            {              
                this.serializedObject.ApplyModifiedProperties();
            }
            catch { }
        }

        private void SerializedField(InfoData item)
        {
            item.member.CreateAllSettingAttribute
             (out LabelAttribute label
             , out GUIColorAttribute color, out EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
             , out DisableEnumValueIfAttribute[] disable, out EnableIfAttribute[] enableIf, out DisableIfAttribute[] disableIf
             , out HelperBoxAttribute helperBox, out _
             , out DisplayTextureAttribute displayTexture
             , out PropertyRangeAttribute propertyRange
             , out RangeAttribute defaultRange
             , out RuntimeDisabledGroupAttribute runtimeDisabledGroup, out EditorDisabledGroupAttribute editorDisabledGroup
             , out ListDrawerSettingAttribute listDrawerSetting
             , out BoolanPopupAttribute boolanPopup, out DisableGroupIfAttribute disableGroupIf
             , out DisableGroupEnumValueIfAttribute disableGroupEnumValueIf
             , out SpaceAttribute spaceAttribute);

            GUI.color = color != null ? color.Color : Color.white;          
            if (ConditionUtility.DrawConditionIf(enableEnumValueIfAttribute, enableIf, disable, disableIf, target.GetType(), target))
            {
                item.active = true;
                EditorGUI.BeginDisabledGroup(ConditionUtility.DisableGroupLifeCycle(runtimeDisabledGroup, editorDisabledGroup) || ConditionUtility.DisableGroupInValue(target.GetType(), target, disableGroupEnumValueIf, disableGroupIf));
                if (spaceAttribute != null)              
                    EditorGUILayout.Space(spaceAttribute.height);

                if (helperBox != null)
                    DrawingUtility.PropertyFieldInHelperBox(helperBox);

                if ((defaultRange != null || propertyRange != null) && (item.property.propertyType == SerializedPropertyType.Integer || item.property.propertyType == SerializedPropertyType.Float))
                {
                    DrawingUtility.PropertyFieldInSlider(item, label, defaultRange, propertyRange);
                }
                else if (PropertyUtility.CheckPropertyInBoolan(target.GetType(), item.property) && boolanPopup != null)
                {                         
                    HelperUtility.DrawHelperWarning(item.property,displayTexture != null,false, defaultRange != null || propertyRange != null);                   
                    DrawingUtility.PropertyFieldInBoolValue(item, label, boolanPopup);
                }
                else if (PropertyUtility.CheckPropertyInEnum(target.GetType(), item.property))
                {
                    HelperUtility.DrawHelperWarning(item.property, displayTexture != null, boolanPopup != null, defaultRange != null || propertyRange != null);
                    DrawingUtility.PropertyFieldInEnum(item, label,enumDisplayNames);
                }
                else if (PropertyUtility.CheckPropertyInTexture((item.member as FieldInfo).FieldType, displayTexture))
                {
                    HelperUtility.DrawHelperWarning(item.property, false, boolanPopup != null, defaultRange != null || propertyRange != null);
                    DrawingUtility.PropertyFieldInTexture(displayTexture,item,label);
                }
                else
                {
                    HelperUtility.DrawHelperWarning(item.property, displayTexture != null, boolanPopup != null, (defaultRange != null || propertyRange != null) && listDrawerSetting == null);
                    DrawingUtility.PropertyField(item, label,listPairs);
                }
                EditorGUI.EndDisabledGroup();
            }
            else item.active = false;
            GUI.color = Color.white;
        }

        private void SerializeMethod(MethodData methodData)
        {
            methodData.method.CreateAllSettingAttribute
               (out _
               , out GUIColorAttribute color, out EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
               , out DisableEnumValueIfAttribute[] disable, out EnableIfAttribute[] enableIf, out DisableIfAttribute[] disableIf
               , out HelperBoxAttribute helperBox
               , out _, out _
               , out _, out _
               , out RuntimeDisabledGroupAttribute runtimeDisabledGroup, out EditorDisabledGroupAttribute editorDisabledGroup
               , out _, out _, out var disableGroupIf, out var disableGroupEnumValueIf, out _);      
            if (ConditionUtility.DrawConditionIf(enableEnumValueIfAttribute, enableIf, disable, disableIf, target.GetType(), target))
            {
                var Method = methodData.methodButton;
                GUI.color = color != null ? color.Color : Color.white;
                EditorGUI.BeginDisabledGroup(ConditionUtility.DisableGroupLifeCycle(runtimeDisabledGroup, editorDisabledGroup) || ConditionUtility.DisableGroupInValue(target.GetType(), target, disableGroupEnumValueIf, disableGroupIf));
                if (helperBox != null)
                    DrawingUtility.PropertyFieldInHelperBox(helperBox);

                ParameterInfo[] parameterInfos = methodData.method.GetParameters();

                string label = string.IsNullOrEmpty(Method.Label) ? methodData.method.Name : Method.Label;
                bool executed = false;
               
                if (parameterInfos.Length != 0)
                {
                    EditorGUILayout.BeginVertical("PreferencesSectionBox", GUILayout.Height(Method.Height + 3));
                    EditorGUILayout.BeginHorizontal();
                    methodData.foldOut = EditorGUILayout.Foldout(methodData.foldOut, label, true);
                    executed = GUILayout.Button("Invoke", GUILayout.Width(60));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    executed = GUILayout.Button(label, GUILayout.Height(Method.Height));
                }
                if (methodData.foldOut)
                {
                    EditorGUILayout.BeginVertical("OL box flat");
                    for(int i = 0; i < parameterInfos.Length;i++)
                    { 
                        var parameter = parameterInfos[i];
                        string name = parameter.Name.ToUpper()[0] + parameter.Name.Substring(1);
                        var list = methodData.reoderableInfos.Find(x => x.index == i && x.type == parameter.ParameterType);
                        if (list != null && list.reorderableList != null)
                        {
                            var newRect = EditorGUILayout.BeginHorizontal("OL box NoExpand");
                            GUILayout.Label(" ");
                            list.foldOut = EditorGUILayout.Foldout(list.foldOut, parameter.Name);
                            GUIStyle nameStype = new GUIStyle();
                            nameStype.fontStyle = FontStyle.Bold;
                            nameStype.alignment = TextAnchor.MiddleCenter;
                            nameStype.fontSize = 22;
                            nameStype.fixedHeight = 18;
                            nameStype.fixedWidth = 25;
                            nameStype.normal.textColor = Color.grey;
                            GUI.Box(new Rect(newRect) { x = newRect.xMax - 28, width = 1 }, string.Empty, "LODBlackBox");                          
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("+", nameStype))
                            {
                                var type = list.type;
                                Type elementType = null;
                                if (type.IsArray)
                                    elementType = type.GetElementType();
                                else if (type.IsGenericType)
                                    elementType = type.GetGenericArguments()[0];

                                if (elementType == null) continue;
                                bool subObject = elementType.IsSubclassOf(typeof(UnityEngine.Object)) || elementType.Equals(typeof(UnityEngine.Object));
                                if (type.IsArray)
                                {
                                    var tempList = Array.CreateInstance(elementType, list.reorderableList.list.Count + 1);
                                    Array.Copy((Array)list.reorderableList.list, tempList, list.reorderableList.list.Count);
                                    list.reorderableList.list = tempList;
                                }
                                else if (type.IsGenericType)
                                {
                                    if (subObject)
                                        list.reorderableList.list.Add(null);
                                    else list.reorderableList.list.Add(Activator.CreateInstance(elementType));
                                }
                            }
                            else if (newRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                            {
                                list.foldOut = !list.foldOut;
                            }
                            EditorGUILayout.EndHorizontal();
                            if (list.foldOut)
                                list.reorderableList.DoLayoutList();
                            methodData.args[i] = list.reorderableList.list;
                        }
                        else if (parameter.ParameterType.GetCustomAttribute<SerializableAttribute>() != null && parameter.ParameterType.IsClass && parameter.ParameterType != typeof(string))
                        {                         
                            EditorGUILayout.HelpBox("方法参数不支持对标记Serializable的类的序列化!", MessageType.Warning);
                        }
                        else if (parameter.ParameterType.IsSubclassOf(typeof(Enum)) || parameter.ParameterType.Equals(typeof(Enum)))
                        {
                            methodData.displayNames.Clear();
                            foreach (var field in parameter.ParameterType.GetRuntimeFields())
                            {
                                if (field.Name.StartsWith("value_")) continue;
                                var enumName = field.Name;

                                var enumType = field.FieldType.GetField(enumName);
                                var labelAttributes = enumType.GetCustomAttributes(typeof(LabelAttribute), false).ToArray();
                                methodData.displayNames.Add(labelAttributes.Length > 0 ? (labelAttributes[0] as LabelAttribute).Label : enumName);
                            }


                            methodData.displayIndex = EditorGUILayout.Popup(name, methodData.displayIndex, methodData.displayNames.ToArray());
                            methodData.args[i] = Enum.GetValues(parameter.ParameterType).GetValue(methodData.displayIndex);
                        }
                        else
                            methodData.args[i] = DrawingUtility.PropertyField(name, methodData.args[i], parameter.ParameterType);                       
                    }
                    EditorGUILayout.EndVertical();
                }

                if (executed)
                {
                    methodData.method.Invoke(target,methodData.args);
                }
                EditorGUILayout.EndVertical();           
                EditorGUI.EndDisabledGroup();
                GUI.color = Color.white;
            }
        }

        private void SetGroup(GUIGroupAttribute group, MemberInfo info,SerializedProperty property,Type type)
        {          
            if (infoDataPairs.ContainsKey(group.GroupName))
            { }
            else
                infoDataPairs.Add(group.GroupName, new List<InfoData>());

            if (infoDataPairs[group.GroupName].Find(x => x.member == info || x.property == property) != null)
            {
                Debug.Log("存在这个变量");
                return;
            }
            var data = new InfoData()
            {
                member = info,
                property = property,
                config = config
            };
            var label = info.GetCustomAttribute<LabelAttribute>(true);
            var listDrawerSetting = info.GetCustomAttribute<ListDrawerSettingAttribute>(true);
            if (PropertyUtility.CheckPropertyInGeneric(type) && listDrawerSetting != null)
            {               
                var list = new ReorderableList(serializedObject, property, true, true, false, false);
                DrawingUtility.SetReoderableList(data, list, listDrawerSetting,target,target.GetType());
                listPairs[data.member.Name] = list;
            }
            infoDataPairs[group.GroupName].Add(data);
        }
    }
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class CustomScriptableObjectInspectorEditor : CustomInspectorEditor
    {
        
    }
#endif
}
