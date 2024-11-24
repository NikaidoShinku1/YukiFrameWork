///=====================================================
/// - FileName:      BehaviourParam.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/14 16:06:37
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections;
using YukiFrameWork.Events;
using UnityEngine.Events;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
#endif
namespace YukiFrameWork.Behaviours
{
    public enum BehaviourParamType
    {
        Int,
        Float,
        Bool,      
        String,
        Vector2,
        Vector3,
        Vector4,
        GameObject,
        BoolList,
        IntList,
        FloatList,
        StringList,
        Vector2List,
        Vector3List,
        Vector4List,   
        GameObjectList,
        UnityEvent,
    }
	[Serializable]
	public class BehaviourParam
	{
        internal static Dictionary<Type, BehaviourParamType> AllParamTypes = new Dictionary<Type, BehaviourParamType>()
        {
            [typeof(int)] = BehaviourParamType.Int,
            [typeof(float)] = BehaviourParamType.Float,
            [typeof(bool)] = BehaviourParamType.Bool,
            [typeof(string)] = BehaviourParamType.String,
            [typeof(Vector2)] = BehaviourParamType.Vector2,
            [typeof(Vector3)] = BehaviourParamType.Vector3,
            [typeof(Vector4)] = BehaviourParamType.Vector4,
            [typeof(GameObject)] = BehaviourParamType.GameObject,
            [typeof(List<bool>)] = BehaviourParamType.BoolList,
            [typeof(List<int>)] = BehaviourParamType.IntList,
            [typeof(List<float>)] = BehaviourParamType.FloatList,
            [typeof(List<string>)] = BehaviourParamType.StringList,
            [typeof(List<Vector2>)] = BehaviourParamType.Vector2List,
            [typeof(List<Vector3>)] = BehaviourParamType.Vector3List,
            [typeof(List<Vector4>)] = BehaviourParamType.Vector4List,
            [typeof(List<GameObject>)] = BehaviourParamType.GameObjectList,
            [typeof(UnityEvent<AIBehaviour>)] = BehaviourParamType.UnityEvent,
            [typeof(bool[])] = BehaviourParamType.BoolList,
            [typeof(int[])] = BehaviourParamType.IntList,
            [typeof(float[])] = BehaviourParamType.FloatList,
            [typeof(string[])] = BehaviourParamType.StringList,
            [typeof(Vector2[])] = BehaviourParamType.Vector2List,
            [typeof(Vector3[])] = BehaviourParamType.Vector3List,
            [typeof(Vector4[])] = BehaviourParamType.Vector4List,
            [typeof(GameObject[])] = BehaviourParamType.GameObjectList,
        };
        [LabelText("参数类型")]
        public BehaviourParamType behaviourParamType;      
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.String)]private string stringValue;
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Int)]private int intValue;
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Float)]private float floatValue;
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Bool)]private bool boolValue;
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Vector2)]private Vector2 vector2Value;
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Vector3)]private Vector3 vector3Value;
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Vector4)]private Vector4 vector4Value;
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.GameObject)]private GameObject gameObjectValue;
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.BoolList)]private List<bool> boolLists = new List<bool>();
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.IntList)]private List<int> intLists = new List<int>();
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.FloatList)]private List<float> floatLists = new List<float>();
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.StringList)]private List<string> stringLists = new List<string>();
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Vector2List)]private List<Vector2> vector2Lists = new List<Vector2>();
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Vector3List)]private List<Vector3> vector3Lists = new List<Vector3>();
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.Vector4List)]private List<Vector4> vector4Lists = new List<Vector4>();
        [LabelText("参数本体"),SerializeField,ShowIf(nameof(behaviourParamType),BehaviourParamType.GameObjectList)]private List<GameObject> gameObjectList = new List<GameObject>();
        [LabelText("参数本体"),SerializeField, ShowIf(nameof(behaviourParamType), BehaviourParamType.UnityEvent)] private UnityEvent<AIBehaviour> unityEvent = new UnityEvent<AIBehaviour>();
        public string StringValue
        {
            get => behaviourParamType != BehaviourParamType.String ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : stringValue;
            set
            {
                if (behaviourParamType != BehaviourParamType.String) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                stringValue = value;
            }
        }

        public int IntValue
        {
            get => behaviourParamType != BehaviourParamType.Int ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : intValue;
            set
            {
                if (behaviourParamType != BehaviourParamType.Int) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                intValue = value;
            }
        }

        public float FloatValue
        {
            get => behaviourParamType != BehaviourParamType.Float ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : floatValue;
            set
            {
                if (behaviourParamType != BehaviourParamType.Float) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                floatValue = value;
            }
        }

        public bool BoolValue
        {
            get => behaviourParamType != BehaviourParamType.Bool ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : boolValue;
            set
            {
                if (behaviourParamType != BehaviourParamType.Bool) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                boolValue = value;
            }
        }

        public Vector2 Vector2Value
        {
            get => behaviourParamType != BehaviourParamType.Vector2 ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : vector2Value;
            set
            {
                if (behaviourParamType != BehaviourParamType.Vector2) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                vector2Value = value;
            }
        }

        public Vector3 Vector3Value
        {
            get => behaviourParamType != BehaviourParamType.Vector3 ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : vector3Value;
            set
            {
                if (behaviourParamType != BehaviourParamType.Vector3) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                vector3Value = value;
            }
        }

        public Vector4 Vector4Value
        {
            get => behaviourParamType != BehaviourParamType.Vector4 ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : vector4Value;
            set
            {
                if (behaviourParamType != BehaviourParamType.Vector4) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                vector4Value = value;
            }
        }

        public GameObject GameObjectValue
        {
            get => behaviourParamType != BehaviourParamType.GameObject ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : gameObjectValue;
            set
            {
                if (behaviourParamType != BehaviourParamType.GameObject) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                gameObjectValue = value;
            }
        }

        public List<bool> BoolLists
        {
            get => behaviourParamType != BehaviourParamType.BoolList ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : boolLists;
            set
            {
                if (behaviourParamType != BehaviourParamType.BoolList) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                boolLists = value;
            }
        }

        public List<int> IntLists
        {
            get => behaviourParamType != BehaviourParamType.IntList ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : intLists;
            set
            {
                if (behaviourParamType != BehaviourParamType.IntList) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                intLists = value;
            }
        }

        public List<float> FloatLists
        {
            get => behaviourParamType != BehaviourParamType.FloatList ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : floatLists;
            set
            {
                if (behaviourParamType != BehaviourParamType.FloatList) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                floatLists = value;
            }
        }

        public List<string> StringLists
        {
            get => behaviourParamType != BehaviourParamType.StringList ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : stringLists;
            set
            {
                if (behaviourParamType != BehaviourParamType.StringList) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                stringLists = value;
            }
        }

        public List<Vector2> Vector2Lists
        {
            get => behaviourParamType != BehaviourParamType.Vector2List ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : vector2Lists;
            set
            {
                if (behaviourParamType != BehaviourParamType.Vector2List) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                vector2Lists = value;
            }
        }

        public List<Vector3> Vector3Lists
        {
            get => behaviourParamType != BehaviourParamType.Vector3List ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : vector3Lists;
            set
            {
                if (behaviourParamType != BehaviourParamType.Vector3List) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                vector3Lists = value;
            }
        }

        public List<Vector4> Vector4Lists
        {
            get => behaviourParamType != BehaviourParamType.Vector4List ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : vector4Lists;
            set
            {
                if (behaviourParamType != BehaviourParamType.Vector4List) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                vector4Lists = value;
            }
        }

        public List<GameObject> GameObjectList
        {
            get => behaviourParamType != BehaviourParamType.GameObjectList ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : gameObjectList;
            set
            {
                if (behaviourParamType != BehaviourParamType.GameObjectList) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                gameObjectList = value;
            }
        }

        public UnityEvent<AIBehaviour> UnityEvent
        {
            get => behaviourParamType != BehaviourParamType.UnityEvent ? throw new Exception("参数类型不一致,Type:" + behaviourParamType) : unityEvent;
            set
            {
                if (behaviourParamType != BehaviourParamType.UnityEvent) throw new Exception("参数类型不一致,Type:" + behaviourParamType);
                unityEvent = value;
            }
        }

        public object Value
        {
            get
            {
                return behaviourParamType switch
                {
                    BehaviourParamType.Int => IntValue,
                    BehaviourParamType.Float => FloatValue,
                    BehaviourParamType.String => StringValue,
                    BehaviourParamType.Vector2 => Vector2Value,
                    BehaviourParamType.Vector3 => Vector3Value,
                    BehaviourParamType.Vector4 => Vector4Value,
                    BehaviourParamType.GameObject => GameObjectValue,
                    BehaviourParamType.Bool => BoolValue,
                    BehaviourParamType.BoolList => BoolLists,
                    BehaviourParamType.IntList => IntLists,
                    BehaviourParamType.FloatList => FloatLists,
                    BehaviourParamType.StringList => StringLists,
                    BehaviourParamType.Vector2List => Vector2Lists,
                    BehaviourParamType.Vector3List => Vector3Lists,
                    BehaviourParamType.Vector4List => Vector4Lists,
                    BehaviourParamType.GameObjectList => GameObjectList,
                    BehaviourParamType.UnityEvent => UnityEvent,
                    _ => default
                };
            }

        }    
    }
    /// <summary>
    /// 标记该特性，可让对应的字段自动在运行时赋值已有的参数，如果可序列化，此时会禁止对应字段在SO中Inspector的绘制并根据类型提示相应的提示/警告,同时可以在BehaviourTree组件中同步字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BehaviourParamAttribute : Attribute
    {
        
    }



#if UNITY_EDITOR
    public abstract class BehaviourDrawValue<T> : OdinAttributeDrawer<BehaviourParamAttribute, T>
    {
        private GUIStyle codeTextStyle;
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (codeTextStyle == null)
            {
                codeTextStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
                codeTextStyle.normal.textColor = ExpertCodeConfig.TextColor;
                codeTextStyle.active.textColor = ExpertCodeConfig.TextColor;
                codeTextStyle.focused.textColor = ExpertCodeConfig.TextColor;
                codeTextStyle.wordWrap = false;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label,GUILayout.Width(80));
            string info = string.Empty;
            MessageType messageType = MessageType.None;
            BehaviourParamAttribute attirbute = Attribute as BehaviourParamAttribute;

            if (Property.Info.GetMemberInfo() is System.Reflection.PropertyInfo)
            {
                messageType = MessageType.Error;
                info = $"不支持为属性添加[BehaviourParam]特性!";
            }
            
            else if (typeof(AIBehaviour).IsAssignableFrom(Property.Info.GetMemberInfo().DeclaringType))
            {
                if (BehaviourParam.AllParamTypes.ContainsKey(typeof(T)))
                {
                    info = $"已为该字段标记[BehaviourParam]特性";
                    messageType = MessageType.Info;
                }
                else
                {
                    info = $"标记字段并不是参数所兼容同步的类型，请检查! Field Type:{typeof(T)}";
                    messageType = MessageType.Warning;
                }
            }
            else
            {
                info = $"[BehaviourParam]特性不允许为非AIBehaviour类派生的类进行标记，这并不会有任何效果，且会禁止Inspector的序列化";
                messageType = MessageType.Error;
            }
           
            EditorGUILayout.HelpBox(info,messageType);
            EditorGUILayout.EndHorizontal();
        }
    }

    public class BehaviourIntDrawValue : BehaviourDrawValue<int>
    {
        
    }

    public class BehaviourStringDrawValue : BehaviourDrawValue<string>
    {

    }

    public class BehaviourBoolanDrawValue : BehaviourDrawValue<bool>
    {

    }

    public class BehaviourFloatDrawValue : BehaviourDrawValue<float>
    {
        
    }

    public class BehaviourVector2DrawValue : BehaviourDrawValue<Vector2>
    {
        
    }

    public class BehaviourVector3DrawValue : BehaviourDrawValue<Vector3>
    {
        
    }

    public class BehaviourVector4DrawValue : BehaviourDrawValue<Vector4>
    {
        
    }

    public class BehaviourGameObjectDrawValue : BehaviourDrawValue<GameObject>
    {
        
    }

    public class BehaviourIntListDrawValue : BehaviourDrawValue<List<int>>
    {
        
    }

    public class BehaviourIntArrayDrawValue : BehaviourDrawValue<int[]>
    {
        
    }

    public class BehaviourStringListDrawValue : BehaviourDrawValue<List<string>>
    {
        
    }

    public class BehaviourStringArrayDrawValue : BehaviourDrawValue<string[]>
    {
        
    }

    public class BehaviourBoolListDrawValue : BehaviourDrawValue<List<bool>> 
    {

    }

    public class BehaviourBoolArrayDrawValue : BehaviourDrawValue<bool[]>
    {

    }

    public class BehaviourFloatListDrawValue : BehaviourDrawValue<List<float>>
    {
        
    }

    public class BehaviourFloatArrayDrawValue : BehaviourDrawValue<float[]>
    {
        
    }

    public class BehaviourVector2ListDrawValue : BehaviourDrawValue<List<Vector2>>
    {
        
    }

    public class BehaviourVector2ArrayDrawValue : BehaviourDrawValue<Vector2[]>
    {
        
    }


    public class BehaviourVector3ListDrawValue : BehaviourDrawValue<List<Vector3>>
    {

    }

    public class BehaviourVector3ArrayDrawValue : BehaviourDrawValue<Vector3[]>
    {

    }


    public class BehaviourVector4ListDrawValue : BehaviourDrawValue<List<Vector4>>
    {

    }

    public class BehaviourVector4ArrayDrawValue : BehaviourDrawValue<Vector4[]>
    {

    }

    public class BehaviourGameObjectListDrawValue : BehaviourDrawValue<List<GameObject>>
    {

    }

    public class BehaviourGameObjectArrayDrawValue : BehaviourDrawValue<GameObject[]>
    {

    }

    public class BehaviourUnityEventListDrawValue : BehaviourDrawValue<UnityEvent<AIBehaviour>>
    {

    }

#endif
}
