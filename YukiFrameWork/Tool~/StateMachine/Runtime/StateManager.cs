///=====================================================
/// - FileName:      StateManager.cs
/// - NameSpace:     YukiFrameWork.StateMachine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 14:07:10
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Collections;
using YukiFrameWork.Extension;
using System.Linq;
using System.Collections.Generic;
namespace YukiFrameWork.Machine
{
    public enum StateInitializeType
    {
        Awake,
        Start
    }
    [DisableViewWarning]
    public class StateManager : SerializedMonoBehaviour,IController,IEnumerable<StateMachineCore>
    {
        [SerializeField,DictionaryDrawerSettings(KeyLabel = StateMachineConst.StateMachineKeyDisplay,ValueLabel = StateMachineConst.StateMachineValueDisplay)]
        [LabelText("运行时状态机配置")]
        [InfoBox(StateMachineConst.RuntimeStateMachineWarning,InfoMessageType.Warning)]
        private YDictionary<string, RuntimeStateMachineCore> runtime_StateMachineCore_Datas = new YDictionary<string, RuntimeStateMachineCore>();

        /// <summary>
        /// 运行时不同的标识所对应的配置转换的状态机执行实例
        /// </summary>
        private Dictionary<string,StateMachineCore> runtime_StateMachineCores = new Dictionary<string, StateMachineCore>();       

        [SerializeField,InfoBox(StateMachineConst.initializeDisplay)]
        private StateInitializeType initializeType = StateInitializeType.Awake;   
        
        [SerializeField]
        [Tooltip(StateMachineConst.resetDisableDisplay)]
        [InfoBox(StateMachineConst.resetDisableDisplay)]
        private bool resetByDisable;

        [SerializeField, InfoBox("已有架构选择")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(AllArchectureTypes))]
#endif
        private string architecture = "None";
        private IArchitecture runtime_Architecture;       

        [InfoBox(StateMachineConst.initializeEventDisplay)]
        public UnityEvent onInitialized;

        [InfoBox(StateMachineConst.onChangeStateEventDisplay)]
        public UnityEvent<StateMachineCore,StateMachine, string> onChangeState;

        /// <summary>
        /// StateManager组件默认的状态机集合
        /// <para>当状态机配置仅存一个时，运行时获取到这个默认的状态机集合</para>
        /// </summary>
        public StateMachineCore DefaultStateMachineCore =>  GetDefaultRuntimeMachineCore();
#if UNITY_EDITOR

        /// <summary>
        /// 作用于编辑器访问
        /// </summary>
        private List<RuntimeStateMachineCore> editor_All_StateMachineCores = new List<RuntimeStateMachineCore>();
        private List<StateMachineCore> editor_All_StateMachines = new List<StateMachineCore>();

        public List<RuntimeStateMachineCore> Editor_All_StateMachineCores
        {
            get
            {
                editor_All_StateMachineCores.Clear();

                if (!Application.isPlaying)
                {
                    foreach (var item in runtime_StateMachineCore_Datas)
                    {
                        if (!item.Value) continue;
                        editor_All_StateMachineCores.Add(item.Value);
                    }
                }
                else
                {
                    foreach (var item in runtime_StateMachineCores)
                    {
                        if (!item.Value.RuntimeStateMachineCore) continue;
                        editor_All_StateMachineCores.Add(item.Value.RuntimeStateMachineCore);
                    }
                }
                return editor_All_StateMachineCores;
            }
        }

        public List<StateMachineCore> Editor_All_StateMachines
        {
            get
            {
                editor_All_StateMachines.Clear();

                foreach (var item in runtime_StateMachineCores)
                {
                    if (item.Value == null) continue;
                    editor_All_StateMachines.Add(item.Value);
                }
                return editor_All_StateMachines;
            }
        }

        IEnumerable AllArchectureTypes
        {
            get
            {
               return AssemblyHelper
                .GetTypes(x => typeof(IArchitecture)
                .IsAssignableFrom(x) && x.IsPublic && !x.IsAbstract && x.IsClass)
                .Select(x => new ValueDropdownItem() { Text = x.Name, Value = x.ToString() })
                .Concat(ValueNull);
            }
        }

        private static ValueDropdownItem[] ValueNull = new ValueDropdownItem[] { new ValueDropdownItem() { Text = "None", Value = "None" } };
#endif      
        /// <summary>
        ///  状态机是否已经初始化完成
        /// </summary>
        public bool IsInitialized { get; private set; }

        protected void Awake()
        {
            if (architecture != "None")
            {
                Type architectureType = AssemblyHelper.GetType(architecture);
                if (architectureType == null)
                    throw new NullReferenceException($"状态机选择了架构类型，但架构类型是无效的，请检查架构类型是否正确处理，ArchitectureType:{architecture}");
                runtime_Architecture = ArchitectureConstructor.Instance.GetOrAddArchitecture(architectureType);
            }

            //有不止一个配置时检查是否存在空标识的配置
            if (runtime_StateMachineCore_Datas.Count > 1 && runtime_StateMachineCore_Datas.ContainsKey(string.Empty))
                throw new InvalidCastException("发现复数配置中存在标识为空的状态机配置，请为配置设置标识!");
           
            if (initializeType == StateInitializeType.Awake)
                Init();
        }

        private void Start()
        {
            if (initializeType == StateInitializeType.Start)
                Init();
        }

        private void OnEnable()
        {
            StateMachineSystem.Instance.AddStateManager(this);
        }
        private void OnDisable()
        {
            if (resetByDisable)
            {
                foreach (var item in runtime_StateMachineCores)
                {
                    item.Value.Cancel();
                }
            }
            StateMachineSystem.Instance.RemoveStateManager(this);
        }
        /// <summary>
        /// 状态机组件初始化方法
        /// </summary>
        private void Init()
        {
            //没有配置就不初始化
            if (runtime_StateMachineCore_Datas.Count == 0)
            {                
                return;
            }
            if (IsInitialized) return;
            foreach (var item in runtime_StateMachineCore_Datas)
            {
                StateMachineCore stateMachineCore = new StateMachineCore(this,item.Value);

                string key = string.Empty;
                if (item.Key.IsNullOrEmpty())
                    key = "Default Machine";
                else key = item.Key;
                runtime_StateMachineCores.Add(key, stateMachineCore);
            }

            IsInitialized = true;
            onInitialized?.Invoke();
        }
        /// <summary>
        /// 根据运行集合配置获取当前运行的状态信息
        /// </summary>
        /// <param name="layerName">集合的唯一标识(在StateManager配置)</param>
        /// <returns></returns>
        public StateBase GetCurrentStateInfo(string layerName)
        {
            if (!runtime_StateMachineCores.TryGetValue(layerName, out var stateMachineCore))
                return null;
            return stateMachineCore.GetCurrentStateInfo();
        }
        /// <summary>
        /// 通过在组件上的层级配置获取指定的状态机集合。
        /// <para>对于只有一个配置的状态机管理器。可调用GetDefaultRuntimeMachineCore方法或者访问DefaultStateMachineCore属性</para>
        /// </summary>
        /// <param name="layerName">集合的唯一标识(在StateManager配置)</param>
        /// <returns></returns>
        public StateMachineCore GetRuntimeMachineCore(string layerName)
        {
            runtime_StateMachineCores.TryGetValue(layerName, out var core);
            return core;
        }

        /// <summary>
        /// 遍历所有的运行时状态机集合本体
        /// </summary>
        /// <param name="each"></param>
        public void ForEach(Action<StateMachineCore> each)
        {
            foreach (var item in runtime_StateMachineCores)
            {
                each?.Invoke(item.Value);
            }
        }      

        internal StateMachineCore GetRuntimeMachineCore(RuntimeStateMachineCore runtimeStateMachineCore)
        {
            foreach (var item in runtime_StateMachineCores)
            {
                if (item.Value.RuntimeStateMachineCore.runtime_GUID == runtimeStateMachineCore.runtime_GUID)
                    return item.Value;
            }
            return null;
        }

        #region AllMachineCore Parameter API
        public void SetBool(StateMachineCore stateMachineCore, string name, bool value)
        {
            stateMachineCore.SetBool(name, value);
        }

        public void SetBool(StateMachineCore stateMachineCore, int nameToHash, bool value)
        {
            stateMachineCore.SetBool(nameToHash, value);
        }

        public void SetFloat(StateMachineCore stateMachineCore, string name, float value)
        {
            stateMachineCore.SetFloat(name, value);
        }

        public void SetFloat(StateMachineCore stateMachineCore, int nameToHash, float value)
        {
            stateMachineCore.SetFloat(nameToHash, value);
        }

        public void SetInt(StateMachineCore stateMachineCore, string name, int value)
        {
            stateMachineCore.SetInt(name, value);
        }

        public void SetInt(StateMachineCore stateMachineCore, int nameToHash, int value)
        {
            stateMachineCore.SetInt(nameToHash, value);
        }


        public void SetTrigger(StateMachineCore stateMachineCore, string name)
        {
            stateMachineCore.SetTrigger(name);
        }

        public void SetTrigger(StateMachineCore stateMachineCore, int nameToHash)
        {
            stateMachineCore.SetTrigger(nameToHash);
        }

        public void ResetTrigger(StateMachineCore stateMachineCore, string name)
        {
            stateMachineCore.ResetTrigger(name);
        }

        public void ResetTrigger(StateMachineCore stateMachineCore, int nameToHash)
        {
            stateMachineCore.SetTrigger(nameToHash);
        }

        public bool GetBool(StateMachineCore stateMachineCore, string name)
        {
            return stateMachineCore.GetBool(name);
        }

        public bool GetBool(StateMachineCore stateMachineCore, int nameToHash)
        {
            return stateMachineCore.GetBool(nameToHash);
        }

        public float GetFloat(StateMachineCore stateMachineCore, string name)
        {
            return stateMachineCore.GetFloat(name);
        }

        public float GetFloat(StateMachineCore stateMachineCore, int nameToHash)
        {
            return stateMachineCore.GetFloat(nameToHash);
        }

        public int GetInt(StateMachineCore stateMachineCore, string name)
        {
            return stateMachineCore.GetInt(name);
        }

        public int GetInt(StateMachineCore stateMachineCore, int nameToHash)
        {
            return stateMachineCore.GetInt(nameToHash);
        }

        public bool GetTrigger(StateMachineCore stateMachineCore, string name)
        {
            return stateMachineCore.GetTrigger(name);
        }

        public bool GetTrigger(StateMachineCore stateMachineCore, int nameToHash)
        {
            return stateMachineCore.GetTrigger(nameToHash);
        }

        public int GetTriggerCount(StateMachineCore stateMachineCore, string name)
        {
            return stateMachineCore.GetTriggerCount(name);
        }

        public int GetTriggerCount(StateMachineCore stateMachineCore, int nameToHash)
        {
            return stateMachineCore.GetTriggerCount(nameToHash);
        }
        #endregion

        /// <summary>
        /// 通过在组件上的层级配置获取默认的状态机集合。
        /// <para>对于单个配置。可使用该重载</para>
        /// </summary>
        /// <param name="stateDefault"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public StateMachineCore GetDefaultRuntimeMachineCore()
        {        
            if(runtime_StateMachineCores.Count != 1)
                throw new NullReferenceException("状态机配置不唯一!无法通过默认加载获取");
            return runtime_StateMachineCores.FirstOrDefault().Value;            
        }
        public IEnumerator<StateMachineCore> GetEnumerator()
        {
            return runtime_StateMachineCores.Values.GetEnumerator();
        }
        [DisableEnumeratorWarning]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        private void OnDestroy() 
        {
            architecture = string.Empty;
            runtime_Architecture = null;
        }

        public IArchitecture GetArchitecture()
        {
            return runtime_Architecture;
        }

        #region static
        private static Dictionary<string, int> string_hashs = new Dictionary<string, int>();
        private static Dictionary<int, string> hash_strings = new Dictionary<int, string>();
        public static int StringToHash(string event_name)
        {
            if (string_hashs.TryGetValue(event_name, out int hash))
                return hash;

            hash = Animator.StringToHash(event_name);

            string_hashs.Add(event_name, hash);
            if (!hash_strings.ContainsKey(hash))
                hash_strings.Add(hash, event_name);

            return string_hashs[event_name];
        }

        internal static string HashToString(int hash)
        {
            if (hash_strings.ContainsKey(hash))
                return hash_strings[hash];
            return string.Empty;
        }
        private static Dictionary<string, StateManager> static_runtime_Machines = new Dictionary<string, StateManager>();
        /// <summary>
        /// 启动一个不进行StateManager组件挂载的状态机并返回
        /// <para>Tip:通过StartMachine启动的状态机，是不可被销毁的对象，如需要移除，需要手动调用StateManager.RemoveMachine方法</para>
        /// </summary>
        /// <param name="machineName">全局状态机的名称</param>
        /// <param name="stateMachineCore">这个状态机需要用到的配置</param>     
        /// <param name="archectureType">可依赖的框架架构类型</param>
        /// <returns></returns>
        public static StateManager StartMachine(string machineName,RuntimeStateMachineCore stateMachineCore,Type archectureType = default)
        {
            if (static_runtime_Machines.ContainsKey(machineName))
            {
                Debug.LogWarning($"这个状态机已经启动了! StateManager:{machineName}");
                return static_runtime_Machines[machineName];
            }
            GameObject gameObject = new GameObject(machineName);
            gameObject.DonDestroyOnLoad();
            var stateManager = gameObject.AddComponent<StateManager>();
            stateManager.runtime_StateMachineCore_Datas.Add(string.Empty, stateMachineCore);
            static_runtime_Machines.Add(machineName, stateManager);
            if (typeof(IArchitecture).IsAssignableFrom(archectureType))
                stateManager.runtime_Architecture = ArchitectureConstructor.Instance.GetOrAddArchitecture(archectureType);
            stateManager.Init();
           
            return stateManager;
        }

        /// <summary>
        /// 获取通过StartMachine方法加载的全局状态机管理组件
        /// </summary>
        /// <param name="machineName"></param>
        /// <returns></returns>
        public static StateManager GetGlobalStateManager(string machineName)
        {
            static_runtime_Machines.TryGetValue(machineName, out var manager);
            return manager;
        }

        /// <summary>
        /// 移除通过StartMachine方法加载的全局状态机管理组件
        /// </summary>
        /// <param name="machineName">全局状态机名称</param>
        /// <returns></returns>
        public static bool RemoveMachine(string machineName)
        {
            if (!static_runtime_Machines.TryGetValue(machineName, out var manager))
                return false;
            manager.gameObject.Destroy();
            static_runtime_Machines.Remove(machineName);
            return true;
            
        }
        #endregion

    }
}
