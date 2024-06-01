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

        private Dictionary<Type,IPanel> panelCore = new Dictionary<Type,IPanel>();

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

            MonoHelper.Destroy_AddListener(I.Release);
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
            EventSystem = eventSystem.SetParent(Canvas.transform);

            Object.DontDestroyOnLoad(Canvas.gameObject);

            LogKit.I("UIKit Initialization Succeeded!");           

            UpdateScreenAspect();
            ScreenTool.OnScreenChanged.RegisterEvent(UpdateScreenAspect);
        }
        int currentWidth;
        int currentHeight;
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
        public void InitLevel()
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
        }

        public Transform GetPanelLevel(UILevel level)
        {
            levelDicts.TryGetValue(level, out var transform);
            return transform;
        }

        public void AddPanelCore(IPanel panel)
        {
            panelCore[panel.GetType()] = panel;         
        }
      
        public void SetPanelFieldAndProperty(BasePanel panel)
        {
            Type type = panel.GetType();
            (panel as IYMonoBehaviour).InitAllFields();
        }

        [Obsolete]
        private void SetPanelFieldsAndProperties(Type type,BasePanel panel)
        {
            foreach (var members in type.GetMembers(BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.SetProperty))
            {
                UIAutoMationAttribute attribute = members.GetCustomAttribute<UIAutoMationAttribute>();

                if (attribute == null) continue;

                string name = attribute.Name;

                if (members is FieldInfo field)               
                    field.SetValue(panel, string.IsNullOrEmpty(name) ? panel.GetComponent(field.FieldType) : FindPanelTransform(panel, name).GetComponent(field.FieldType));                               
                else if(members is PropertyInfo property)
                    property.SetValue(panel, string.IsNullOrEmpty(name) ? panel.GetComponent(property.PropertyType) : FindPanelTransform(panel, name).GetComponent(property.PropertyType));
            }
        }
       
        private Transform FindPanelTransform(BasePanel panel,string name)
        {
            Transform[] transform = panel.GetComponentsInChildren<Transform>();
            for (int i = 0; i < transform.Length; i++)
            {
                if (transform[i].name.Equals(name))
                    return transform[i];
            }          
            return null;
        }

        public T GetPanelCore<T>() where T : class, IPanel
        {
            panelCore.TryGetValue(typeof(T),out var value);
            return (T)value;
        }

        public IPanel GetPanelCore(string name)
            => panelCore.FirstOrDefault(x => x.Value.gameObject.name == name).Value;

        public void Release(MonoHelper monoHelper = null)
        {
            levelDicts.Clear();
            reloadLevelSafe = false;
            reloadSystemSafe = false;
            IsLevelInited = false;
            if (UIKit.Default)
            {
                foreach (var obj in panelCore)
                {
                    AssetBundleManager.UnloadAsset(obj);
                }
            }
            panelCore.Clear();
            ScreenTool.OnScreenChanged.UnRegister(UpdateScreenAspect);
            UIKit.Release();
            MonoHelper.Destroy_RemoveListener(Release);
        }
    }
}
