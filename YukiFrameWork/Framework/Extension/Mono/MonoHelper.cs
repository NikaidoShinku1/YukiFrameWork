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

            serializedObject.Update();
            foreach (var index in helper.GetCoreIndex())
            {               
                EditorGUILayout.PropertyField(coreProperty.GetArrayElementAtIndex(index), new GUIContent((EventEditorInfo.IsEN ? "Logger" : "注册器 ")),true);
            }           

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
    
    [ClassAPI("MonoBehaviour的普通拓展类")]
    [GUIDancePath("YukiFrameWork/Framework/Extension")]
    public class MonoHelper : SingletonMono<MonoHelper>
    {
        [Serializable]
        public class MonoEventCore
        {
            [Header("事件生命周期:")]
            public UpdateStatus updateStatus;
            [Header("Event")]
            public UnityEvent<MonoHelper> onMonoEvent;
        }
      
        [SerializeField, Header("可视化的事件注册")] private MonoEventCore[] cores = new MonoEventCore[] 
        {
            new MonoEventCore
            {
                updateStatus = UpdateStatus.OnUpdate
            },
            new MonoEventCore
            {
                updateStatus = UpdateStatus.OnFixedUpdate,
            },
            new MonoEventCore 
            {
                updateStatus = UpdateStatus.OnLateUpdate
            }
        };

#if UNITY_EDITOR
        [MenuItem("GameObject/YukiFrameWork/MonoHelper")]
        public static void CreateHelperTool()
        {
            
            GameObject obj = Selection.activeGameObject;
#if UNITY_2020_1_OR_NEWER
            obj ??= new GameObject("MonoBehaviour Helper");
#endif
            Undo.RegisterCreatedObjectUndo(obj, "Create Mono Helper");

            obj.AddComponent<MonoHelper>();

            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }  

        public IEnumerable<int> GetCoreIndex()
        {
            for (int i = 0; i < cores.Length; i++)
            {
                yield return i;
            }
        }
#endif
        protected override void Awake()
        {
            base.Awake();           

            Action<MonoHelper> onEvent = null;
            foreach(var core in cores)
            {
                onEvent = helper => core.onMonoEvent?.Invoke(helper);
               
                switch (core.updateStatus)
                {
                    case UpdateStatus.OnUpdate:
                        Update_AddListener(onEvent);
                        break;
                    case UpdateStatus.OnFixedUpdate:
                        FixedUpdate_AddListener(onEvent);
                        break;
                    case UpdateStatus.OnLateUpdate:
                        LateUpdate_AddListener(onEvent);
                        break;                 
                }
            };
           
        }    
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