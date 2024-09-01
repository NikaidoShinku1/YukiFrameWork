namespace YukiFrameWork.ActionStates
{   
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using YukiFrameWork.Extension;
    using Object = UnityEngine.Object;

    public enum TypeCode
    {
        Empty = 0,
        Object = 1,
        DBNull = 2,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 0xF,
        DateTime = 0x10,
        String = 18,
        Vector2,
        Vector3,
        Vector4,
        Rect,
        Color,
        Color32,
        Quaternion,
        AnimationCurve,
        GenericType,
        Array,
        Enum,
    }

    [Serializable]
    public class Metadata
    {
        public string name;
        public TypeCode type;
        public string typeName;
        public string data;
        public object target;
        public FieldInfo field;
        public Object Value;
        public List<Object> values;
        public List<Object> Values
        {
            get
            {
                values ??= new List<Object>();
                return values;
            }
            set { values = value; }
        }
        private object _value;
        public object value
        {
            get
            {
                if (target != null & field != null)
                    _value = field.GetValue(target);
                if (_value == null)
                    _value = Read();
                return _value;
            }
            set
            {
                _value = value;
                if (target != null & field != null)
                    field.SetValue(target, _value);
                Write(_value);
            }
        }
        private Type _type;
        public Type Type
        {
            get
            {
                if (_type == null)
                    _type = AssemblyHelper.GetType(typeName);
                return _type;
            }
        }
        public Type _itemType;
        public Type itemType
        {
            get
            {
                if (_itemType == null)
                    if (!GenericTypeArguments.TryGetValue(Type, out _itemType))
                        GenericTypeArguments.Add(Type, _itemType = Type.GetInterface(typeof(IList<>).FullName).GenericTypeArguments[0]);
                return _itemType;
            }
        }
        public int arraySize;
        public bool foldout;

        static readonly Dictionary<Type, Type> GenericTypeArguments = new Dictionary<Type, Type>();

        public Metadata() { }
        public Metadata(string name, string fullName, TypeCode type, object target, FieldInfo field)
        {
            this.name = name;
            typeName = fullName;
            this.type = type;
            this.field = field;
            this.target = target;
            Write(value);
        }

        public object Read()
        {
            switch (type)
            {
                case TypeCode.Byte:
                    return Convert.ToByte(data);
                case TypeCode.SByte:
                    return Convert.ToSByte(data);
                case TypeCode.Boolean:
                    return Convert.ToBoolean(data);
                case TypeCode.Int16:
                    return Convert.ToInt16(data);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(data);
                case TypeCode.Char:
                    return Convert.ToChar(data);
                case TypeCode.Int32:
                    return Convert.ToInt32(data);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(data);
                case TypeCode.Single:
                    return Convert.ToSingle(data);
                case TypeCode.Int64:
                    return Convert.ToInt64(data);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(data);
                case TypeCode.Double:
                    return Convert.ToDouble(data);
                case TypeCode.String:
                    return data;
                case TypeCode.Enum:
                    return Enum.Parse(Type, data);
                case TypeCode.Object:
                    if (Value == null)
                        return null;
                    return Value;
                case TypeCode.Vector2:
                    var datas = data.Split(',');
                    return new Vector2(float.Parse(datas[0]), float.Parse(datas[1]));
                case TypeCode.Vector3:
                    var datas1 = data.Split(',');
                    return new Vector3(float.Parse(datas1[0]), float.Parse(datas1[1]), float.Parse(datas1[2]));
                case TypeCode.Vector4:
                    var datas2 = data.Split(',');
                    return new Vector4(float.Parse(datas2[0]), float.Parse(datas2[1]), float.Parse(datas2[2]), float.Parse(datas2[2]));
                case TypeCode.Quaternion:
                    var datas3 = data.Split(',');
                    return new Quaternion(float.Parse(datas3[0]), float.Parse(datas3[1]), float.Parse(datas3[2]), float.Parse(datas3[2]));
                case TypeCode.Rect:
                    var datas4 = data.Split(',');
                    return new Rect(float.Parse(datas4[0]), float.Parse(datas4[1]), float.Parse(datas4[2]), float.Parse(datas4[2]));
                case TypeCode.Color:
                    var datas5 = data.Split(',');
                    return new Color(float.Parse(datas5[0]), float.Parse(datas5[1]), float.Parse(datas5[2]), float.Parse(datas5[2]));
                case TypeCode.Color32:
                    var datas6 = data.Split(',');
                    return new Color32(byte.Parse(datas6[0]), byte.Parse(datas6[1]), byte.Parse(datas6[2]), byte.Parse(datas6[2]));
                case TypeCode.GenericType:
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        IList list = (IList)Activator.CreateInstance(Type);
                        for (int i = 0; i < Values.Count; i++)
                        {
                            if (Values[i] == null)
                                list.Add(null);
                            else
                                list.Add(Values[i]);
                        }
                        return list;
                    }
                    else return SerializationTool.DeserializedObject(data, Type);
                case TypeCode.Array:
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        IList list = Array.CreateInstance(itemType, Values.Count);
                        for (int i = 0; i < Values.Count; i++)
                        {
                            if (Values[i] == null) continue;
                            list[i] = Values[i];
                        }
                        return list;
                    }
                    else return SerializationTool.DeserializedObject(data, Type);
            }
            return null;
        }

        public void Write(object value)
        {
            if (type == TypeCode.Object)
            {
                Value = (Object)value;
            }
            else if (value != null)
            {
                if (type == TypeCode.Vector2)
                {
                    Vector2 v2 = (Vector2)value;
                    data = $"{v2.x},{v2.y}";
                }
                else if (type == TypeCode.Vector3)
                {
                    Vector3 v = (Vector3)value;
                    data = $"{v.x},{v.y},{v.z}";
                }
                else if (type == TypeCode.Vector4)
                {
                    Vector4 v = (Vector4)value;
                    data = $"{v.x},{v.y},{v.z},{v.w}";
                }
                else if (type == TypeCode.Quaternion)
                {
                    Quaternion v = (Quaternion)value;
                    data = $"{v.x},{v.y},{v.z},{v.w}";
                }
                else if (type == TypeCode.Rect)
                {
                    Rect v = (Rect)value;
                    data = $"{v.x},{v.y},{v.width},{v.height}";
                }
                else if (type == TypeCode.Color)
                {
                    Color v = (Color)value;
                    data = $"{v.r},{v.g},{v.b},{v.a}";
                }
                else if (type == TypeCode.Color32)
                {
                    Color32 v = (Color32)value;
                    data = $"{v.r},{v.g},{v.b},{v.a}";
                }
                else if (type == TypeCode.GenericType | type == TypeCode.Array)
                {
                    if (itemType == typeof(Object) | itemType.IsSubclassOf(typeof(Object)))
                    {
                        Values.Clear();
                        IList list = (IList)value;
                        for (int i = 0; i < list.Count; i++)
                            Values.Add(list[i] as Object);
                    }
                    else data = SerializationTool.SerializedObject(value);
                }
                else data = value.ToString();
            }
            else
            {
                data = null;
            }
        }
    }

    /// <summary>
    /// 状态机脚本不显示的字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class HideField : Attribute { }


    /// <summary>
    /// 状态行为基类
    /// </summary>
    [Serializable]
    public class BehaviourBase : AbstractController
    {
        [HideField]
        public string name;
        [HideField]
        public int ID;
        /// <summary>
        /// 展开编辑器检视面板
        /// </summary>       
        [HideInInspector]
        public bool show = true;
        /// <summary>
        /// 脚本是否启用?
        /// </summary>
        [HideField]
        public bool Active = true;
        [HideField]
        public List<Metadata> metadatas;
        public List<Metadata> Metadatas
        {
            get
            {
                metadatas ??= new List<Metadata>();
                return metadatas;
            }
            set { metadatas = value; }
        }
        public IStateMachine stateMachine;
        /// <summary>
        /// 当前状态
        /// </summary>
        public State state => stateMachine.States[ID];
        /// <summary>
        /// 当前状态机挂载在物体的父转换对象
        /// </summary>
        public Transform transform => stateMachine.transform;
        public Type Type { get { return AssemblyHelper.GetType(name); } }
        public void InitMetadatas()
        {
            var type = GetType();
            InitMetadatas(type);
        }
        public void InitMetadatas(Type type)
        {          
            name = type.ToString();
            var fields = type.GetRuntimeFields()
                .Where(field => !(field.IsStatic | field.HasCustomAttribute<HideInInspector>() | !field.IsPublic | field.HasCustomAttribute<HideField>()) 
                || field.HasCustomAttribute<SerializeField>());
            Metadatas.Clear();
            foreach (var field in fields)
            {     
                InitField(field);
            }
        }

        private void InitField(FieldInfo field)
        {
            var code = Type.GetTypeCode(field.FieldType);
            if (code == System.TypeCode.Object)
            {
                if (field.FieldType.IsSubclassOf(typeof(Object)) | field.FieldType == typeof(Object))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Object, this, field));
                else if (field.FieldType == typeof(Vector2))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector2, this, field));
                else if (field.FieldType == typeof(Vector3))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector3, this, field));
                else if (field.FieldType == typeof(Vector4))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Vector4, this, field));
                else if (field.FieldType == typeof(Quaternion))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Quaternion, this, field));
                else if (field.FieldType == typeof(Rect))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Rect, this, field));
                else if (field.FieldType == typeof(Color))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Color, this, field));
                else if (field.FieldType == typeof(Color32))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Color32, this, field));
                else if (field.FieldType == typeof(AnimationCurve))
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.AnimationCurve, this, field));
                else if (field.FieldType.IsGenericType)
                {
                    var gta = field.FieldType.GenericTypeArguments;
                    if (gta.Length > 1)
                        return;
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.GenericType, this, field));
                }
                else if (field.FieldType.IsArray)
                    Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Array, this, field));
            }
            else if (field.FieldType.IsEnum)
                Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), TypeCode.Enum, this, field));
            else Metadatas.Add(new Metadata(field.Name, field.FieldType.ToString(), (TypeCode)code, this, field));
        }

        public void Reload(Type type, List<Metadata> metadatas)
        {
            InitMetadatas(type);
            foreach (var item in Metadatas)
            {
                foreach (var item1 in metadatas)
                {
                    if (item.name == item1.name & item.typeName == item1.typeName)
                    {
                        item.data = item1.data;
                        item.Value = item1.Value;
                        item.Values = item1.Values;
                        item.arraySize = item1.arraySize;
                        item.foldout = item1.foldout;
                        item.field.SetValue(this, item1.value);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 当初始化调用
        /// </summary>
        public override void OnInit() { }

        /// <summary>
        /// 当组件被删除调用一次
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// 当绘制编辑器检视面板 (重要提示!你想自定义编辑器检视面板则返回真,否则显示默认编辑器检视面板)
        /// </summary>
        /// <param name="state">当前状态</param>
        /// <returns></returns>
        public virtual bool OnInspectorGUI(State state)
        {
            return false; //返回假: 绘制默认监视面板 | 返回真: 绘制扩展自定义监视面板
        }

        /// <summary>
        /// 进入下一个状态, 如果状态正在播放就不做任何处理, 如果想让动作立即播放可以使用 OnEnterNextState 方法
        /// </summary>
        /// <param name="stateID"></param>
        public void EnterState(int stateID, int actionId = 0) => stateMachine.StatusEntry(stateID, actionId);

        /// <summary>
        /// 当进入下一个状态, 你也可以立即进入当前播放的状态, 如果不想进入当前播放的状态, 使用StatusEntry方法
        /// </summary>
        /// <param name="stateID">下一个状态的ID</param>
        public void OnEnterNextState(int stateID, int actionId = 0) => stateMachine.EnterNextState(stateID, actionId);

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="force"></param>
        public void ChangeState(int stateId, int actionId = 0, bool force = false) => stateMachine.ChangeState(stateId, actionId, force);     

        /// <summary>
        /// 初始化真实类型并且赋值记录的值
        /// </summary>
        /// <returns></returns>
        public BehaviourBase InitBehaviour(IStateMachine stateMachine)
        {
            var type = AssemblyHelper.GetType(name);
            var runtimeBehaviour = (BehaviourBase)Activator.CreateInstance(type);
            runtimeBehaviour.stateMachine = stateMachine;
            runtimeBehaviour.Active = Active;
            runtimeBehaviour.ID = ID;
            runtimeBehaviour.name = name;
            runtimeBehaviour.Metadatas = Metadatas;
            runtimeBehaviour.show = show;
            foreach (var metadata in Metadatas)
            {
                var field = type.GetField(metadata.name);
                if (field == null)
                    continue;
                var value = metadata.Read();//必须先读值才能赋值下面字段和对象
                metadata.field = field;
                metadata.target = runtimeBehaviour;
                field.SetValue(runtimeBehaviour, value);
            }
            var lateUpdateMethod = type.GetMethod("OnLateUpdate");
            var fixedUpdateMethod = type.GetMethod("OnFixedUpdate");
            var root = stateMachine;
            while (root != null)
            {
                if (root.Parent == null) //最后一层
                    break;
                root = root.Parent;
            }
           
            if ((lateUpdateMethod.DeclaringType == type) && (stateMachine.UpdateStatus & UpdateStatus.OnLateUpdate) == 0)
                root.UpdateStatus |= UpdateStatus.OnLateUpdate;
            if ((fixedUpdateMethod.DeclaringType == type) && (stateMachine.UpdateStatus & UpdateStatus.OnFixedUpdate) == 0)
                root.UpdateStatus |= UpdateStatus.OnFixedUpdate;    
            return runtimeBehaviour;
        }
    }
}