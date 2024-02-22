using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.IOC
{
    public static class IOCHelper
    {
        public static T CreateInstance<T>(Action<object> onReset = null, params object[] args)
        {
            return (T)CreateInstance(typeof(T), onReset,args);
        }

        public static object CreateInstance(this Type type,Action<object> onReset = null,params object[] args)
        {
            object obj = Activator.CreateInstance(type,args);
            onReset?.Invoke(obj);
            AddTick(obj);
            return obj;
        }

        public static void AddTick(object obj)
        {           
            if (obj is IUpdateTickable  update)
                MonoHelper.Update_AddListener(update.Update);

            if (obj is IFixedUpdateTickable fixedUpdate)
                MonoHelper.FixedUpdate_AddListener(fixedUpdate.FixedUpdate);

            if (obj is ILateUpdateTickable lateUpdate)
                MonoHelper.LateUpdate_AddListener(lateUpdate.LateUpdate);
        }

        public static void ClearTick(object obj)
        {
            if (obj is IUpdateTickable  update)
                MonoHelper.Update_RemoveListener(update.Update);

            if (obj is IFixedUpdateTickable fixedUpdate)
                MonoHelper.FixedUpdate_RemoveListener(fixedUpdate.FixedUpdate);

            if (obj is ILateUpdateTickable lateUpdate)
                MonoHelper.LateUpdate_RemoveListener(lateUpdate.LateUpdate);
        }
    }
}
