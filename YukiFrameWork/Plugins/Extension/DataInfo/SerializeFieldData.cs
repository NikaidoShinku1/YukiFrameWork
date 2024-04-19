using System;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
using Object = UnityEngine.Object;

namespace YukiFrameWork
{   
    [Serializable]
    public class SerializeFieldData 
    {      
        public string[] fieldLevel = new string[]
        {
            "private",
            "protected",           
            "internal",
            "public"
        };     

        public int fieldLevelIndex = 0;

        public int fieldTypeIndex = 0;

        [SerializeField]
        private List<string> mComponents;

        public List<string> Components
        {
            get
            {
                if (mComponents == null) mComponents = new List<string>();
                mComponents.Clear();
                if (gameObject != null)
                {
                    mComponents.Add(typeof(GameObject).ToString());
                    Component[] components = gameObject.GetComponents<Component>();

                    foreach (Component comp in components)
                    {
                        mComponents.Add(comp.GetType().FullName);
                    }
                }
                else if(target != null)
                {                  
                    Type baseType = this.target.GetType();
                    do
                    {
                        if(!baseType.Equals(typeof(object)))
                            mComponents.Add(baseType.FullName);
                        baseType = baseType.BaseType;
                    } while (baseType != null);
                }

                return mComponents;
            }
        }
        [HideInInspector]
        [SerializeField]public Object target;

        [SerializeField]private GameObject gameObject => target as GameObject;

        public string Name => target.name;
        public string fieldName;

        public SerializeFieldData(Object target)
        {
            mComponents = new List<string>();
            this.target = target;          
            fieldLevelIndex = 0;
            this.fieldName = target.name.Replace(" ","");            
        }

        public T GetComponent<T>() where T : Component
        {
            return GetComponent(typeof(T)) as T;
        }

        public Component GetComponent(Type fieldType)
        {
            if (gameObject == null) return null;

            return gameObject.GetComponent(fieldType);
        }


    }
}