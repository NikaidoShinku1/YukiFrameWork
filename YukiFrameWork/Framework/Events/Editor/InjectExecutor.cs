///=====================================================
/// - FileName:      InjectExecutor.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/22 14:47:00
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Mono.Cecil;
using System.Collections.Generic;

using EventDefinition = System.Collections.Generic.Dictionary<Mono.Cecil.TypeDefinition, System.Collections.Generic.List<Mono.Cecil.MethodDefinition>>;
using System.Linq;
using Mono.Cecil.Cil;
using System.Text;
namespace YukiFrameWork.Events
{
	public class InjectExecutor
	{
        internal TypeDefinition definition;
        internal AssemblyDefinition assemblyDefinition;

        public MethodDefinition Customize_op_Implicit_
        {
            get
            {
                var _op_Implicit = this.definition.Methods.FirstOrDefault(m => m.Name == "op_Implicit" && m.ReturnType.FullName == "System.Boolean");
                return _op_Implicit;
            }
        }
        public InjectExecutor(TypeDefinition definition, AssemblyDefinition assemblyDefinition)
        {
            this.definition = definition;            
            this.assemblyDefinition = assemblyDefinition;
        }

        private bool? _isMono;
        public bool isMono
        {
            get
            {
                if (this._isMono == null)
                {
                    this._isMono = this.IsMono(this.definition);
                }
                return this._isMono.Value;
            }
        }
        internal bool IsMono(TypeDefinition type)
		{
			TypeDefinition current = type;

			while (current.BaseType != null)
			{
				var baseType = current.BaseType;
				if (baseType.FullName == typeof(MonoBehaviour).FullName)
				{
					return true;
				}
                try
                {
                    current = baseType?.Resolve(); 
                }
                catch { break; }
			}

			return false;
		}

		internal  EventDefinition default_event_Dicts
			= new EventDefinition();

		internal  EventDefinition static_event_Dicts
			= new EventDefinition();

		internal  EventDefinition mono_event_Dicts
			= new EventDefinition();

        internal  EventDefinition task_default_event_Dicts
            = new EventDefinition();

        internal  EventDefinition task_static_event_Dicts
            = new EventDefinition();

        internal  EventDefinition task_mono_event_Dicts
            = new EventDefinition();

        internal Dictionary<MethodDefinition,List<TypeDefinition>> remove_event_Dicts
            = new Dictionary<MethodDefinition, List<TypeDefinition>>();

        private  EventDefinition GetEventDefinition(bool isStatic,bool onlyMonoEnable)
		{
			if (isStatic) return static_event_Dicts;
			else if (onlyMonoEnable) return mono_event_Dicts;

			return default_event_Dicts;
		}

        private  EventDefinition GetTaskEventDefinition(bool isStatic, bool onlyMonoEnable)
        {
            if (isStatic) return task_static_event_Dicts;
            else if (onlyMonoEnable) return task_mono_event_Dicts;

            return task_default_event_Dicts;
        }

        public string GetMethodInfos(out int count)
        {
            StringBuilder builder = new StringBuilder();
            HashSet<TypeDefinition> types = new HashSet<TypeDefinition>();
            types.UnionWith(GetAllStaticEventAndTask());
            types.UnionWith(GetAllInstanceEventAndTask());
            count = 0;
            foreach (var type in types) 
            {
                List<MethodDefinition> lists = new List<MethodDefinition>();
                {
                    var list = GetEventDefinitions(type, true, false);
                    if (list != null)
                        lists.AddRange(list);
                }
                {
                    var list = GetEventDefinitions(type, false, false);
                    if (list != null)
                        lists.AddRange(list);
                }
                {
                    var list = GetEventDefinitions(type, false, true);
                    if (list != null)
                        lists.AddRange(list);
                }
               

                foreach (var method in lists)
                {
                    builder.AppendLine($"Event =====> {method.FullName}");
                }
                count += lists.Count;
            }
            return builder.ToString();
        }

        public string GetRemoveMethodInfos(out int count)
        {
            count = 0;
            StringBuilder builder = new StringBuilder();
            foreach (var item in remove_event_Dicts)
            {                
                count++;
                builder.AppendLine($"Listener Event =====>{item.Key.FullName}");
                foreach (var type in item.Value)
                {
                    builder.AppendLine($"RemoveListener Type:{type.FullName}");
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public IEnumerable<TypeDefinition> GetAllStaticEventAndTask()
        {
            var ret = new List<TypeDefinition>();
            ret.AddRange(this.static_event_Dicts.Keys);
            ret.AddRange(this.task_static_event_Dicts.Keys);
            return ret;
        }

        public IEnumerable<MethodDefinition> GetEventDefinitions(TypeDefinition eventType, bool isStatic, bool CallOnlyIfMonoEnable)
        {
            var ret = new List<MethodDefinition>();

            var eventCollection = this.GetEventDefinition(isStatic, CallOnlyIfMonoEnable);
            if (eventCollection.ContainsKey(eventType))
            {
                ret.AddRange(eventCollection[eventType]);
            }
            var taskCollection = this.GetTaskEventDefinition(isStatic, CallOnlyIfMonoEnable);
            if (taskCollection.ContainsKey(eventType))
            {
                ret.AddRange(taskCollection[eventType]);
            }

            if (ret.Count == 0)
            {
                return null;
            }
            else
            {
                return ret;
            }
        }

        public IEnumerable<TypeDefinition> GetAllInstanceEventAndTask()
        {
            var ret = new HashSet<TypeDefinition>();
            foreach (var eventType in this.default_event_Dicts.Keys)
            {
                ret.Add(eventType);
            }
            foreach (var eventType in this.mono_event_Dicts.Keys)
            {
                if (ret.Contains(eventType)) continue;
                ret.Add(eventType);
            }
            foreach (var eventType in this.task_default_event_Dicts.Keys)
            {
                ret.Add(eventType);
            }
            foreach (var eventType in this.task_mono_event_Dicts.Keys)
            {
                if (ret.Contains(eventType)) continue;
                ret.Add(eventType);
            }
            return ret;
        }

        public  void AddListener(MethodDefinition method, bool onlyMonoEnable)
		{
			AddListener_All_Internal(method, GetEventDefinition(method.IsStatic, onlyMonoEnable));
		}

		public  void AddListener_Async(MethodDefinition method, bool onlyMonoEnable)
		{
            AddListener_All_Internal(method, GetTaskEventDefinition(method.IsStatic, onlyMonoEnable));
        }

        private bool IsCheckParameterType(TypeDefinition type)
        {
            try
            {
                if (type.FullName == typeof(IEventArgs).FullName)
                    return true;
                foreach (var item in type.NestedTypes)
                {
                    if (IsCheckParameterType(item))
                        return true;
                }

                foreach (var interfaces in type.Interfaces)
                {
                    if (interfaces.InterfaceType.FullName == typeof(IEventArgs).FullName)
                        return true;
                }

                return false;
            }
            catch 
            {
                return false;
            }
        }

        public void RemoveListener(MethodDefinition method,CustomAttribute removeAttribute)
        {
            try
            {
                CustomAttributeArgument[] item = (CustomAttributeArgument[])removeAttribute.ConstructorArguments.FirstOrDefault().Value;
                foreach (var i in item)
                {
                    if (i.Value == null) continue;

                    TypeDefinition type = i.Value as TypeDefinition;

                    if (!IsCheckParameterType(type))
                        continue;
                    if (type == null) continue;
                    if (method.IsStatic) continue;
                    if (method.Body == null) continue;

                    if (!remove_event_Dicts.TryGetValue(method, out List<TypeDefinition> types))
                    {
                        types = new List<TypeDefinition>();
                        remove_event_Dicts[method] = types;
                    }                   
                    types.Add(type);

                }
            }
            catch
            {
                Debug.Log($"检测到{definition.Name}类中{method.Name}方法{nameof(EventManager.RemoveAllListeners)}特性中Type为空:将不保留可注销类型，无法进行注入".Color(Color.red));
                remove_event_Dicts.Clear();
            }
            
        }

        internal void AddListener_All_Internal(MethodDefinition method, EventDefinition events) 
        {
            TypeDefinition type = method.Parameters[0].ParameterType.Resolve();
            if (!events.TryGetValue(type, out List<MethodDefinition> methods))
            {
                methods = new List<MethodDefinition>();
                events[type] = methods;
            }
            methods.Add(method);          
        }
        internal static Dictionary<TypeDefinition, InjectExecutor> Executors = new Dictionary<TypeDefinition, InjectExecutor>();
        public static InjectExecutor GetOrCreate(TypeDefinition definition,AssemblyDefinition assemblyDefinition)
        {
            if (!Executors.ContainsKey(definition))
                Executors[definition] = new InjectExecutor(definition,assemblyDefinition);

            return Executors[definition];
        }

        public static void ForEach(Action<InjectExecutor> action)
        {
            foreach (var ex in Executors.Values)
            {
                action?.Invoke(ex);
            }
        }

        public void Init()
        {
            var InjectedNameSpace = "YukiFrameWork";
            var InjectedClazz = "Event_Builder";

            var injectedFullName = $"{InjectedNameSpace}.{InjectedClazz}";
            var has = assemblyDefinition.MainModule.Types.FirstOrDefault(t => t.FullName == injectedFullName);
            if (has == null)
            {
                var typeAttri = Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Public;
                var baseType = assemblyDefinition.MainModule.TypeSystem.Object;

                var injectedTypeDef = new TypeDefinition(InjectedNameSpace, InjectedClazz, typeAttri, baseType);

                var PreserveCtor = typeof(UnityEngine.Scripting.PreserveAttribute).GetConstructors()[0];
                var Preserve = new CustomAttribute(assemblyDefinition.MainModule.ImportReference(PreserveCtor));
                injectedTypeDef.CustomAttributes.Add(Preserve);               
                assemblyDefinition.MainModule.Types.Add(injectedTypeDef);

                this.newBridgeType = injectedTypeDef;
            }
            else
            {
                this.injectedBridge = true;
                this.newBridgeType = has;
            }        
            NewGenerate_EventBuilder();
            NewGenerate_CTOR();
            NewGenerate_StaticRegister();
        }
        internal TypeDefinition newBridgeType;
        internal bool injectedBridge;        
        private void NewGenerate_EventBuilder()
        {
            if (this.injectedBridge) return;
            var invokerTypeRef = assemblyDefinition.MainModule.ImportReference(typeof(IEventBuilder));
            var invoker = new InterfaceImplementation(invokerTypeRef);
            this.newBridgeType.Interfaces.Add(invoker);
        }

        private void NewGenerate_CTOR()
        {
            if (this.injectedBridge) return;          
            // 实现构造函数
            var CTOR_Name = ".ctor";  
           
            var CTOR_Attri = Mono.Cecil.MethodAttributes.Public;
            CTOR_Attri |= Mono.Cecil.MethodAttributes.HideBySig;
            CTOR_Attri |= Mono.Cecil.MethodAttributes.SpecialName;
            CTOR_Attri |= Mono.Cecil.MethodAttributes.RTSpecialName;

            var CTOR_Ret = assemblyDefinition.MainModule.ImportReference(typeof(void));

            var CTOR = new MethodDefinition(CTOR_Name, CTOR_Attri, CTOR_Ret);

            {
                var PreserveCtor = typeof(UnityEngine.Scripting.PreserveAttribute).GetConstructors()[0];
                var Preserve = new CustomAttribute(assemblyDefinition.MainModule.ImportReference(PreserveCtor));
                CTOR.CustomAttributes.Add(Preserve);
            }

            var ilProcessor = CTOR.Body.GetILProcessor();
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
            var obj_ctor = typeof(object).GetConstructor(new Type[] { });
            var obj_ctor_Ref = assemblyDefinition.MainModule.ImportReference(obj_ctor);
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call, obj_ctor_Ref));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

            this.newBridgeType.Methods.Add(CTOR);
        }
        private void NewGenerate_StaticRegister()
        {
            // 添加 Static Init
            var staticRegisterName = "StaticInit";
            var has = this.newBridgeType.Methods.FirstOrDefault(m => m.Name == staticRegisterName);
            if (has != null)
            {
                this.newStaticRegisterMethod = has;
                return;
            }

            var staticRegisterAttri = Mono.Cecil.MethodAttributes.Public;
            staticRegisterAttri |= Mono.Cecil.MethodAttributes.HideBySig;
            staticRegisterAttri |= Mono.Cecil.MethodAttributes.NewSlot;
            staticRegisterAttri |= Mono.Cecil.MethodAttributes.Virtual;
            staticRegisterAttri |= Mono.Cecil.MethodAttributes.Final;

            var staticRegisterRet = assemblyDefinition.MainModule.ImportReference(typeof(void));

            var staticRegisterMethod = new MethodDefinition(staticRegisterName, staticRegisterAttri, staticRegisterRet);

            var ilProcesser = staticRegisterMethod.Body.GetILProcessor();
            ilProcesser.Append(ilProcesser.Create(OpCodes.Ret));

            this.newBridgeType.Methods.Add(staticRegisterMethod);
            this.newStaticRegisterMethod = staticRegisterMethod;

        }
        internal MethodDefinition newStaticRegisterMethod;

    }
}
