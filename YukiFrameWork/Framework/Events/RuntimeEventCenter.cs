using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Events
{
    [ClassAPI("运行时事件注册")]
    [GUIDancePath("YukiFrameWork/Framework/Events")]
    public class RuntimeEventCenter : MonoBehaviour
    {
        [HideInInspector]
        public RegisterType registerType = RegisterType.String;

        [SerializeField]
        private List<EventCenter> centers = new List<EventCenter>();

        public void AddEventCenter(EventCenter eventCenter)
            => centers.Add(eventCenter);

        public void RemoveEventCenter(EventCenter eventCenter)
            => centers.Remove(eventCenter);

        public void RemoveEventCenter(int index)
            => centers.RemoveAt(index);

        public EventCenter GetEventCenter(int index)
            => centers[index];         

        public IEnumerable<int> GetEventCenterIndex()
        {            
            for (int i = 0; i < centers.Count; i++)
            {             
                yield return i;
            }         
        }       

    }   
}
