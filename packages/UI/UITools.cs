using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YukiFrameWork.UI
{
    public class UITools
    {
        private BasePanel panel;

        public UITools(BasePanel panel)
        {
            this.panel = panel;
        }

        /// <summary>
        /// 获取或者生成Basepanel下的组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>返回一个组件</returns>
        public T GetComponent<T>() where T : Component
        {
            var component = panel.GetComponent<T>();
            if(component == null)
                component = panel.gameObject.AddComponent<T>();
            return component;
        }

        public GameObject FindChildGameObject(string name)
        { 
            Transform[] transforms = panel.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in transforms)
            {
                if (item.name == name)
                {
                    return item.gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// 查找当前面板下的子物体的组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="name">obj名</param>
        /// <returns>返回一个组件</returns>
        public T GetComponentInChildren<T>(string name) where T : Component
        {
            GameObject obj = FindChildGameObject(name);
            if (obj == null)
            {
                Debug.LogError($"当前面板下没有这个GameObject，obj名字为{name}");
                return null;
            }          
            var component = obj.GetComponent<T>();
            if(component == null)
                component= panel.gameObject.AddComponent<T>();
            return component;

        }
    }
}
