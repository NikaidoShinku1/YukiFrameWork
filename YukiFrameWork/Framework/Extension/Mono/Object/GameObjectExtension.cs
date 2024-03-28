using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
using Object = UnityEngine.Object;

namespace YukiFrameWork
{
    [ClassAPI("Mono GameObject Extension")]
    [GUIDancePath("YukiFrameWork/Framework/Extension")]
    public static class GameObjectExtension
    {

        #region Instantiate
        public static T Instantiate<T>(this T core, Transform parent = null) where T : Object
            => Object.Instantiate(core, parent);

        public static T Instantiate<T>(this T obj) where T : Object
            => Object.Instantiate(obj);

        public static T Instantiate<T>(this T obj, Vector3 position, Quaternion quaternion) where T : Object
            => Object.Instantiate(obj, position, quaternion);

        public static T Instantiate<T>(this T obj, Vector3 position, Quaternion quaternion, Transform parent) where T : Object
            => Object.Instantiate(obj, position, quaternion, parent);

        public static T Instantiate<T>(this T obj, Transform parent, bool instanitateInWorldSpace) where T : Object
            => Object.Instantiate(obj, parent, instanitateInWorldSpace);
        #endregion

        #region Show And Hide

        public static GameObject Show(this GameObject core)
        {
            core.SetActive(true);
            return core;
        }

        public static GameObject Hide(this GameObject core)
        {
            core.SetActive(false);
            return core;
        }

        public static T Show<T>(this T component) where T : Component
        {
            Show(component.gameObject);
            return component;
        }

        public static T Hide<T>(this T component) where T : Component
        {
            Hide(component.gameObject);
            return component;
        }

        #endregion

        #region LifeCycle
        public static bool Destroy<T>(this T obj, float time) where T : Object
        {
            try
            {
                Object.Destroy(obj, time);
                return true;
            }
            catch
            {
                return false;
            }
        }       
        public static bool Destroy<T>(this T obj) where T : Object
            => Destroy(obj, 0);

        public static T DonDestroyOnLoad<T>(this T core) where T : Object
        {
            Object.DontDestroyOnLoad(core);
            return core;
        }
        #endregion

        #region Info
        public static T SetName<T>(this T core,string name) where T : Object
        {
            core.name = name;
            return core;
        }      
        public static T Layer<T>(this T component, int layer) where T : Component
        {
            Layer(component.gameObject, layer);
            return component;
        }
    
        public static GameObject Layer(this GameObject gameObject,int layer)
        {
            gameObject.layer = layer;        
            return gameObject;
        }

        public static T Layer<T>(this T component, string layerName) where T : Component
        {
            Layer(component.gameObject, layerName);
            return component;
        }

        public static GameObject Layer(this GameObject gameObject, string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
            return gameObject;
        }

        public static T Tag<T>(this T component, string tag) where T : Component
        {
            Tag(component.gameObject, tag);
            return component;
        }

        public static GameObject Tag(this GameObject gameObject, string tag)
        {
            gameObject.tag = tag;
            return gameObject;
        }
        #endregion

        #region Position
        public static T Rotate<T>(this T core, Vector3 target) where T : Component
        {
            Rotate(core.gameObject, target);
            return core;
        }

        public static GameObject Rotate(this GameObject core, Vector3 target)
        {
            core.transform.Rotate(target);
            return core;
        }


        public static GameObject SetPosition(this GameObject core, Vector3 position)
        {
            core.transform.position = position;
            return core;
        }

        public static T SetPosition<T>(this T core, Vector3 position) where T : Component
        {
            SetPosition(core.gameObject, position);
            return core;
        }


        public static GameObject SetLocalPosition(this GameObject core, Vector3 localPosition)
        {
            core.transform.localPosition = localPosition;
            return core;
        }

        public static T SetLocalPosition<T>(this T core, Vector3 localPosition) where T : Component
        {
            SetLocalPosition(core.gameObject, localPosition);
            return core;
        }

        public static GameObject SetLocalScale(this GameObject core, Vector3 localScale)
        {
            core.transform.localScale = localScale;
            return core;
        }

        public static GameObject SetParent(this GameObject core, Transform parent)
        {
            core.transform.parent = parent;
            return core;
        }

        public static T SetParent<T>(this T core, Transform parent) where T : Component
        {
            SetParent(core.gameObject, parent);
            return core;
        }

        public static T SetLocalScale<T>(this T core, Vector3 localScale) where T : Component
        {
            SetLocalScale(core.gameObject, localScale);
            return core;
        }

        public static GameObject ResetPosition(this GameObject core)
        {
            core.transform.position = Vector3.zero;
            core.transform.localScale = Vector3.one;
            core.transform.rotation = Quaternion.identity;
            return core;
        }

        public static T ResetPosition<T>(this T core) where T : Component
        {
            ResetPosition(core.gameObject);
            return core;
        }

        public static GameObject SetRotation(this GameObject core, Quaternion quaternion)
        {
            core.transform.rotation = quaternion;
            return core;
        }

        public static T SetRotation<T>(this T core, Quaternion quaternion) where T : Component
        {
            SetRotation(core.gameObject, quaternion);
            return core;
        }


        public static GameObject SetLocalRotation(this GameObject core, Quaternion quaternion)
        {
            core.transform.localRotation = quaternion;
            return core;
        }

        public static T SetLocalRotation<T>(this T core, Quaternion quaternion) where T : Component
        {
            SetLocalRotation(core.gameObject, quaternion);
            return core;
        }
        #endregion

        #region Component
        public static T GetOrAddComponent<T>(this GameObject core) where T : Component
        {
            T component = core.GetComponent<T>();

#if UNITY_2020_1_OR_NEWER
            return component ?? core.AddComponent<T>();
#else
            return component != null ? component : core.AddComponent<T>();
#endif
        }

        public static T GetOrAddComponent<T>(this Component core) where T : Component
            => GetOrAddComponent<T>(core.gameObject);

        public static T Find<T>(this Component core,string objName,bool includeInactive = false) where T : Component
        {
            return Find<T>(core.gameObject, objName,includeInactive);
        }

        public static T Find<T>(this GameObject core, string objName, bool includeInactive = false) where T : Component
        {
            return FindRoot<T>(core, objName, false,includeInactive);
        }

        public static Transform Find(this Component core, string objName, bool includeInactive = false) 
        {
            return Find(core.gameObject, objName, includeInactive);
        }

        public static Transform Find(this GameObject core, string objName, bool includeInactive = false) 
        {
            return FindRoot<Transform>(core, objName, false, includeInactive);
        }

        public static T FindOrAdd<T>(this Component core, string objName, bool includeInactive = false) where T : Component
        {
            return FindOrAdd<T>(core.gameObject, objName, includeInactive);
        }

        public static T FindOrAdd<T>(this GameObject core, string objName, bool includeInactive = false) where T : Component
        {
            if (typeof(T) == typeof(Transform))
            {
                Debug.LogError("Transform is held by default and does not trigger additions");
            }
            return FindRoot<T>(core, objName, false, includeInactive);
        }

        private static T FindRoot<T>(this GameObject core, string objName, bool isAddComponent, bool includeInactive = false) where T : Component
        {
            Transform[] transforms = core.GetComponentsInChildren<Transform>(includeInactive);

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name.Equals(objName))
                    return isAddComponent ? transforms[i].GetOrAddComponent<T>() : transforms[i].GetComponent<T>();
            }

            return null;
        }
    }
    #endregion
}
