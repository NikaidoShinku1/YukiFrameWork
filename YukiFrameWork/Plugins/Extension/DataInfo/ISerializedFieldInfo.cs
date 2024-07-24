using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork
{
    public interface ISerializedFieldInfo
    {
        string name { get; set; }
        public void AddFieldData(SerializeFieldData data);

        public void RemoveFieldData(SerializeFieldData data);

        public void ClearFieldData();         

        public IEnumerable<SerializeFieldData> GetSerializeFields();

        SerializeFieldData Find(Func<SerializeFieldData, bool> func);      
    }
}
