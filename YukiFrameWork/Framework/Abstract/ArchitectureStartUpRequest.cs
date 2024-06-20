///=====================================================
/// - FileName:      ArchitectureStartUpRequest.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/6 19:47:39
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using XFABManager;
using System.Collections;
using YukiFrameWork.Extension;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Linq;
namespace YukiFrameWork
{
    public class ArchitectureStartUpRequest : CustomYieldInstruction
    {
        public override bool keepWaiting => !_isDone;

        internal bool _isDone = false;

        public bool isDone => _isDone;

        private Type[] allRolds;

        private Type architectureType;

        internal IArchitecture architecture;  

        public ArchitectureStartUpRequest(Type[] allRolds, Type architectureType)
        {
            _isDone = false;
            this.architectureType = architectureType;
            this.allRolds = allRolds;
            architecture = ArchitectureConstructor.I.GetOrAddArchitecture(architectureType);
            MonoHelper.Start(StartModule());
        }

        /// <summary>
        /// 手动释放某一个架构,并不是将架构本身置空，仍可访问，而是对架构的所有数据进行清空操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ReleaseArchitecture<T>() where T : class,IArchitecture
        {
            if (ArchitectureConstructor.I.runtimeRequests.TryGetValue(typeof(T).FullName, out var request))
            {
                ArchitectureConstructor.I.runtimeRequests.Remove(typeof(T).FullName);
                request.architecture.Dispose();
                MonoHelper.Destroy_RemoveListener(request.OnArchitectureDispose);               
            }
        }

        public static ArchitectureStartUpRequest StartUpArchitecture<T>() where T :class, IArchitecture
        {
            var runtimeRequests = ArchitectureConstructor.I.runtimeRequests;
            string key = typeof(T).FullName;
            if (!runtimeRequests.TryGetValue(key, out var request))
            {
                request = new ArchitectureStartUpRequest(AssemblyHelper.GetTypes(typeof(T)), typeof(T));               
                runtimeRequests[key] = request;
            }                                         
            return request;
        }

        public YieldTask<ArchitectureStartUpRequest> GetAwaiter()
        {
            YieldTask<ArchitectureStartUpRequest> awaiter = new YieldTask<ArchitectureStartUpRequest>();       
            YieldTaskExtension.RunOnUnityScheduler(() => MonoHelper.Start(NextYield()));

            IEnumerator NextYield()
            {
                yield return this;
                awaiter.Complete(null, this);
            }
            return awaiter;
        }

        abstract class OrderModule<T>
        {
            public T Value;
            public int order;
        }

        class OrderModel : OrderModule<IModel>
        {
           
        }

        class OrderSystem : OrderModule<ISystem>
        {
           
        }

        class OrderUtility : OrderModule<IUtility>
        {
            
        }

        class OrderController : OrderModule<AbstractController>
        {
            
        }

        private FastList<OrderModel> models = new FastList<OrderModel>();
        private FastList<OrderSystem> systems = new FastList<OrderSystem>();

        private FastList<OrderController> controllers = new FastList<OrderController>();
        
        private EasyContainer container => architecture.Container;

        private IEnumerator StartModule()
        {          
            if (container == null)
            {
                OnCompleted("丢失架构容器，模块启动失败! 请检查 ArchitectureType:" + architectureType);
                yield break;
            }
            if (architecture == null)
            {
                OnCompleted("当前没有查找到对应的架构,模块启动失败!请检查是否已经在运行时全部获取完毕了,如果没有获取请等待，生命周期执行在场景之前才可获得，而后才可以加载架构模块的初始化 Architecture Type:" + architectureType);
                yield break;
            }           
            float allCount = allRolds.Length;
            float currentCount = 0;
            for (int i = 0; i < allRolds.Length; i++)
            {
                yield return null;
                try
                {
                    currentCount++;
                    progress = currentCount / allCount * 0.5f;

                    Type type = allRolds[i];
                    if (type == null) continue;

                    if (typeof(IModel).IsAssignableFrom(type)
                        || typeof(ISystem).IsAssignableFrom(type)
                        || typeof(IUtility).IsAssignableFrom(type))
                    {
                        RegistrationAttribute registration = type.GetCustomAttribute<RegistrationAttribute>(true);
                        if (registration == null) continue;
                        if (registration.architectureType != architectureType) continue;
                        object value = Activator.CreateInstance(type);

                        registration.registerType ??= type;

                        if (!registration.registerType.IsAssignableFrom(type))
                        {
                            OnCompleted("存在定义自定义类型时使用的类型与本体类没有继承关系,模块启动失败! Type:" + type.FullName + "   RegisterType:" + registration.registerType);
                            yield break;
                        }

                        if (value is IUtility utility)
                        {
                            container.Register(utility, registration.registerType);
                        }
                        else if (value is IModel model)
                        {
                            container.Register(model, registration.registerType);
                            models.Add(new OrderModel() { Value = model,order = registration.order});
                        }
                        else if (value is ISystem system)
                        {
                            container.Register(system, registration.registerType);
                            systems.Add(new OrderSystem() {Value = system,order = registration.order });
                        }
                    }
                    else if (type.IsSubclassOf(typeof(AbstractController)))
                    {
                        InitControllerAttribute initController = type.GetCustomAttribute<InitControllerAttribute>(true);
                        if (initController == null)                       
                            continue;
                        AbstractController value = Activator.CreateInstance(type) as AbstractController;

                        if (value == null) continue;

                        controllers.Add(new OrderController() { Value = value,order = initController.order });
                    }
                }
                catch (Exception ex)
                {
                    OnCompleted("模块启动失败!    " + ex.ToString());
                    yield break;
                }
            }

            try
            {
                var orderModels = models
                    .OrderByDescending(m => m.order);

                foreach (var model in orderModels)
                {
                    model.Value.SetArchitecture(architecture);
                    model.Value.Init();
                }

                var orderSystems = systems
                    .OrderByDescending(s => s.order);

                foreach (var system in orderSystems)
                {
                    system.Value.SetArchitecture(architecture);
                    system.Value.Init();
                }

                var orderControllers = controllers
                    .OrderByDescending(c => c.order);

                foreach (var controller in orderControllers)
                {
                    controller.Value.OnInit();
                }

                models.Clear();
                systems.Clear();
                controllers.Clear(); 
            }
            catch(Exception ex)
            {
                OnCompleted(ex.ToString());
                yield break;
            }

            (string sceneName, SceneLoadType loadType) = architecture.DefaultSceneName;
            if (!sceneName.IsNullOrEmpty())
            {
               
                switch (loadType)
                {
                    case SceneLoadType.Local:
                        var operation = SceneManager.LoadSceneAsync(sceneName);
                        while (operation != null && !operation.isDone)
                        {
                            yield return null;
                            progress = 0.5f + operation.progress * 0.5f;
                        }
                        break;
                    case SceneLoadType.XFABManager:
                        SceneTool.XFABManager.Init(architecture.OnProjectName);
                        var request = AssetBundleManager.LoadSceneAsynchrony(architecture.OnProjectName, sceneName, LoadSceneMode.Single);
                        if (request != null && !request.isDone)
                        {
                            yield return null;
                            progress = 0.5f + request.progress * 0.5f;
                        }
                        break;                  
                }
            }
            architecture.Init();
            MonoHelper.Destroy_AddListener(OnArchitectureDispose);
            OnCompleted(string.Empty);
        }

        void OnArchitectureDispose(MonoHelper helper)
        {
            architecture.Dispose();
        }

        internal void OnCompleted(string error)
        {
            this.error = error;
            progress = 1;
            _isDone = true;
        }

        public float progress { get; private set; }

        private string _error;

        public string error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
            }
        }
    }
}
