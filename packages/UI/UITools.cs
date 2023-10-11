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
        /// ��ȡ��������Basepanel�µ����
        /// </summary>
        /// <typeparam name="T">����</typeparam>
        /// <returns>����һ�����</returns>
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
        /// ���ҵ�ǰ����µ�����������
        /// </summary>
        /// <typeparam name="T">����</typeparam>
        /// <param name="name">obj��</param>
        /// <returns>����һ�����</returns>
        public T GetComponentInChildren<T>(string name) where T : Component
        {
            GameObject obj = FindChildGameObject(name);
            if (obj == null)
            {
                Debug.LogError($"��ǰ�����û�����GameObject��obj����Ϊ{name}");
                return null;
            }          
            var component = obj.GetComponent<T>();
            if(component == null)
                component= panel.gameObject.AddComponent<T>();
            return component;

        }
    }
}
