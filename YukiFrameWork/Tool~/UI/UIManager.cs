using UnityEngine;
using System;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using YukiFrameWork.Pools;
using UnityEngine.UI;
using System.Reflection;
using XFABManager;

namespace YukiFrameWork.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private UIManager() { }
        public BindableProperty<Canvas> Canvas { get; private set; }

        public EventSystem DefaultEventSystem { get; private set; }

        private Dictionary<UILevel, RectTransform> levelDicts = DictionaryPools<UILevel, RectTransform>.Get();

        private List<BasePanel> panelCore = new List<BasePanel>();     

        public override void OnInit()
        {
            Canvas = new BindableProperty<Canvas>(GameObject.Find(UIKit.CanvasName).GetComponent<Canvas>());          
          
            if (Canvas.Value == null)
            {
                throw new Exception("请在场景中添加画布！初始化失败:Not Canvas In Scene!");
            }

            MonoHelper.Destroy_AddListener(I.Release);          
            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject(typeof(EventSystem).Name).AddComponent<EventSystem>();
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }

            DefaultEventSystem = eventSystem;
                      
            "UIKit Initialization Succeeded!".LogInfo(_ => 
            {
                eventSystem.transform.SetParent(Canvas.Value.transform);
                Object.DontDestroyOnLoad(Canvas.Value.gameObject);
            });          
        }

        public void InitLevel()
        {
            for (int i = 0; i < (int)UILevel.Top; i++)
            {
                UILevel level = (UILevel)Enum.GetValues(typeof(UILevel)).GetValue(i);

                RectTransform transform = new GameObject(level.ToString()).AddComponent<RectTransform>();
                transform.SetRectTransformInfo(Canvas.Value);
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

        public void AddPanelCore(BasePanel panel)
        {
            panelCore.Add(panel);           
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

        public T GetPanelCore<T>() where T : BasePanel
        {
            return panelCore.Find(x => x.GetType().Equals(typeof(T))) as T;
        }

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
            UIKit.Release();
            MonoHelper.Destroy_RemoveListener(Release);
        }
    }
}
