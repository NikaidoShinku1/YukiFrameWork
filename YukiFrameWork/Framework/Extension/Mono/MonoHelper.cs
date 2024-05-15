using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using YukiFrameWork.Events;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
#if UNITY_EDITOR

    [CustomEditor(typeof(MonoHelper))]
    public class MonoHelperInspector : Editor 
    {
        private SerializedProperty coreProperty;
        private void Awake()
        {
            MonoHelper helper = target as MonoHelper;
            if (helper == null) return;
        }

        private void OnEnable()
        {
            coreProperty = serializedObject.FindProperty("cores");
        }

        public override void OnInspectorGUI()
        {
            MonoHelper helper = target as MonoHelper;
            if (helper == null) return;

            if (!Application.isPlaying && !helper.IsAgree)
            {
                EditorGUILayout.HelpBox("Yuki非常不建议在Editor状态下添加MonoHelper,可能会引发生命周期顺序不一而导致的问题。Helper应该是运行时通过代码调用生成!如你确保自己的逻辑处理以及生命周期触发完全没有问题,那么可以在Editor添加", MessageType.Error);

                EditorGUILayout.Space();
                if (GUILayout.Button("我已确认警告",GUILayout.Height(30)))
                {
                    helper.IsAgree = true;
                }
            }
            else base.OnInspectorGUI();
        }
    }

#endif
    
    [ClassAPI("MonoBehaviour的普通拓展类")]
    [GUIDancePath("YukiFrameWork/Framework/Extension")]
    public class MonoHelper : SingletonMono<MonoHelper>
    {
        internal bool IsAgree = false;
        private event Action<MonoHelper> onUpdateEvent;

        private event Action<MonoHelper> onLateUpdateEvent;

        private event Action<MonoHelper> onFixedUpdateEvent;

        private event Action<MonoHelper> onDestroyEvent;

        public static void Update_AddListener(Action<MonoHelper> onUpdateEvent)
        {          
            Instance.onUpdateEvent += onUpdateEvent;
        }

        public static void FixedUpdate_AddListener(Action<MonoHelper> onFixedUpdateEvent)
        {
            Instance.onFixedUpdateEvent += onFixedUpdateEvent;
        }

        public static void LateUpdate_AddListener(Action<MonoHelper> onLateUpdateEvent)
        {
            Instance.onLateUpdateEvent += onLateUpdateEvent;
        }

        public static void Update_RemoveListener(Action<MonoHelper> onUpdateEvent)
        {
            Instance.onUpdateEvent -= onUpdateEvent;
        }

        public static void FixedUpdate_RemoveListener(Action<MonoHelper> onFixedUpdateEvent)
        {
            Instance.onFixedUpdateEvent -= onFixedUpdateEvent;
        }

        public static void LateUpdate_RemoveListener(Action<MonoHelper> onLateUpdateEvent)
        {
            Instance.onLateUpdateEvent -= onLateUpdateEvent;
        }

        public static void Destroy_AddListener(Action<MonoHelper> onDestroyEvent)
        {
            Instance.onDestroyEvent += onDestroyEvent;
        }

        public static void Destroy_RemoveListener(Action<MonoHelper> onDestroyEvent)
        {
            Instance.onDestroyEvent -= onDestroyEvent;
        }

        public static Coroutine Start(IEnumerator enumerator)
        {
            if (ReferenceEquals(I,null)) return null;
            return I.StartCoroutine(enumerator);
        }
        public static void Stop(Coroutine coroutine)
        {
            if (ReferenceEquals(I, null)) return;
            I.StopCoroutine(coroutine);
        }

        private void Update()
        {
            onUpdateEvent?.Invoke(this);
        }

        private void FixedUpdate()
        {
            onFixedUpdateEvent?.Invoke(this);
        }

        private void LateUpdate()
        {
            onLateUpdateEvent?.Invoke(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();          
            onUpdateEvent = null;
            onFixedUpdateEvent = null;
            onLateUpdateEvent = null;
            onDestroyEvent?.Invoke(this);
            onDestroyEvent = null;

        }
    }
}