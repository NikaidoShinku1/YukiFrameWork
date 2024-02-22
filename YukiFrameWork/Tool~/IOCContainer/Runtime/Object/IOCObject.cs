using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork.IOC
{
   
    public interface IEntryPoint : IObject
    {
        string Name { get; }
        object Value { get; }
        void Destroy();
    }
    public interface IEntryPoint<T> : IEntryPoint
    {
        
    }

    public interface IObject
    {
        int HashID { get; }
    }

    public abstract class IOCObject<T> : IEntryPoint<T>, IEntryPoint, IObject,IDisposable
    {
        public object Value => Get();

        public abstract T Get();

        public int HashID => Value.GetHashCode();

        protected IOCContainer container;

        protected string name;
        public string Name => name;

        protected Type type;

        protected bool IsDestroy = false;

        protected LifeTime lifeTime;

        public IOCObject(string name, LifeTime lifeTime, IOCContainer container, Type type)
        {
            this.name = name;
            this.lifeTime = lifeTime;
            this.container = container;
            this.type = type;
        }

        public IOCObject() { }

        public override string ToString()
        {
            return Value.ToString();
        }

        public abstract void Dispose();

        public void Destroy()
        {
            IsAutoRelease = true;
            Dispose();
        }

        /// <summary>
        /// 是否通过自动释放
        /// </summary>
        public bool IsAutoRelease { get;protected set; } = false;
    }
}