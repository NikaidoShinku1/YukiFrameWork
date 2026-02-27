using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace YukiFrameWork.AI
{
    
    public enum NavMeshUpdateMode
    {
        Update = 0,
        FixedUpdate = 1,
        LateUpdate = 2
    }

    public static class AIServicesKit
    {
        private static IAIServicesLoader _aiServicesLoader = null;
        private static Dictionary<string,Dictionary<int,AIServicesInfo>> runtime_AllAIServicesInfos = new Dictionary<string,Dictionary<int,AIServicesInfo>>();
        private static Dictionary<int, NavMeshSurface> runtime_AllNavMeshSurfaces = new Dictionary<int, NavMeshSurface>();
        private static Dictionary<int, RuntimeAIServices> runtime_Services = new Dictionary<int, RuntimeAIServices>();
        private static List<int> releases = new List<int>();

        public class RuntimeAIServices
        {
            public int ServicesId => AIServicesInfo.id;
            public int InstanceId { get; }
            public IAIServices Services { get; }
            public AIServicesInfo AIServicesInfo { get; }

            public bool IsRunning { get; internal set; }

            private RuntimeAIServices()
            {
                
            }

            public RuntimeAIServices(IAIServices services, AIServicesInfo servicesInfo)
            {
                this.Services = services;
                this.AIServicesInfo = servicesInfo;
                Services.Agent.speed = servicesInfo.speed;
                this.InstanceId = Services.Agent.GetInstanceID();
                
                Services.ServicesInit();
            }
            

            internal async void Enable()
            {
                if (IsRunning) return;
                IsRunning = true;
                Services.Enable();
                
                //启动后等待Agent成功同步到NavMesh上再设置区域权重，避免在Agent未同步到NavMesh上时设置区域权重导致的异常
#if UNITY_2021_1_OR_NEWER
                await CoroutineTool.WaitWhile(() => IsOnNavMesh);
                for (int i = 0; i < AIServicesInfo.areaCastInfos.Count; i++)
                {
                    var castInfo = AIServicesInfo.areaCastInfos[i];
                    if (castInfo == null) continue;
                    int areaIndex = NavMesh.GetAreaFromName(castInfo.areaName);
                    this.Services.Agent.SetAreaCost(areaIndex,castInfo.cast);
                }
#else
                MonoHelper.Start(WaitAgentOnNavMesh());
#endif
            }
#if !UNITY_2021_1_OR_NEWER
            private IEnumerator WaitAgentOnNavMesh()
            {
                yield return CoroutineTool.WaitUntil(() => IsOnNavMesh);
                for (int i = 0; i < AIServicesInfo.areaCastInfos.Count; i++)
                {
                    var castInfo = AIServicesInfo.areaCastInfos[i];
                    if (castInfo == null) continue;
                    int areaIndex = NavMesh.GetAreaFromName(castInfo.areaName);
                    this.Services.Agent.SetAreaCost(areaIndex,castInfo.cast);
                }
            }
#endif
            internal void Disable()
            {
                if (!IsRunning) return;
                IsRunning = false;
                Services.Disable();
            }

            internal void Warp(Vector3 position)
            {
                Services.Agent.Warp(position);

                if (Services.Agent.isOnNavMesh)
                    Services.NavMeshSync();
                else Services.NavMeshClean();
            }

            internal void Update()
            {
                if (!IsOnNavMesh || !IsRunning)
                {
                    if (HasPath)
                        Services.Agent.ResetPath();
                    return;
                }

                if (!HasPath)
                    Services.Agent.SetDestination(Services.EndPos);
                
                if(IsOnOffMeshLink && Services.IsLinkIgnore)
                    Services.Agent.CompleteOffMeshLink();

                Vector3 direction = HasPath ? Services.Agent.nextPosition - Services.Agent.transform.position : Vector3.zero;
                Services.NavMeshUpdate(direction);

            }

            internal void FixedUpdate()
            {
                if (!IsRunning || !IsOnNavMesh) return;
                Vector3 direction = HasPath ? Services.Agent.nextPosition - Services.Agent.transform.position : Vector3.zero;
                Services.NavMeshFixedUpdate(direction);
            }

            internal void LateUpdate()
            {
                if (!IsRunning || !IsOnNavMesh) return;

                Vector3 direction = HasPath ? Services.Agent.nextPosition - Services.Agent.transform.position : Vector3.zero;
                Services.NavMeshLateUpdate(direction);
            }

            public bool IsOnNavMesh => Services.Agent.isOnNavMesh;
            public bool HasPath => Services.Agent.hasPath;

            public bool IsOnOffMeshLink => Services.Agent.isOnOffMeshLink;

        }

        static AIServicesKit()
        {
            MonoHelper.Update_RemoveListener(Update);
            MonoHelper.Update_AddListener(Update);
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            MonoHelper.FixedUpdate_AddListener(FixedUpdate);
            MonoHelper.LateUpdate_RemoveListener(LateUpdate);
            MonoHelper.LateUpdate_AddListener(LateUpdate);
        }

        public static void Init(string projectName)
        {
            Init(new ABManagerServicesLoader(projectName));
        }

        public static void Init(IAIServicesLoader loader)
        {
            _aiServicesLoader = loader;
        }

        public static void LoadAIServicesConfig(string nameOrPath)
        {
            LoadAIServicesConfig(_aiServicesLoader.Load<AIServicesConfig>(nameOrPath));
        }

        public static void LoadAIServicesConfig(AIServicesConfig config)
        {
            if (!runtime_AllAIServicesInfos.ContainsKey(config.groupName))
                runtime_AllAIServicesInfos[config.groupName] = new Dictionary<int, AIServicesInfo>();
            for(int i = 0;i < config.aiServicesInfos.Count;i++)
            {
                var info = config.aiServicesInfos[i];
                if (!info)continue;

                runtime_AllAIServicesInfos[config.groupName].Add(info.id,info.Instantiate());
            }
            _aiServicesLoader?.UnLoad(config);
        }

        public static IEnumerator LoadAIServicesConfigAsync(string nameOrPath)
        {
            bool completed = false;
            
            _aiServicesLoader.LoadAsync<AIServicesConfig>(nameOrPath, config =>
            {
                LoadAIServicesConfig(config);
                completed = true;
            });
            
            yield return CoroutineTool.WaitUntil(() => completed);
        }

        /// <summary>
        /// 注册Surface
        /// </summary>
        /// <param name="id"></param>
        /// <param name="navMeshSurface"></param>
        /// <exception cref="System.Exception"></exception>
        public static void RegisterSurface(int id, NavMeshSurface navMeshSurface)
        {
            if (runtime_AllNavMeshSurfaces.ContainsKey(id))
                throw new System.Exception("已存在指定的Surface Id! Id:" + id);

            runtime_AllNavMeshSurfaces[id] = navMeshSurface;
        }

        /// <summary>
        /// 注销Surface
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool UnregisterSurface(int id)
        {
            if (!runtime_AllNavMeshSurfaces.ContainsKey(id)) return false;

            Clear(id);

            return runtime_AllNavMeshSurfaces.Remove(id);

        }

        /// <summary>
        /// 注册AI
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterServices(IAIServices services,string groupName,int id)
        {
            if(!services.Agent)
                throw new  NullReferenceException("丢失导航组件,无法注册AI服务! Id:" + id);
            
            services.Agent.updatePosition = services.Agent.updateRotation = services.Agent.updateUpAxis = false;
            if (CheckAIServices(services, out _))
                throw new System.Exception("指定的AI服务已存在:GameObject:" + services.Agent.gameObject.name);
            
            if(!runtime_AllAIServicesInfos.TryGetValue(groupName, out var servicesInfos))
                throw new System.Exception("不存在指定分组的AI分组! GroupName:" + groupName);
            
            if (!servicesInfos.TryGetValue(id, out var servicesInfo))
                throw new System.Exception("不存在指定标识的AI服务! GroupName:" + groupName + " Id:" + id);
            
            runtime_Services.Add(services.Agent.GetInstanceID(),new RuntimeAIServices(services,servicesInfo));
        }

        /// <summary>
        /// 注销AI
        /// </summary>
        /// <param name="services"></param>
        public static bool UnRegisterServices(IAIServices services)
        {
            return UnRegisterServices(services.Agent.GetInstanceID());
        }
        
        internal static bool UnRegisterServices(int instanceId)
        {
            return runtime_Services.Remove(instanceId);
        }

        private static void Warp()
        {
            foreach (var serviceData in runtime_Services)
            {
                var services = serviceData.Value.Services;
                if (services == null) continue;
                services.Warp(services.Agent.transform.position);

            }
        }

        /// <summary>
        /// 同步AI的位置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="position"></param>
        /// <exception cref="System.Exception"></exception>
        public static void Warp(this IAIServices services, Vector3 position)
        {
            if (!CheckAIServices(services, out var result))
                throw new System.Exception("未注册指定的AI服务 GameObject:" + services.Agent.gameObject.name);
            result.Warp(position);
        }

        /// <summary>
        /// 启动AI
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="System.Exception"></exception>
        public static void AIEnable(this IAIServices services)
        {
            if (!CheckAIServices(services,out var result))
                throw new System.Exception("未注册指定的AI服务 GameObject:" + services.Agent.gameObject.name);

            result.Enable();
        }

        /// <summary>
        /// 关闭AI
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="System.Exception"></exception>
        public static void AIDisable(this IAIServices services)
        {
           if(!CheckAIServices(services,out var result))
                throw new System.Exception("未注册指定的AI服务 GameObject:" + services.Agent.gameObject.name);

            result.Disable();
        }

        private static bool CheckAIServices(IAIServices services,out RuntimeAIServices result)
        {
            return runtime_Services.TryGetValue(services.Agent.GetInstanceID(), out result);
        }

        internal static void Update(MonoHelper _)
        {
            Execute(NavMeshUpdateMode.Update);
        }

        internal static void FixedUpdate(MonoHelper _)
        {
            Execute(NavMeshUpdateMode.FixedUpdate);
        }

        internal static void LateUpdate(MonoHelper _)
        {

            Execute(NavMeshUpdateMode.LateUpdate);
        }

        private static void Execute(NavMeshUpdateMode updateMode)
        {
            foreach(var services in runtime_Services.Values)
            {
                switch (updateMode)
                {
                    case NavMeshUpdateMode.Update:
                        services.Update();
                        //如果导航已经丢失自动处理释放
                        if(!services.Services.Agent)
                            releases.Add(services.InstanceId);
                        break;
                    case NavMeshUpdateMode.FixedUpdate:
                        services.FixedUpdate();
                        break;
                    case NavMeshUpdateMode.LateUpdate:
                        services.LateUpdate();
                        break;
                    default:
                        break;
                }
            }

            if (updateMode == NavMeshUpdateMode.LateUpdate)
            {
                if (releases.Count != 0)
                {
                    for (int j = 0; j < releases.Count; j++)
                    {
                        UnRegisterServices(releases[j]);
                    }
                    releases.Clear();
                }
            }
        }

        /// <summary>
        /// 烘焙指定标识的网格
        /// </summary>
        /// <param name="id"></param>
        public static void Baker(int id)
        {
            if (!runtime_AllNavMeshSurfaces.TryGetValue(id, out var surface))
            {
                Debug.LogWarning("不存在指定标识的NavMeshSurface，无法构建烘焙! Id:" + id);
                return;
            }

            surface.BuildNavMesh();
            runtime_AllNavMeshSurfaces[id] = surface;
            Warp();
        }

        /// <summary>
        /// 清空指定标识的网格
        /// </summary>
        /// <param name="id"></param>
        public static void Clear(int id)
        {
            if (!runtime_AllNavMeshSurfaces.TryGetValue(id, out var surface))
            {
                Debug.LogWarning("不存在指定标识的NavMeshSurface，无法释放烘焙! Id:" + id);
                return;
            }

            surface.RemoveData();
            surface.navMeshData = null;
           
            runtime_AllNavMeshSurfaces[id] = surface;
            Warp();
        }

        /// <summary>
        /// 指定的网格是否已经烘焙
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsBaker(int id)
        {
            if (!runtime_AllNavMeshSurfaces.TryGetValue(id, out var surface))
                return false;

            return surface.navMeshData != null;
        }

        /// <summary>
        /// 更新烘焙网格的数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="navMeshData"></param>
        /// <returns></returns>
        public static AsyncOperation Update(int id, NavMeshData navMeshData)
        {
            if (!runtime_AllNavMeshSurfaces.TryGetValue(id, out var surface))
            {
                Debug.LogWarning("不存在指定标识的NavMeshSurface，无法更新烘焙! Id:" + id);
                return null;
            }

            var operation = surface.UpdateNavMesh(navMeshData);

            runtime_AllNavMeshSurfaces[id] = surface;
            Warp();
            return operation;
        }

        /// <summary>
        /// 当前AI是否处于活动状态
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static bool IsRunning(this IAIServices services)
        {
            return AsRuntime(services)?.IsRunning == true;
        }

        /// <summary>
        /// 服务转换为运行服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static RuntimeAIServices AsRuntime(this IAIServices services)
        {
            if (!CheckAIServices(services, out var result))
                throw new System.Exception("未注册指定的AI服务 GameObject:" + services.Agent.gameObject.name);

            return result;
        }
    }
}