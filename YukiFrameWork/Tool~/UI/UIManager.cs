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

        private Dictionary<UILevel, RectTransform> levelDicts = DictionaryPools<UILevel, RectTransform>.Get();

        private Dictionary<Type,IPanel> panelCore = new Dictionary<Type,IPanel>();     

        public override void OnInit()
        {
            Canvas = Object.FindObjectsOfType<Canvas>().FirstOrDefault(x => x.name.Equals("UIRoot"));
            if(Canvas == null)
                Canvas = Resources.Load<Canvas>("UIRoot").Instantiate();          
          
            if (Canvas == null)
            {
                throw new Exception("UIRoot丢失，请重新导入UI模块!");
            }

            CanvasScaler = Canvas.GetComponent<CanvasScaler>();
            GraphicRaycaster = Canvas.GetComponent<GraphicRaycaster>();

            MonoHelper.Destroy_AddListener(I.Release);
            var eventSystem = Object.FindObjectOfType<EventSystem>();

            if (eventSystem == null)
            {
                eventSystem = new GameObject(nameof(EventSystem)).AddComponent<EventSystem>();
                eventSystem.GetOrAddComponent<StandaloneInputModule>();
            }

            eventSystem.SetParent(Canvas.transform);
                      
            "UIKit Initialization Succeeded!".LogInfo(_ => 
            {              
                Object.DontDestroyOnLoad(Canvas.gameObject);
            });

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
        public void InitLevel()
        {
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

            SetPanelFieldsAndProperties(type, panel);
        }

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
