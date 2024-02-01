using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;


namespace YukiFrameWork
{
    [ClassAPI("Mono GameObject Extension")]
    [GUIDancePath("YukiFrameWork/Framework/Extension")]
    public static class GameObjectExtension
    {
        public static GameObject Instantiate(this GameObject core, Transform parent = null)
            => GameObject.Instantiate(core, parent);

        public static T Instantiate<T>(this T component) where T : Component
            => Instantiate(component.gameObject).GetOrAddComponent<T>();

        public static GameObject SetPosition(this GameObject core, Vector3 position)
        {
            core.transform.position = position;
            return core;
        }

        public static T SetPosition<T>(this T core, Vector3 position) where T : Component
            => SetPosition(core.gameObject, position).GetOrAddComponent<T>();


        public static T SetLocalPosition<T>(this GameObject core, Vector3 localPosition) where T : Component
        {
            core.transform.localPosition = localPosition;
            return core.GetOrAddComponent<T>();
        }

        public static T SetLocalPosition<T>(this T core, Vector3 localPosition) where T : Component
            => SetLocalPosition<T>(core.gameObject, localPosition);

        public static T ResetPosition<T>(this GameObject core) where T : Component
        {
            core.transform.position = Vector3.zero;
            core.transform.localScale = Vector3.one;
            core.transform.rotation = Quaternion.identity;
            return core.GetOrAddComponent<T>();
        }

        public static T ResetPosition<T>(this T core) where T : Component
          => ResetPosition<T>(core.gameObject);

        public static GameObject SetRotation(this GameObject core, Quaternion quaternion)
        {
            core.transform.rotation = quaternion;
            return core;
        }

        public static T SetRotation<T>(this T core, Quaternion quaternion) where T : Component
            => SetRotation(core.gameObject, quaternion).GetOrAddComponent<T>();
      
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


    }
}
