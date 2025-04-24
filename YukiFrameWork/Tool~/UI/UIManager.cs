using UnityEngine;
using System;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using YukiFrameWork.Pools;
using UnityEngine.UI;
using System.Reflection;
using XFABManager;
using System.Linq;

namespace YukiFrameWork.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private UIManager() { }
        public Canvas Canvas { get; private set; }
        public RectTransform transform => Canvas.transform as RectTransform;
        public CanvasScaler CanvasScaler { get; private set; }
        public GraphicRaycaster GraphicRaycaster { get; private set; }

        public EventSystem EventSystem { get; private set; }
        private Dictionary<UILevel, RectTransform> levelDicts = DictionaryPools<UILevel, RectTransform>.Get();

        internal Dictionary<Type,IPanel> panelCore = new Dictionary<Type,IPanel>();

        internal UIPrefabExector Exector { get; private set; }

        //加载安全锁
        private bool reloadLevelSafe = false;
        private bool reloadSystemSafe = false;
        public override void OnInit()
        {
#if UNITY_2023_1_OR_NEWER
            Canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(x => x.name.Equals("UIRoot"));
#else
            Canvas = Object.FindObjectsOfType<Canvas>(true).FirstOrDefault(x => x.name.Equals("UIRoot"));
#endif
            if (Canvas == null)
                Canvas = Resources.Load<Canvas>("UIRoot").Instantiate();
            else reloadLevelSafe = true;
          
            if (Canvas == null)
            {
                throw new Exception("UIRoot丢失，请重新导入UI模块!");
            }

            CanvasScaler = Canvas.GetComponent<CanvasScaler>();
            GraphicRaycaster = Canvas.GetComponent<GraphicRaycaster>();
           
#if UNITY_2023_1_OR_NEWER
            var eventSystem = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
#else
            var eventSystem = Object.FindObjectOfType<EventSystem>(true);
#endif
            if (eventSystem == null)
            {
                eventSystem = new GameObject(nameof(EventSystem)).AddComponent<EventSystem>();
                eventSystem.GetOrAddComponent<StandaloneInputModule>();
            }
            else reloadSystemSafe = true;

            Exector = Canvas.GetComponentInChildren<UIPrefabExector>(true);

            Exector ??= new GameObject("UIPrefabRoot").AddComponent<UIPrefabExector>();

            Exector.SetParent(Canvas);
            Exector.InitExector();

            EventSystem = eventSystem.SetParent(Canvas.transform);

            Object.DontDestroyOnLoad(Canvas.gameObject);

            LogKit.I("UIKit Initialization Succeeded!");           

            UpdateScreenAspect();
            ScreenTool.OnScreenChanged  .RegisterEvent(UpdateScreenAspect);
        }
        int currentWidth;
        int currentHeight;

        /// <summary>
        /// 可根据当前分辨率进行更新CanvasScaler画布比例方法
        /// </summary>
        public void UpdateScreenAspect()
        {
            if (Screen.width == currentWidth || Screen.height == currentHeight)
                return;

            // 计算出比例
            float aspect = (float)Screen.width / Screen.height;
            float inverse_lerp;
            if (IsLandscape())
                inverse_lerp = Mathf.InverseLerp(1.33f, 1.77f, aspect); // 12:9 ~ 16:9  
            else
                inverse_lerp = Mathf.InverseLerp(9.0f / 16, 9.0f / 12, aspect); // 

            CanvasScaler.matchWidthOrHeight = inverse_lerp;

            currentWidth = Screen.width;
            currentHeight = Screen.height;
        }

        private bool IsLandscape()
        {
            return Screen.width > Screen.height;
        }

        private bool IsLevelInited = false;
        internal void InitLevel()
        {
            if (IsLevelInited) return;
            IsLevelInited = true;
            for (int i = 0; i < (int)UILevel.Top + 1; i++)
            {
                UILevel level = (UILevel)Enum.GetValues(typeof(UILevel)).GetValue(i);

                RectTransform transform = new GameObject(level.ToString()).AddComponent<RectTransform>();
                transform.SetRectTransformInfo(Canvas);
                var image = transform.gameObject.AddComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0);                
                image.raycastTarget = false;
                levelDicts.Add(level, transform);
               
            }           
        }
        [RuntimeInitializeOnLoadMethod]
        private static void ReLoadSceneLevel()
        {           
            if (!I.IsLevelInited) return;
            if(I.reloadLevelSafe)
            for (int i = 0; i < I.Canvas.transform.childCount; i++)
            {
                I.Canvas.transform.GetChild(i).Hide().Show();
            }

            if (I.reloadSystemSafe)
            {
                I.EventSystem.Hide().Show();
            }

            I.Exector.Hide().Show();
            MonoHelper.ApplicationQuit_AddListener(_ => UIKit.Release());
        }

        public RectTransform GetPanelLevel(UILevel level)
        {
            levelDicts.TryGetValue(level, out var transform);
            return transform;
        }

        internal void AddPanelCore(IPanel panel)
        {
            panelCore[panel.GetType()] = panel;         
        }                     

        internal T GetPanelCore<T>() where T : class, IPanel
        {
            panelCore.TryGetValue(typeof(T),out var value);
            return (T)value;
        }

        public IPanel GetPanelCore(string name)
            => panelCore.FirstOrDefault(x => x.Value.gameObject.name == name).Value;

        [Obsolete("请直接调用UIKit.Release方法进行对面板的完全释放操作。不需要进行额外的释放操作")]
        public void Release(MonoHelper monoHelper = null)
        {
            UIKit.Release();
        }

        public override void OnDestroy()
        {
            levelDicts.Clear();
            reloadLevelSafe = false;
            reloadSystemSafe = false;
            IsLevelInited = false;

            ScreenTool.OnScreenChanged.UnRegister(UpdateScreenAspect);
            Type[] panelTypes = panelCore.Keys.ToArray();
            foreach (var type in panelTypes)
                UIKit.UnLoadPanel(type);
            panelCore.Clear();
            base.OnDestroy();           
        }
    }
}
