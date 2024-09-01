using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YukiFrameWork;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Events
{
    public class RuntimeEventInitializationFactory
    {        
        public static void Initialization<T>(T viewController) where T : ViewController
        {
            if (viewController == null || !viewController || viewController.IsEventInited) return;
            
            viewController.IsEventInited = true;
            if (viewController.IsAutoRegisterAttributeEvent)
            {
                MethodInfo[] methodInfos = viewController.GetType().GetRuntimeMethods().Where(x => x.ReturnType == typeof(void) && x.HasCustomAttribute<BaseRegisterEvent>(true))
                        .ToArray();

                for (int i = 0; i < methodInfos.Length; i++)
                {
                    MethodInfo methodInfo = methodInfos[i];
                    if (methodInfo == null) continue;
                    var infos = methodInfo.GetParameters();

                    if (infos == null) continue;

                    if (infos.Length != 1) continue;

                    if (!typeof(IEventArgs).IsAssignableFrom(infos[0].ParameterType)) continue;

                    if (!methodInfo.GetRegisterAttribute(out var registerEvent, out var stringRegisterEvent, out var enumRegisterEvent))
                        continue;
                    Type parameterType = infos[0].ParameterType;

                    if (registerEvent != null)
                    {
                        var ev = viewController.GetArchitectureByInternal.SyncDynamicEventSystem.Register(parameterType, methodInfo, viewController);
                        if (registerEvent.unRegisterType == UnRegisterType.OnDisable)
                            ev.UnRegisterWaitGameObjectDisable(viewController);
                        else if (registerEvent.unRegisterType == UnRegisterType.OnDestroy)
                            ev.UnRegisterWaitGameObjectDestroy(viewController);
                    }

                    if (stringRegisterEvent != null)
                    {
                        string key = stringRegisterEvent.eventName.IsNullOrEmpty() ? methodInfo.Name : stringRegisterEvent.eventName;
                        var ev = viewController.GetArchitectureByInternal.SyncDynamicEventSystem.Register(key, methodInfo, viewController);

                        if (stringRegisterEvent.unRegisterType == UnRegisterType.OnDisable)
                            ev.UnRegisterWaitGameObjectDisable(viewController);
                        else if (stringRegisterEvent.unRegisterType == UnRegisterType.OnDestroy)
                            ev.UnRegisterWaitGameObjectDestroy(viewController);
                    }
                    if (enumRegisterEvent != null)
                    {
                        bool IsDepend = Enum.IsDefined(enumRegisterEvent.enumType, enumRegisterEvent.enumId);
                        if (!IsDepend)
                        {
                            Debug.LogWarningFormat("该下标没有指定枚举值---- Type:{0} ---- Id{1}  MethodName:{2}", enumRegisterEvent.enumType, enumRegisterEvent.enumId, methodInfo.Name);
                            continue;
                        }
                        var ev = viewController.GetArchitectureByInternal.SyncDynamicEventSystem.Register(enumRegisterEvent.enumType, enumRegisterEvent.enumId, methodInfo, viewController);
                        if (enumRegisterEvent.unRegisterType == UnRegisterType.OnDisable)
                            ev.UnRegisterWaitGameObjectDisable(viewController);
                        else if (enumRegisterEvent.unRegisterType == UnRegisterType.OnDestroy)
                            ev.UnRegisterWaitGameObjectDestroy(viewController);
                    }

                }
            }
            var eventCenter = viewController.GetComponent<RuntimeEventCenter>();
            if (eventCenter == null) return;

            foreach (var item in eventCenter.centers)
            {
                if (item.eventName.IsNullOrEmpty()) continue;
                var ev =  viewController.RegisterEvent<IEventArgs>(item.eventName,arg => item.onEvent?.Invoke(arg));

                if (item.unRegisterType == UnRegisterType.OnDisable)
                    ev.UnRegisterWaitGameObjectDisable(viewController);
                else if (item.unRegisterType == UnRegisterType.OnDestroy)
                    ev.UnRegisterWaitGameObjectDestroy(viewController);
            }
        }     
        
    }
}