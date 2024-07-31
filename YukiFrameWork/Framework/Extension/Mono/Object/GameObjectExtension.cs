using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using XFABManager;
using YukiFrameWork.Extension;
using Object = UnityEngine.Object;

namespace YukiFrameWork
{
    [ClassAPI("Mono GameObject Extension")]
    [GUIDancePath("YukiFrameWork/Framework/Extension")]
    public static class GameObjectExtension
    {

        #region Instantiate
        public static T Instantiate<T>(this T core, Component parent = null) where T : Object
            => Object.Instantiate(core, parent.transform);

        public static T Instantiate<T>(this T obj) where T : Object
            => Object.Instantiate(obj);

        public static T Instantiate<T>(this T obj, Vector3 position, Quaternion quaternion) where T : Object
            => Object.Instantiate(obj, position, quaternion);

        public static T Instantiate<T>(this T obj, Vector3 position, Quaternion quaternion, Transform parent) where T : Object
            => Object.Instantiate(obj, position, quaternion, parent);

        public static T Instantiate<T>(this T obj, Component parent, bool instanitateInWorldSpace) where T : Object
            => Object.Instantiate(obj, parent.transform, instanitateInWorldSpace);
        #endregion

        #region Core
        public static T Core<T>(this T core,Action<T> coreEvent)
        {
            coreEvent?.Invoke(core);
            return core;
        }
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

        public static GameObject ShowOrHide(this GameObject core,bool active)
        {
            core.SetActive(active);
            return core;
        }

        public static T ShowOrHide<T>(this T component,bool active) where T : Component
        {
            ShowOrHide(component.gameObject, active);
            return component;
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

        public static T[] Show<T>(this T[] components) where T : Component
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].Show();
            }
            return components;
        }

        public static GameObject[] Show(this GameObject[] gameObjects)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].Show();
            }
            return gameObjects;
        }

        public static T[] Hide<T>(this T[] components) where T : Component
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].Hide();
            }
            return components;
        }

        public static GameObject[] Hide(this GameObject[] gameObjects)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].Hide();
            }
            return gameObjects;
        }

        public static List<T> Show<T>(this List<T> components) where T : Component
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Show();
            }
            return components;
        }

        public static List<GameObject> Show(this List<GameObject> gameObjects)
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Show();
            }
            return gameObjects;
        }

        public static List<T> Hide<T>(this List<T> components) where T : Component
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Hide();
            }
            return components;
        }

        public static List<GameObject> Hide(this List<GameObject> gameObjects)
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Hide();
            }
            return gameObjects;
        }

        public static bool Active<T>(this T component) where T : Component
        {
            return Active(component.gameObject);
        }

        public static bool Active(this GameObject gameObject)
        {
            return gameObject.activeSelf;
        }

        public static bool ActiveInHierarchy<T>(this T component) where T : Component
        {
            return Active(component.gameObject);
        }

        public static bool ActiveInHierarchy(this GameObject gameObject)
        {
            return gameObject.activeInHierarchy;
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

        /// <summary>
        /// 判断是否已经销毁
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static bool IsDestroy<T>(this T component) where T : Object
        {
            try
            {
                return !component;
            }
            catch
            {
                return true;
            }            
        }     

        public static T DonDestroyOnLoad<T>(this T core) where T : Object
        {
            Object.DontDestroyOnLoad(core);
            return core;
        }

        public static T DestroyChildren<T>(this T core) where T : Component
        {
            DestroyChildren(core.gameObject);
            return core;
        }

        public static GameObject DestroyChildren(this GameObject core)
        {           
            var childCount = core.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var item = core.transform.GetChild(i);
                item.gameObject.Destroy();
                    
            }
            return core;
        }

        public static T DestroyChildrenWithCondition<T>(this T component,Func<Transform,bool> condition) where T : Component
        {
            DestroyChildrenWithCondition(component.gameObject, condition);
            return component;
        }

        public static GameObject DestroyChildrenWithCondition(this GameObject core,Func<Transform,bool> condition) 
        {
            var childCount = core.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var item = core.transform.GetChild(i);
                if(condition.Invoke(item))
                    item.gameObject.Destroy();                  
            }
            return core;
        }

        /// <summary>
        /// 使用对象池回收加载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        public static T UnLoadChildren<T>(this T core) where T : Component
        {
            UnLoadChildren(core.gameObject);
            return core;
        }
        /// <summary>
        /// 使用对象池回收加载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        public static GameObject UnLoadChildren(this GameObject core)
        {
            var childCount = core.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var item = core.transform.GetChild(i);
                GameObjectLoader.UnLoad(item.gameObject);

            }
            return core;
        }
        /// <summary>
        /// 使用对象池回收加载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        public static T UnLoadChildrenWithCondition<T>(this T component, Func<Transform, bool> condition) where T : Component
        {
            UnLoadChildrenWithCondition(component.gameObject, condition);
            return component;
        }
        /// <summary>
        /// 使用对象池回收加载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        public static GameObject UnLoadChildrenWithCondition(this GameObject core, Func<Transform, bool> condition)
        {
            var childCount = core.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var item = core.transform.GetChild(i);
                if (condition.Invoke(item))
                    GameObjectLoader.UnLoad(item.gameObject,true);
            }
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

        public static GameObject SetPositionX(this GameObject core, float x)
        {
            SetPosition(core.gameObject, new Vector3(x, core.transform.position.y, core.transform.position.z));
            return core;
        }

        public static GameObject SetPositionY(this GameObject core, float y) 
        {
            SetPosition(core.gameObject, new Vector3(core.transform.position.x, y, core.transform.position.z));
            return core;
        }

        public static GameObject SetPositionZ(this GameObject core, float z)
        {
            SetPosition(core.gameObject, new Vector3(core.transform.position.x, core.transform.position.y, z));
            return core;
        }

        public static T SetPosition<T>(this T core, Vector3 position) where T : Component
        {
            SetPosition(core.gameObject, position);
            return core;
        }

        public static T SetPositionX<T>(this T core, float x) where T : Component
        {
            SetPosition(core.gameObject, new Vector3(x,core.transform.position.y,core.transform.position.z));
            return core;
        }

        public static T SetPositionY<T>(this T core, float y) where T : Component
        {
            SetPosition(core.gameObject, new Vector3(core.transform.position.x, y, core.transform.position.z));
            return core;
        }

        public static T SetPositionZ<T>(this T core, float z) where T : Component
        {
            SetPosition(core.gameObject, new Vector3(core.transform.position.x, core.transform.position.y, z));
            return core;
        }

        public static GameObject SetLocalPosition(this GameObject core, Vector3 localPosition)
        {
            core.transform.localPosition = localPosition;
            return core;
        }

        public static GameObject SetLocalPositionX(this GameObject core, float x)
        {
            SetLocalPosition(core.gameObject, new Vector3(x, core.transform.position.y, core.transform.position.z));
            return core;
        }

        public static GameObject SetLocalPositionY(this GameObject core, float y)
        {
            SetLocalPosition(core.gameObject, new Vector3(core.transform.position.x, y, core.transform.position.z));
            return core;
        }

        public static GameObject SetLocalPositionZ(this GameObject core, float z)
        {
            SetLocalPosition(core.gameObject, new Vector3(core.transform.position.x, core.transform.position.y, z));
            return core;
        }

        public static T SetLocalPositionX<T>(this T core, float x) where T : Component
        {
            SetLocalPosition(core.gameObject, new Vector3(x, core.transform.position.y, core.transform.position.z));
            return core;
        }

        public static T SetLocalPositionY<T>(this T core, float y) where T : Component
        {
            SetLocalPosition(core.gameObject, new Vector3(core.transform.position.x, y, core.transform.position.z));
            return core;
        }

        public static T SetLocalPositionZ<T>(this T core, float z) where T : Component
        {
            SetLocalPosition(core.gameObject, new Vector3(core.transform.position.x, core.transform.position.y, z));
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

        public static T SetLocalScaleX<T>(this T core, float x) where T : Component
        {
            SetLocalScale(core.gameObject, new Vector3(x, core.transform.localScale.y, core.transform.localScale.z));
            return core;
        }

        public static T SetLocalScaleY<T>(this T core, float y) where T : Component
        {
            SetLocalScale(core.gameObject, new Vector3(core.transform.localScale.x, y, core.transform.localScale.z));
            return core;
        }

        public static T SetLocalScaleZ<T>(this T core, float z) where T : Component
        {
            SetLocalScale(core.gameObject, new Vector3(core.transform.localScale.x, core.transform.localScale.y, z));
            return core;
        }

        public static GameObject SetParent(this GameObject core, GameObject parent)
        {
            core.transform.SetParent(parent.transform);
            return core;
        }

        public static GameObject SetParent(this GameObject core, Component parent)
        {
            core.transform.SetParent(parent.transform);
            return core;
        }

        public static T SetParent<T>(this T core, Transform parent) where T : Component
        {
            core.transform.SetParent(parent);
            return core;
        }

        public static T SetAsLastSibling<T>(this T core) where T : Component
        {
            core.transform.SetAsLastSibling();
            return core;
        }

        public static GameObject SetAsLastSibling(this GameObject core)
        {
            core.transform.SetAsLastSibling();
            return core;
        }

        public static T SetAsFirstSibling<T>(this T core) where T : Component
        {
            core.transform.SetAsFirstSibling();
            return core;
        }

        public static GameObject SetAsFirstSibling(this GameObject core)
        {
            core.transform.SetAsFirstSibling();
            return core;
        }

        public static T SetParent<T>(this T core, Component parent) where T : Component
        {
            SetParent(core.gameObject, parent.transform);
            return core;
        }

        public static T SetLocalScale<T>(this T core, Vector3 localScale) where T : Component
        {
            SetLocalScale(core.gameObject, localScale);
            return core;
        }

        public static GameObject SetLocalScaleX(this GameObject core, float x)
        {
            SetLocalScale(core.gameObject, new Vector3(x, core.transform.localScale.y, core.transform.localScale.z));
            return core;
        }

        public static GameObject SetLocalScaleY(this GameObject core, float y)
        {
            SetLocalScale(core.gameObject, new Vector3(core.transform.localScale.x, y, core.transform.localScale.z));
            return core;
        }

        public static GameObject SetLocalScaleZ(this GameObject core, float z)
        {
            SetLocalScale(core.gameObject, new Vector3(core.transform.localScale.x, core.transform.localScale.y, z));
            return core;
        }

        public static T SetLocalEulerAngles<T>(this T core,Vector3 localEnlerAngles) where T : Component
        {
            SetLocalEulerAngles(core.gameObject, localEnlerAngles);
            return core;
        }

        public static T SetLocalEulerAnglesX<T>(this T core, float x) where T : Component
        {
            SetLocalEulerAngles(core.gameObject, new Vector3(x, core.transform.localEulerAngles.y, core.transform.localEulerAngles.z));
            return core;
        }

        public static T SetLocalEulerAnglesY<T>(this T core, float y) where T : Component
        {
            SetLocalEulerAngles(core.gameObject, new Vector3(core.transform.localEulerAngles.x, y, core.transform.localEulerAngles.z));
            return core;
        }

        public static T SetLocalEulerAnglesZ<T>(this T core, float z) where T : Component
        {
            SetLocalEulerAngles(core.gameObject, new Vector3(core.transform.localEulerAngles.x, core.transform.localEulerAngles.y, z));
            return core;
        }

        public static GameObject SetLocalEulerAnglesX(this GameObject core, float x)
        {
            SetLocalEulerAngles(core.gameObject, new Vector3(x, core.transform.localEulerAngles.y, core.transform.localEulerAngles.z));
            return core;
        }

        public static GameObject SetLocalEulerAnglesY(this GameObject core, float y)
        {
            SetLocalEulerAngles(core.gameObject, new Vector3(core.transform.localEulerAngles.x, y, core.transform.localEulerAngles.z));
            return core;
        }

        public static GameObject SetLocalEulerAnglesZ(this GameObject core, float z)
        {
            SetLocalEulerAngles(core.gameObject, new Vector3(core.transform.localEulerAngles.x, core.transform.localEulerAngles.y, z));
            return core;
        }

        public static GameObject SetLocalEulerAngles(this GameObject core, Vector3 localEnlerAngles) 
        {
            core.transform.localEulerAngles = localEnlerAngles;
            return core;
        }

        public static T SetEulerAngles<T>(this T core, Vector3 enlerAngles) where T : Component
        {
            SetEulerAngles(core.gameObject, enlerAngles);
            return core;
        }

        public static T SetEulerAnglesX<T>(this T core, float x) where T : Component
        {
            SetEulerAngles(core.gameObject, new Vector3(x, core.transform.eulerAngles.y, core.transform.eulerAngles.z));
            return core;
        }

        public static T SetEulerAnglesY<T>(this T core, float y) where T : Component
        {
            SetEulerAngles(core.gameObject, new Vector3(core.transform.eulerAngles.x, y, core.transform.eulerAngles.z));
            return core;
        }

        public static T SetEulerAnglesZ<T>(this T core, float z) where T : Component
        {
            SetEulerAngles(core.gameObject, new Vector3(core.transform.eulerAngles.x, core.transform.eulerAngles.y, z));
            return core;
        }


        public static GameObject SetEulerAngles(this GameObject core, Vector3 enlerAngles)
        {
            core.transform.eulerAngles = enlerAngles;
            return core;
        }

        public static GameObject SetEulerAnglesX(this GameObject core, float x)
        {
            SetEulerAngles(core.gameObject, new Vector3(x, core.transform.eulerAngles.y, core.transform.eulerAngles.z));
            return core;
        }

        public static GameObject SetEulerAnglesY(this GameObject core, float y)
        {
            SetEulerAngles(core.gameObject, new Vector3(core.transform.eulerAngles.x, y, core.transform.eulerAngles.z));
            return core;
        }

        public static GameObject SetEulerAnglesZ(this GameObject core, float z)
        {
            SetEulerAngles(core.gameObject, new Vector3(core.transform.eulerAngles.x, core.transform.eulerAngles.y, z));
            return core;
        }

        public static T SetLocalEulerAngles2D<T>(this T core, Vector2 localEnlerAngles) where T : Component
        {
            return SetLocalEulerAngles(core, localEnlerAngles);
        }

        public static GameObject SetLocalEulerAngles2D(this GameObject core, Vector2 localEnlerAngles)
        {
            return SetLocalEulerAngles(core, localEnlerAngles);
        }

        public static T SetEulerAngles2D<T>(this T core, Vector2 enlerAngles) where T : Component
        {
            return SetEulerAngles(core, enlerAngles);
        }

        public static GameObject SetEulerAngles2D(this GameObject core, Vector2 enlerAngles)
        {
            return SetEulerAngles(core, enlerAngles);
        }

        public static GameObject ResetIdentity(this GameObject core)
        {
            core.transform.position = Vector3.zero;
            core.transform.localScale = Vector3.one;
            core.transform.rotation = Quaternion.identity;
            return core;
        }

        public static T ResetIdentity<T>(this T core) where T : Component
        {
            ResetIdentity(core.gameObject);
            return core;
        }

        public static GameObject ResetLocalIdentity(this GameObject core)
        {
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one;
            core.transform.localRotation = Quaternion.identity;
            return core;
        }

        public static T ResetLocalIdentity<T>(this T core) where T : Component
        {
            ResetLocalIdentity(core.gameObject);
            return core;
        }

        public static T SetLocalPositionIdentity<T>(this T core) where T : Component
        {
            SetLocalPositionIdentity(core.gameObject);
            return core;
        }

        public static GameObject SetLocalPositionIdentity(this GameObject core)
        {
            core.transform.localPosition = Vector3.zero;
            return core;
        }

        public static T SetPositionIdentity<T>(this T core) where T : Component
        {
            SetPositionIdentity(core.gameObject);
            return core;
        }

        public static GameObject SetPositionIdentity(this GameObject core)
        {
            core.transform.position = Vector3.zero;
            return core;
        }

        public static T SetLocalRotationIdentity<T>(this T core) where T : Component
        {
            SetLocalRotationIdentity(core.gameObject);
            return core;
        }

        public static GameObject SetLocalRotationIdentity(this GameObject core)
        {
            core.transform.localRotation = Quaternion.identity;
            return core;
        }

        public static T SetRotationIdentity<T>(this T core) where T : Component
        {
            SetRotationIdentity(core.gameObject);
            return core;
        }

        public static GameObject SetRotationIdentity(this GameObject core)
        {
            core.transform.rotation = Quaternion.identity;
            return core;
        }      

        public static T SetLocalScaleIdentity<T>(this T core) where T : Component
        {
            SetLocalScaleIdentity(core.gameObject);
            return core;
        }

        public static GameObject SetLocalScaleIdentity(this GameObject core)
        {
            core.transform.localScale = Vector3.one;
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

        public static T SetPosition2D<T>(this T core, Vector2 position) where T : Component
        {
            return SetPosition(core, position);
        }

        public static GameObject SetPosition2D(this GameObject core, Vector2 position)
        {
            return SetPosition(core, position);
        }

        public static T SetLocalPosition2D<T>(this T core, Vector2 position) where T : Component
        {
            return SetLocalPosition(core, position);
        }

        public static GameObject SetLocalPosition2D(this GameObject core, Vector2 position)
        {
            return SetLocalPosition(core, position);
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

        public static GameObject FindRootGameObject(this Scene scene,string name)
        {
            GameObject[] gameObjects = scene.GetRootGameObjects();

            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i].name == name)
                    return gameObjects[i];
            }

            return null;
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
                Debug.LogWarning("Transform is held by default and does not trigger additions");
            }
            return FindRoot<T>(core, objName, true, includeInactive);
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

        public static FastList<(Transform, T2)> QueryComponentsInChildren<T2>(this GameObject core, bool includeInactive = false) where T2 : Component
        {
            FastList<(Transform, T2)> enumerator = new FastList<(Transform, T2)>();

            T2[] values = core.GetComponentsInChildren<T2>(includeInactive);  

            for (int i = 0; i < values.Length; i++)
            {
                enumerator.Add((values[i].transform, values[i]));
            }
            return enumerator;
        }

        public static FastList<(Transform, T2)> QueryComponentsInChildren<T2>(this Component core, bool includeInactive = false) where T2 : Component
        {
            return QueryComponentsInChildren<T2>(core.gameObject,includeInactive);
        }


        public static FastList<(Transform,T1, T2)> QueryComponentsInChildren<T1, T2>(this GameObject core, bool includeInactive = false) where T1 : Component where T2 : Component
        {
            FastList<(Transform,T1, T2)> enumerator = new FastList<(Transform, T1, T2)>();
            FastList<(Transform, T1)> values = QueryComponentsInChildren<T1>(core,includeInactive);

            for (int i = 0; i < values.Count; i++)
            {
                (Transform, T1) value = values[i];
                T2 t2 = value.Item1.GetComponent<T2>();
                if (!t2) continue;
                enumerator.Add((value.Item1, value.Item2, t2));
            }
            return enumerator;
        }

        public static FastList<(Transform, T1, T2)> QueryComponentsInChildren<T1, T2>(this Component core, bool includeInactive = false) where T1 : Component where T2 : Component
        {
            return QueryComponentsInChildren<T1, T2>(core.gameObject, includeInactive);
        }

        public static FastList<(Transform, T1, T2,T3)> QueryComponentsInChildren<T1, T2,T3>(this GameObject core, bool includeInactive = false) where T1 : Component where T2 : Component where T3 : Component
        {
            FastList<(Transform, T1, T2, T3)> enumerator = new FastList<(Transform, T1, T2, T3)>();

            FastList<(Transform, T1, T2)> values = QueryComponentsInChildren<T1, T2>(core, includeInactive);

            for (int i = 0; i < values.Count; i++)
            {
                (Transform, T1,T2) value = values[i];
                T3 t3 = value.Item1.GetComponent<T3>();
                if (!t3) continue;
                enumerator.Add((value.Item1, value.Item2, value.Item3,t3));
            }

            return enumerator;
        }

        public static FastList<(Transform, T1, T2,T3)> QueryComponentsInChildren<T1, T2, T3>(this Component core, bool includeInactive = false) where T1 : Component where T2 : Component where T3 : Component
        {
            return QueryComponentsInChildren<T1, T2,T3>(core.gameObject, includeInactive);
        }

        public static FastList<(Transform, T1, T2, T3, T4)> QueryComponentsInChildren<T1, T2, T3,T4>(this GameObject core, bool includeInactive = false) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
        {
            FastList<(Transform, T1, T2, T3,T4)> enumerator = new FastList<(Transform, T1, T2, T3, T4)>();

            FastList<(Transform, T1, T2,T3)> values = QueryComponentsInChildren<T1, T2,T3>(core, includeInactive);

            for (int i = 0; i < values.Count; i++)
            {
                (Transform, T1, T2,T3) value = values[i];
                T4 t4 = value.Item1.GetComponent<T4>();
                if (!t4) continue;
                enumerator.Add((value.Item1, value.Item2, value.Item3,value.Item4, t4));
            }
            return enumerator;
        }

        public static FastList<(Transform, T1, T2, T3,T4)> QueryComponentsInChildren<T1, T2, T3,T4>(this Component core, bool includeInactive = false) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
        {
            return QueryComponentsInChildren<T1, T2, T3,T4>(core.gameObject, includeInactive);
        }

        public static FastList<T> Condition<T>(this FastList<T> list,Predicate<T> condition)
        {
            return list.FindAll(condition).ToFastList();
        }

        public static FastList<T> ToFastList<T>(this IEnumerable<T> list)
            => new FastList<T>(list); 

        #endregion

        #region Action
        public static Action ToSystemAction(this UnityEvent uEvent)
        {
            return () => uEvent?.Invoke();
        }

        public static Action<T> ToSystemAction<T>(this UnityEvent<T> uEvent)
        {
            return v => uEvent?.Invoke(v);
        }

        public static Action<T,K> ToSystemAction<T, K>(this UnityEvent<T, K> uEvent)
        {
            return (t,k) => uEvent?.Invoke(t,k);
        }

        public static Action<T, K, Q> ToSystemAction<T, K, Q>(this UnityEvent<T, K, Q> uEvent)
        {
            return (t,k,q) => uEvent?.Invoke(t,k,q);
        }

        public static Action<T, K, Q, R> ToSystemAction<T, K, Q, R>(this UnityEvent<T, K, Q, R> uEvent)
        {
            return (t, k, q,r) => uEvent?.Invoke(t, k, q,r);
        }

        public static UnityAction ToUnityAction(this Action uEvent)
        {
            return () => uEvent?.Invoke();
        }

        public static UnityAction<T> ToUnityAction<T>(this Action<T> uEvent)
        {
            return v => uEvent?.Invoke(v);
        }

        public static UnityAction<T, K> ToUnityAction<T, K>(this Action<T, K> uEvent)
        {
            return (t, k) => uEvent?.Invoke(t, k);
        }

        public static UnityAction<T, K, Q> ToUnityAction<T, K, Q>(this Action<T, K, Q> uEvent)
        {
            return (t, k, q) => uEvent?.Invoke(t, k, q);
        }

        public static UnityAction<T, K, Q, R> ToUnityAction<T, K, Q, R>(this Action<T, K, Q, R> uEvent)
        {
            return (t, k, q, r) => uEvent?.Invoke(t, k, q, r);
        }

        public static UnityEvent ToUnityEvent(this Action uEvent)
        {
            var u = new UnityEvent();
            u.AddListener(() => uEvent?.Invoke());
            return u;
        }

        public static UnityEvent<T> ToUnityEvent<T>(this Action<T> uEvent)
        {
            var u = new UnityEvent<T>();
            u.AddListener(v => uEvent?.Invoke(v));
            return u;
        }

        public static UnityEvent<T, K> ToUnityEvent<T, K>(this Action<T, K> uEvent)
        {
            var u = new UnityEvent<T,K>();
            u.AddListener((v,k) => uEvent?.Invoke(v,k));
            return u;
        }

        public static UnityEvent<T, K, Q> ToUnityEvent<T, K, Q>(this Action<T, K, Q> uEvent)
        {
            var u = new UnityEvent<T, K,Q>();
            u.AddListener((v, k,q) => uEvent?.Invoke(v, k,q));
            return u;
        }

        public static UnityEvent<T, K, Q, R> ToUnityEvent<T, K, Q, R>(this Action<T, K, Q, R> uEvent)
        {
            var u = new UnityEvent<T, K, Q,R>();
            u.AddListener((v, k, q,r) => uEvent?.Invoke(v, k, q,r));
            return u;
        }
        #endregion
    }

    public static class BindablePropertyOrEventExtension
    {

        /// <summary>
        /// 注销事件，并且绑定MonoBehaviour生命周期,当销毁时自动清空事件
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitGameObjectDestroy<Component>(this IUnRegister property, Component component, Action onFinish = null) where Component : UnityEngine.Component
        {
            UnRegisterWaitGameObjectDestroy(property, component.gameObject, onFinish);
        }

        /// <summary>
        /// 注销事件，并且绑定MonoBehaviour生命周期,当销毁时自动清空事件
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitGameObjectDestroy(this IUnRegister property, UnityEngine.GameObject gameObject, Action onFinish = null)
        {
            if (!gameObject.TryGetComponent(out OnGameObjectTrigger objectSend))
            {
                objectSend = gameObject.AddComponent<OnGameObjectTrigger>();
            }
            objectSend.AddUnRegister(property);

            objectSend.PushFinishEvent(onFinish);

        }

        /// <summary>
        /// 注销事件，并且绑定MonoBehaviour生命周期,当失活时自动清空事件
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitGameObjectDisable<Component>(this IUnRegister property, Component component) where Component : UnityEngine.Component
        {
            UnRegisterWaitGameObjectDisable(property, component.gameObject);
        }

        /// <summary>
        /// 注销事件，并且绑定MonoBehaviour生命周期,当失活时自动清空事件
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitGameObjectDisable(this IUnRegister property, UnityEngine.GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(out OnGameObjectTrigger objectSend))
            {
                objectSend = gameObject.AddComponent<OnGameObjectTrigger>();
            }
            objectSend.AddUnRegisterByDisable(property);

        }

        public static void CancelGameObjectWithDestroy(this GameObject core,Action onFinish)
        {
            EasyEvent easyEvent = new EasyEvent();
            var info = easyEvent.RegisterEvent(onFinish);
            info.UnRegisterWaitGameObjectDestroy(core);
        }

        public static void CancelGameObjectWithDestroy(this Component core, Action onFinish)
        {
            CancelGameObjectWithDestroy(core.gameObject, onFinish);
        }

        public static void CancelGameObjectWithDisable(this GameObject core, Action onFinish)
        {
            EasyEvent easyEvent = new EasyEvent();
            var info = easyEvent.RegisterEvent(onFinish);
            info.UnRegisterWaitGameObjectDisable(core);
        }

        public static void CancelGameObjectWithDisable(this Component core, Action onFinish)
        {
            CancelGameObjectWithDisable(core.gameObject, onFinish);
        }

        /// <summary>
        /// 注销事件，并且绑定当前场景
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitSceneUnLoad<Component>(this IUnRegister property, Action onFinish = null) where Component : UnityEngine.Component
        {
            UnRegisterWaitGameObjectDestroy(property, SceneListener.Instance, onFinish);
        }

        #region LayerMask
        // Fix编码
        /// <summary>
        /// 打开或关闭某个层级
        /// </summary>
        /// <param name="layerMask"></param>
        /// <param name="layerName">层级名称</param>
        /// <param name="open"></param>
        public static void Set(this ref LayerMask layerMask, string layerName, bool open)
        {
            layerMask.Set(LayerMask.NameToLayer(layerName), open);
        }

        /// <summary>
        /// 打开或关闭某个层级
        /// </summary>
        /// <param name="layerMask"></param>
        /// <param name="layer">层级</param>
        /// <param name="open"></param>
        public static void Set(this ref LayerMask layerMask, int layer, bool open)
        {
            if (open)
            {
                // 打开某个层级
                layerMask = layerMask.value | (1 << layer);
            }
            else
            {
                // 关闭某个层级
                layerMask = layerMask.value & ~(1 << layer);
            }

        }


        /// <summary>
        /// 判断某个层级是否打开
        /// </summary>
        /// <param name="layerMask"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static bool Contains(this ref LayerMask layerMask, string layerName)
        {

            return layerMask.Contains(LayerMask.NameToLayer(layerName));
        }

        /// <summary>
        /// 判断某个层级是否打开
        /// </summary>
        /// <param name="layerMask"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool Contains(this ref LayerMask layerMask, int layer)
        {
            int v = layerMask & (1 << layer);
            return v != 0;
        }

        /// <summary>
        /// 获取ProjectSetting Physics2D中某一个层级和其他层级的碰撞关系
        /// </summary>
        /// <param name="layerMask"></param>
        /// <param name="layer"></param>
        public static void Physics2DSetting(this ref LayerMask layerMask, int layer)
        {

            List<string> layers = new List<string>();

            // 遍历所有层级
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                // 跳过空的层级
                if (string.IsNullOrEmpty(name)) continue;
                bool ignore = Physics2D.GetIgnoreLayerCollision(layer, i);
                // 跳过不能碰撞的层级
                if (ignore) continue;
                layers.Add(name);
            }

            layerMask = LayerMask.GetMask(layers.ToArray());
        }

        /// <summary>
        /// 获取ProjectSetting Physics中某一个层级和其他层级的碰撞关系
        /// </summary>
        /// <param name="layerMask"></param>
        /// <param name="layer"></param>
        public static void PhysicsSetting(this ref LayerMask layerMask, int layer)
        {

            List<string> layers = new List<string>();

            // 遍历所有层级
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                // 跳过空的层级
                if (string.IsNullOrEmpty(name)) continue;
                bool ignore = Physics.GetIgnoreLayerCollision(layer, i);
                // 跳过不能碰撞的层级
                if (ignore) continue;
                layers.Add(name);
            }

            layerMask = LayerMask.GetMask(layers.ToArray());
        }
        #endregion
    }
}
