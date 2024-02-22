using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.IOC
{
   
    public interface IStartable
    {
        void Start();
    }

    public interface IUpdateTickable  
    {
        void Update(MonoHelper helper);
    }

    public interface IFixedUpdateTickable 
    {
        void FixedUpdate(MonoHelper helper);
    }

    public interface ILateUpdateTickable
    {
        void LateUpdate(MonoHelper helper);
    }

    public interface IReleaseTickable
    {
        void Release();
    }
}
