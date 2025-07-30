using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace XFABManager
{

    public class OtherComponentAdapter : TargetComponentAdapter
    {
        private static Dictionary<string,Type> typeCaches = new Dictionary<string,Type>();

        public TargetComponentType TargetComponentType => TargetComponentType.Other;

        public void SetColor(ImageLoader loader, Color color)
        {
            // 暂不实现设置颜色功能 TODO
        }
        public Color GetColor(ImageLoader loader)
        {
            // 暂不实现获取颜色功能 TODO
            return Color.white;
        }
        public void SetValue(ImageLoader loader, ImageData imageData)
        {
            if (loader == null) return;

            if (loader.TargetComponent == null)
                loader.TargetComponent = TargetComponent(loader);

            UnityEngine.Object component = loader.TargetComponent;
            if (component == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(loader.TargetComponentType);
                if (loader.TargetComponentType == TargetComponentType.Other)
                {
                    sb.Append(" target_component_full_name:").Append(loader.target_component_full_name);
                    sb.Append(" fields_name:").Append(loader.target_component_fields_name);
                }

                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:{1} 请确认组件是否存在!", loader.gameObject.name, sb.ToString());
                return;
            }

            //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            

            Type type = GetTypeByFullName(loader.target_component_full_name);

            if (type == null) {
                Debug.LogErrorFormat("查询类型失败:{0} 请检查名称是否填写正确!", loader.target_component_full_name); 
                return;
            }

            FieldInfo fieldInfo = null;
            try
            {
                fieldInfo = type.GetField(loader.target_component_fields_name );

                if (fieldInfo != null)
                {
                    object obj = fieldInfo.GetValue(component); 

                    if (fieldInfo.FieldType == typeof(Sprite))
                    {
                        fieldInfo.SetValue(component, imageData != null ? imageData.sprite : null);
                    }
                    else
                    {
                        fieldInfo.SetValue(component, imageData != null ? imageData.texture : null);
                    }
                }
                 
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }


            PropertyInfo propertyInfo = null;
            try
            {
                propertyInfo = type.GetProperty(loader.target_component_fields_name);
                if (propertyInfo != null)
                {  
                    if (propertyInfo.PropertyType == typeof(Sprite))
                    {
                        propertyInfo.SetValue(component, imageData != null ? imageData.sprite : null);
                    }
                    else
                    {
                        propertyInfo.SetValue(component, imageData != null ? imageData.texture : null);
                    }
                }
                  
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            
        }

        public UnityEngine.Object TargetComponent(ImageLoader loader)
        {
            if (loader == null) return null;
            return loader.GetComponent(loader.target_component_full_name);
        }


        private Type GetTypeByFullName(string full_name) {
            
            if(typeCaches.ContainsKey(full_name)) return typeCaches[full_name];

            Type type = null;
             
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                type = assembly.GetType(full_name);
                if (type != null) {
                    break;
                }
            }
            typeCaches.Add(full_name, type);
            return type;
        }

    
    }

}
