using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{ 
    internal struct SceneObject
    {
        public string name { get; set; }

        private int hashCode;

        public float time;

        internal SceneObject(string name, int hashCode)
        {
            this.name = name;
            this.hashCode = hashCode;
            time = Time.time;
        }

        public override int GetHashCode()
        {
            return hashCode;
        } 
    }
}