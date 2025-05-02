///=====================================================
/// - FileName:      UIKit.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   UI管理套件
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace YukiFrameWork.UI
{
    public static class UIKit
    {
        private static IUIConfigLoader loader = null;

        public static UITable Table { get; private set; }    
          

        public static UIPrefabExector Exector => UIManager.Instance.Exector;
        /// <summary>
        /// UI模块初始化方法,模块使用框架资源管理插件ABManager加载
        /// 注意：使用ABManager加载模块,必须前置准备好资源模块的初始化以及准备,否则无法使用。
        /// </summary>
        /// <param name="projectName"></param>      
        public static void Init(string projectName)
        {
            Init(new ABManagerUILoader(projectName));
        }

        /// <summary>
        /// 打开临时面板，此处打开的面板必须是位于UIRoot下PrefabRoot层级下面的面板，而非动态加载面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ShowPanel<T>(params object[] param) where T : BasePanel, IPanel
            => Exector.Show_Internal<T>(param);

        /// <summary>
        /// 关闭临时面板,此处关闭的面板必须是位于UIRoot下PrefabRoot层级下面的面板，而非动态加载面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T HidePanel<T>() where T : BasePanel, IPanel
            => Exector.Hide_Internal<T>();

        public static RectTransform GetPrefabRootTransform()
            => Exector.transform as RectTransform;

        /// <summary>
        /// UI模块初始化方法,传入自定义的UI加载模块
        /// </summary>
        /// <param name="loader">自定义加载器</param>         
        public static void Init(IUIConfigLoader loader)
        {
            Table = new UITable();
            UIKit.loader = loader;           
            
            UIManager.I.InitLevel();
        }   

        /// <summary>
        /// 设置画布的分辨率
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SetCanvasReferenceResolution(float x, float y)
        {           
            if (!UIManager.Instance.Canvas.TryGetComponent<CanvasScaler>(out var canvasScaler)) return;
            canvasScaler.referenceResolution = new Vector2(x, y);
        }

        /// <summary>
        /// 设置画布的分辨率
        /// </summary>      
        public static void SetCanvasReferenceResolution(Vector2 resolution)
        {
            if (!UIManager.Instance.Canvas.TryGetComponent<CanvasScaler>(out var canvasScaler)) return;
            canvasScaler.referenceResolution = resolution;
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="name">路径/名称</param>
        /// <returns></returns>
        public static T OpenPanel<T>(string name = "", params object[] param) where T : BasePanel
        {
            name = name.IsNullOrEmpty() ? typeof(T).Name : name;
            IPanel panelCore = GetPanelCore<T>(name);         
            return OpenPanelExecute<T>(name, panelCore.Level, panelCore as T,param);
        }

        /// <summary>
        /// 打开面板并设置相应的层级
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="name">路径/名称</param>
        /// <param name="level">UI层级</param>
        /// <returns></returns>
        public static T OpenPanel<T>(string name,UILevel level,params object[] param) where T : BasePanel
        {
            name = name.IsNullOrEmpty() ? typeof(T).Name : name;
            IPanel panelCore = GetPanelCore<T>(name);           
            return OpenPanelExecute<T>(name, level, panelCore as T,param);
        }   

        private static IPanel GetPanelCore<T>(string name) where T : BasePanel,IPanel
        {
            UIManager uiMgr = UIManager.I;
            IPanel panelCore = uiMgr.GetPanelCore<T>();

            if (panelCore == null)
            {
                panelCore = CreatePanelCore<T>(name);

                uiMgr.AddPanelCore(panelCore);
            }

            return panelCore;
        }

        /// <summary>
        /// 外部直接传入Prefab加载面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="panel"></param>      
        /// <returns></returns>
        public static T OpenPanel<T>(T panel, params object[] param) where T : BasePanel
        {            
            UIManager.Instance.AddPanelCore(panel);
            return OpenPanelExecute(panel.name,panel.Level,panel,param);
        }
       
        /// <summary>
        /// 外部直接传入Prefab加载面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="panel"></param>      
        /// <returns></returns>
        public static T OpenPanel<T>(T panel,UILevel level, params object[] param) where T : BasePanel
        {
            UIManager.Instance.AddPanelCore(panel);
            return OpenPanelExecute(panel.name, level, panel, param);
        }

        private static T CreatePanelCore<T>(string name) where T :BasePanel, IPanel
        {
            if (loader == null)
            {
                string.Format("没有正确加载出面板的GameObject！如果是模块默认加载则请检查资源名称以及检查是否已经初始化UIKit! name:{0}", name).LogError();
                return null;
            }
            var panelObj = loader.Load<T>(name);                
            if (panelObj == null)
            {
                string.Format("没有正确加载出面板的Panel组件！请检查是否在该面板(GameObject)挂载了需要的脚本！name:{0}", name).LogError();
                return null;
            }

            return panelObj;
        }     

        /// <summary>
        /// 异步打开面板，需要传递一个回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="onCompleted"></param>
        public static void OpenPanelAsync<T>(string name,Action<T> onCompleted, params object[] param) where T : BasePanel
        {
            name = name.IsNullOrEmpty() ? typeof(T).Name : name;
            UIManager uiMgr = UIManager.I;
            var panelCore = uiMgr.GetPanelCore<T>();

            if (panelCore == null)
            {
                loader.LoadAsync<T>(name, panel => 
                {
                    uiMgr.AddPanelCore(panel);
                    onCompleted?.Invoke(OpenPanelExecute(name, panel.Level, panel, param));
                });
                return;
            }          
            onCompleted?.Invoke(OpenPanelExecute(name, panelCore.Level, panelCore,param));
        }

        /// <summary>
        /// 有返回值的异步打开面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static UILoadAssetRequest OpenPanelAsync<T>(string name = "", params object[] param) where T : BasePanel
        {
            name = name.IsNullOrEmpty() ? typeof(T).Name : name;
            UIManager uiMgr = UIManager.I;
            var panelCore = uiMgr.GetPanelCore<T>();
            UILoadAssetRequest loadAssetRequest = new UILoadAssetRequest();
            if (panelCore == null)
            {
                loader.LoadAsync<T>(name, panel =>
                {
                    uiMgr.AddPanelCore(panel);
                    loadAssetRequest.LoadPanelAsync(OpenPanelExecute(name,panel.Level, panel,param));
                });
                return loadAssetRequest;
            }         
            return loadAssetRequest.LoadPanelAsync(OpenPanelExecute(name,panelCore.Level, panelCore, param));
        }

        public static void OpenPanelAsync<T>(string name,UILevel level, Action<T> onCompleted, params object[] param) where T : BasePanel
        {
            UIManager uiMgr = UIManager.I;
            var panelCore = uiMgr.GetPanelCore<T>();

            if (panelCore == null)
            {
                loader.LoadAsync<T>(name, panel =>
                {                   
                    uiMgr.AddPanelCore(panel);
                    onCompleted?.Invoke(OpenPanelExecute(name,level, panel, param));
                });
                return;
            }         
            onCompleted?.Invoke(OpenPanelExecute(name,level, panelCore,param));
        }

        public static UILoadAssetRequest OpenPanelAsync<T>(string name,UILevel level, params object[] param) where T : BasePanel
        {
            UIManager uiMgr = UIManager.I;
            var panelCore = uiMgr.GetPanelCore<T>();
            UILoadAssetRequest loadAssetRequest = new UILoadAssetRequest();
            if (panelCore == null)
            {
                loader.LoadAsync<T>(name, panel =>
                {
                  
                    uiMgr.AddPanelCore(panel);
                    loadAssetRequest.LoadPanelAsync(OpenPanelExecute(name,level, panel, param));
                });
                return loadAssetRequest;
            }         
            return loadAssetRequest.LoadPanelAsync(OpenPanelExecute(name,level, panelCore,param)); 
        }

        internal static T OpenPanelExecute<T>(string name,UILevel level,T panelCore,params object[] param) where T : BasePanel
        {
            UIManager uiMgr = UIManager.I;
            var panel = Table.GetActivityPanel(name,panelCore.Level,panelCore.OpenType);           
            if (panel == null)
            {
                if (panelCore.OpenType != PanelOpenType.Multiple)
                {
                    panel = Table.GetPanelByLevel(name);
                    if (panel != null)
                    {
                        UIKit.ClosePanel(panel);
                    }
                }
                if (panel == null)
                {
                    panel = Object.Instantiate(panelCore, uiMgr.GetPanelLevel(level), false);                  
                    panel.OnPreInit(param);
                    panel.OnInit();
                }

                if (level != panel.Level)
                {
                    panel.SetLevel(level);
                    Table.ChangeLevelByActivityRemove(panel);
                    panel.gameObject.SetParent(uiMgr.GetPanelLevel(level));
                }
                panel.gameObject.name = name;                
                if(panel.IsPanelCache)
                    Table.AddActivityPanel(panel);
            }

            if (panel != null && panel.Level != level)
            {
                panel.SetLevel(level);
                Table.ChangeLevelByActivityRemove(panel);
                if(panel.IsPanelCache)
                    Table.AddActivityPanel(panel);
            }

            if (!panel.IsActive)
            {
                PanelInfo info = new PanelInfo();
                info.panel = panel;
                info.panelName = panel.gameObject.name;
                info.level = panel.Level;
                info.panelType = typeof(T);
                info.param = param;
                Table.PushPanel(info);
            }
            return (T)panel;
        }

        /// <summary>
        /// 根据加载后的面板/路径名称关闭面板(name与使用OpenPanel时一致),如果面板类型为Multiple且场景中打开了多个，会一次性全部关闭
        /// </summary>
        /// <param name="name"></param>
        public static void ClosePanel(string name)
        {
            var core = UIManager.Instance.GetPanelCore(name);        
            if (core == null) return;

            PanelInfo info = new PanelInfo();

            info.panelName = name;
            info.level = core.Level;
            info.panelType = core.GetType();
            Table.PopPanel(info);
        }

        /// <summary>
        /// 关闭相应类型的面板,如果面板类型为Multiple且场景中打开了多个，会一次性全部关闭
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ClosePanel<T>() where T : class, IPanel
        {
            var core = UIManager.Instance.GetPanelCore<T>();          
            if (core == null) return;

            PanelInfo info = new PanelInfo();
            info.panelName = core.gameObject.name;
            info.level = core.Level;
            info.panelType = core.GetType();
            Table.PopPanel(info);
        }

        /// <summary>
        /// 指定关闭的面板,适合在面板类型为Multiple时使用，这样不会出现一次性全部关闭的情况
        /// </summary>
        /// <param name="panel"></param>
        public static void ClosePanel(IPanel panel)
        {
            if (panel == null) return;
            PanelInfo info = new PanelInfo();
            
            info.panelName = panel.gameObject.name;
            info.level = panel.Level;
            info.panelType = panel.GetType();
            info.panel = panel;
            Table.PopPanel(info);         
        }

        /// <summary>
        /// 关闭指定层级下所有已打开的面板
        /// </summary>
        /// <param name="level"></param>
        public static void ClosePanelByLevel(UILevel level)
        {
            PanelInfo info = new PanelInfo()
            {
                level = level,
                levelClear = true
            };

            Table.PopPanel(info);
        }


        /// <summary>
        /// 通过层级获取已经加载的缓存面板(返回第一个找到的面板) 
        /// <para>注意:无法通过该API查找到不缓存的面板</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <returns></returns>
        public static T GetPanel<T>(UILevel level) where T : BasePanel
        {
            return GetPanelInternal(typeof(T), level) as T;
        }     

        internal static BasePanel GetPanelInternal(Type type, UILevel level)
        {
            return Table.GetActivityPanel(type, level,PanelOpenType.Single) as BasePanel;
        }

        /// <summary>
        /// 卸载指定类型面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void UnLoadPanel<T>() where T : BasePanel
        {
            UnLoadPanel(typeof(T));
        }
        /// <summary>
        /// 卸载/释放指定类型的面板资源。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void UnLoadPanel(Type type)
        {
            if (!type.IsSubclassOf(typeof(BasePanel)))
                throw new Exception("类型不是面板无法释放");
#if UNITY_2020_1_OR_NEWER
            if (UIManager.Instance.panelCore.TryGetValue(type, out var panel))
            {
                //遍历全层级移除已打开的面板
                for (UILevel level = UILevel.BG; level <= UILevel.Top; level++)
                {
                    IPanel[] activePanel = Table.GetActivityPanels(type, level);
                    if (activePanel != null && activePanel.Length != 0)
                    {
                        foreach (var item in activePanel)
                        {
                            if (item as BasePanel && item.gameObject)
                            {
                                UIKit.ClosePanel(item);
                                item.gameObject.Destroy();
                            }
                        }
                    }
                }
                //卸载缓存。
                UIManager.Instance.panelCore.Remove(type);
                loader.UnLoad(panel as BasePanel);

            }
#endif
        }


        /// <summary>
        /// 通过层级获取已经加载的该类型所有缓存面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <returns></returns>
        public static T[] GetPanels<T>(UILevel level) where T : BasePanel
        {
            IPanel[] panels = GetPanelInternals(typeof(T), level);
            if (panels == null) return null;
            T[] t = new T[panels.Length];
            for (int i = 0; i < t.Length; i++)
            {
                t[i] = panels[i] as T;
            }
            return t;
        }

        internal static IPanel[] GetPanelInternals(Type type, UILevel level)
        {
            return Table.GetActivityPanels(type, level);
        }

        /// <summary>
        /// 强行获取面板(面板如果在场景中不存在则会创建一个,创建的面板如果是缓存面板会先进行关闭)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetPanel<T>(string name = "") where T : BasePanel
        {
            name = name.IsNullOrEmpty() ? typeof(T).Name : name;
            T panel;         
            for (UILevel level = UILevel.BG; level <= UILevel.Top; level++)
            {               
                panel = GetPanel<T>(level);

                if (panel != null)
                    return panel;
            }           
            panel = OpenPanel<T>(name);
            ///如果面板是缓存面板
            if (panel.IsPanelCache)            
                ClosePanel(panel);      
            return panel;          
        }

        /// <summary>
        /// 判断面板是否已经激活并且开启(仅限缓存面板)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">层级</param>
        /// <param name="openType">打开类型</param>
        /// <returns></returns>
        public static bool IsPanelActive<T>(UILevel level,PanelOpenType openType = PanelOpenType.Single) where T : BasePanel
        {
            if (openType == PanelOpenType.Single)
            {
                var panel = GetPanel<T>(level);

                if (panel == null) return false;

                return panel.IsActive;
            }
            else if (openType == PanelOpenType.Multiple)
            {
                var panels = GetPanels<T>(level);
                if (panels == null) return false;

                for (int i = 0; i < panels.Length; i++)
                {
                    if (!panels[i].IsActive)
                        return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 全层级判断面板是否激活并且开启(仅限缓存面板)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="openType"></param>
        /// <returns></returns>
        public static bool IsPanelActive<T>(PanelOpenType openType = PanelOpenType.Single) where T : BasePanel
        {
            for (UILevel level = UILevel.BG; level <= UILevel.Top; level++)
            {
                if (IsPanelActive<T>(level, openType))
                    return true;
            }
            return false;
        }
      
        public static void Release()
        {
            UIManager.Instance.Dispose();
            Table.Dispose();                      
        }

        /// <summary>
        /// 通过层级查找场景中存在的面板实例，特供于寻找对象本身
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <returns></returns>
        public static T FindPanelByType<T>(UILevel level) where T : BasePanel
        {
            var root = UIManager.Instance.GetPanelLevel(level);
            if (!root) return null;
            return root.GetComponentInChildren<T>(true);
        }


        /// <summary>
        /// 检索所有的层级，获得第一个找到的指定的面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindPanelByType<T>() where T : BasePanel
        {
            for (UILevel level = UILevel.BG; level <= UILevel.Top; level++)
            {
                T panel = FindPanelByType<T>(level);

                if (panel)
                    return panel;
            }

            return null;
        }

        /// <summary>
        /// 通过层级查找场景中存在的面板实例集合，特供于寻找对象本身
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <returns></returns>
        public static T[] FindPanelsByType<T>(UILevel level) where T : BasePanel
        {
            return UIManager.Instance.GetPanelLevel(level).GetComponentsInChildren<T>();
        }       

        public static RectTransform GetPanelLevel(UILevel level)
            => UIManager.Instance.GetPanelLevel(level);
    }

    public static class UIGenericExtension
    {
        public static void SetRectTransformInfo(this RectTransform transform, Canvas canvas)
        {
            transform.SetParent(canvas.transform);
            transform.localPosition = Vector3.zero;
            transform.anchorMax = new Vector2(1, 1);
            transform.anchorMin = Vector2.zero;
            transform.localScale = Vector3.one;
            transform.offsetMax = Vector2.zero;
            transform.offsetMin = Vector2.zero;
        }

        public static IPanel OnPanelExitingListener(this IPanel panel, Action callBack)
        {
            MonoHelper.Start(Async());
            IEnumerator Async()
            {
                yield return CoroutineTool.WaitUntil(() => !panel.IsActive);
                callBack?.Invoke();
            }
            return panel;
        }

        public static IPanel OnPanelPausingListener(this IPanel panel, Action callBack)
        {
            MonoHelper.Start(Async());
            IEnumerator Async()
            {
                yield return CoroutineTool.WaitUntil(() => panel.IsPaused);
                callBack?.Invoke();
            }
            return panel;
        }

        public static T ShowPanel<T>(this T temp) where T : BasePanel, IPanel
            => UIKit.ShowPanel<T>();

        public static T HidePanel<T>(this T temp) where T : BasePanel, IPanel
            => UIKit.HidePanel<T>();

        public static void UnLoadPanel<T>(this T panel) where T : BasePanel,IPanel        
            => UIKit.UnLoadPanel<T>();
        
    }
}