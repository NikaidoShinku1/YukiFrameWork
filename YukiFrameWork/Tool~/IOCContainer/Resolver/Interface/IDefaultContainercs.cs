using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukiFrameWork
{
    public interface IDefaultContainer
    {
        T Resolve<T>() where T : class;
        T Resolve<T>(string name) where T : class;

        object Resolve(Type type, string name = "");
    }
}
