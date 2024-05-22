using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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
            core.transform.SetParent(parent);
            return core;
        }

        public static GameObject SetParent(this GameObject core, Component parent)
        {
            core.transform.SetParent(parent.transform);
            return core;
        }

        public static T SetParent<T>(this T core, Transform parent) where T : Component
        {
            SetParent(core.gameObject, parent);
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

        public static T SetLocalEnlerAngles<T>(this T core,Vector3 localEnlerAngles) where T : Component
        {
            SetLocalEnlerAngles(core.gameObject, localEnlerAngles);
            return core;
        }

        public static GameObject SetLocalEnlerAngles(this GameObject core, Vector3 localEnlerAngles) 
        {
            core.transform.localEulerAngles = localEnlerAngles;
            return core;
        }

        public static T SetEnlerAngles<T>(this T core, Vector3 enlerAngles) where T : Component
        {
            SetEnlerAngles(core.gameObject, enlerAngles);
            return core;
        }

        public static GameObject SetEnlerAngles(this GameObject core, Vector3 enlerAngles)
        {
            core.transform.eulerAngles = enlerAngles;
            return core;
        }

        public static T SetLocalEnlerAngles2D<T>(this T core, Vector2 localEnlerAngles) where T : Component
        {
            return SetLocalEnlerAngles(core, localEnlerAngles);
        }

        public static GameObject SetLocalEnlerAngles2D(this GameObject core, Vector2 localEnlerAngles)
        {
            return SetLocalEnlerAngles(core, localEnlerAngles);
        }

        public static T SetEnlerAngles2D<T>(this T core, Vector2 enlerAngles) where T : Component
        {
            return SetEnlerAngles(core, enlerAngles);
        }

        public static GameObject SetEnlerAngles2D(this GameObject core, Vector2 enlerAngles)
        {
            return SetEnlerAngles(core, enlerAngles);
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
        /// 注销事件，并且绑定MonoBehaviour生命周期,当销毁的时自动清空事件
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitGameObjectDestroy<Component>(this IUnRegister property, Component component, Action onFinish = null) where Component : UnityEngine.Component
        {
            UnRegisterWaitGameObjectDestroy(property, component.gameObject, onFinish);
        }

        /// <summary>
        /// 注销事件，并且绑定MonoBehaviour生命周期,当销毁的时自动清空事件
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

        /// <summary>
        /// 注销事件，并且绑定当前场景
        /// </summary>
        /// <param name="gameObject">GameObject</param>
        public static void UnRegisterWaitSceneUnLoad<Component>(this IUnRegister property, Action onFinish = null) where Component : UnityEngine.Component
        {
            UnRegisterWaitGameObjectDestroy(property, SceneListener.Instance, onFinish);
        }   
    }
}
