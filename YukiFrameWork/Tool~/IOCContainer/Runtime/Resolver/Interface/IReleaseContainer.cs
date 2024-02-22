using System;
using UnityEngine;


namespace YukiFrameWork
{
    public interface IReleaseContainer
    {             
        public bool Destroy(Type type, string name);

        public bool Destroy(Type type);     

    }
}  
