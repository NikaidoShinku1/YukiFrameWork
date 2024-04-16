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
        public Canvas Canvas { get; private set; }

        private Dictionary<UILevel, RectTransform> levelDicts = DictionaryPools<UILevel, RectTransform>.Get();

        private List<BasePanel> panelCore = new List<BasePanel>();     

        public override void OnInit()
        {
            Canvas = Object.FindObjectOfType<Canvas>();
            if(Canvas == null || !Canvas.name.Equals("UIRoot"))
                Canvas = Resources.Load<Canvas>("UIRoot").Instantiate();          
          
            if (Canvas == null)
            {
                throw new Exception("UIRoot丢失，请重新导入UI模块!");
            }

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
        }

        public void InitLevel()
        {
            for (int i = 0; i < (int)UILevel.Top; i++)
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
