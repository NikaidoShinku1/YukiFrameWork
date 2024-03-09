using System.Collections;
using UnityEngine;

namespace YukiFrameWork
{
    public interface ILoggerExtension : ILogger
    {
        T ToObject<T>();
        object ToObject();       
    }
    public class CustomLogger : Logger, ILoggerExtension
    {       
        private object _object = null;

        public CustomLogger(ILogHandler logHandler,object _object) : base(logHandler)
        {
            this._object = _object;
        }       

        public object ToObject()
            => _object;

        public T ToObject<T>()
            => (T)_object;

        public override string ToString()
        {
            return _object.ToString();
        }

        public override int GetHashCode()
        {
            return _object.GetHashCode();
        }
    }
}
