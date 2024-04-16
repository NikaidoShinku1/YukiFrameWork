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
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Pools;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
namespace YukiFrameWork.UI
{
    public class UIKit
    {
        private static IUIConfigLoader loader = null;

        //层级堆栈
        private readonly static Dictionary<UILevel, Stack<BasePanel>> uiLevelPanelDicts = DictionaryPools<UILevel, Stack<BasePanel>>.Get();

        //储存已经被创造出来的panel
        private readonly static Dictionary<UILevel,FastList<PanelInfo>> creativityPanels = DictionaryPools<UILevel,FastList<PanelInfo>>.Get();

        //缓存一次性面板
        private readonly static List<BasePanel> disposables = new List<BasePanel>();

        //需要被销毁的面板
        private readonly static List<PanelInfo> realeasePanels = new List<PanelInfo>();

        const int UNLOAD_CACHESECOUND = 5 * 60;

        const int DETECTION_INTERVAL = 60;
        //检查是否完成初始化
        private static bool isInit = false;

        private static bool isLevelInited = false;

        public static bool Default { get; private set; } = false;      

        class PanelInfo
        {
            public float cacheLoadTime;
            public BasePanel panel;
            public Type panelType;
        }
        /// <summary>
        /// UI模块初始化方法,模块使用框架资源管理插件ABManager加载
        /// 注意：使用ABManager加载模块,必须前置准备好资源模块的初始化以及准备,否则无法使用。
        /// </summary>
        /// <param name="projectName"></param>      
        public static bool Init(string projectName)
        {
            if (isInit)
            {              
                "UI模块已经完成初始化，无需再次调用!".LogInfo();
                return false;
            }                     
            loader = new ABManagerUILoader(projectName);
            Default = true;
            isInit = true;
            return isInit;
        }

        /// <summary>
        /// UI模块初始化方法,传入自定义的UI加载模块
        /// </summary>
        /// <param name="loader">自定义加载器</param>         
        public static bool Init(IUIConfigLoader loader)
        {
            if (isInit)
            {              
                "UI模块已经完成初始化，无需再次调用!".LogInfo();
                return false;
            }                   
            UIKit.loader = loader;
            isInit = true;
            Default = false;
            return isInit;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitUILevel()
        {
            if (isLevelInited) return;
            for (int i = 0; i < (int)UILevel.Top; i++)
            {
                UILevel level = (UILevel)Enum.GetValues(typeof(UILevel)).GetValue(i);
                uiLevelPanelDicts.Add(level, new Stack<BasePanel>());
            }

            for (int i = 0; i < (int)UILevel.Top; i++)
            {
                UILevel level = (UILevel)Enum.GetValues(typeof(UILevel)).GetValue(i);
                creativityPanels.Add(level, new FastList<PanelInfo>());
            }
            StartPanelCheck().Start().CancelWaitGameObjectDestroy(MonoHelper.I);
            UIManager.I.InitLevel();
            isLevelInited = true;
        }

        /// <summary>
        /// 开始检查缓存面板是否需要释放
        /// </summary>
        /// <returns></returns>
        private static IEnumerator StartPanelCheck()
        {
            while (true)
            {
                foreach (var key in creativityPanels.Keys)
                {
                    var value = creativityPanels[key];
                    for (int i = 0; i < value.Count; i++)
                    {
                        var info = value[i];

                        if (Time.time - info.cacheLoadTime >= UNLOAD_CACHESECOUND && !IsPanelActiveInternal(info.panelType,key))
                        {
                            realeasePanels.Add(info);
                        }
                    }
                }

                for(int i = 0;i < realeasePanels.Count;i++)
                {
                    creativityPanels[realeasePanels[i].panel.Level].Remove(realeasePanels[i]);
                    realeasePanels[i].panel.gameObject.Destroy();
                }
                realeasePanels.Clear();
                yield return CoroutineTool.WaitForSeconds(DETECTION_INTERVAL);
            }
        }
          
        public static T OpenPanel<T>(string name) where T : BasePanel
        {                     
            UIManager uiMgr = UIManager.I;
            var panelCore = uiMgr.GetPanelCore<T>();
           
            if (panelCore == null)
            {             
                panelCore = CreatePanelCore<T>(name);
             
                uiMgr.AddPanelCore(panelCore);               
            }

            return OpenPanelExecute(panelCore);
        }

        /// <summary>
        /// 外部直接传入Prefab加载面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="panel"></param>      
        /// <returns></returns>
        public static T OpenPanel<T>(T panel) where T : BasePanel
        {          
            return OpenPanelExecute(panel);
        }

        private static T CreatePanelCore<T>(string name) where T : BasePanel
        {
            if (loader == null)
            {
                string.Format("没有正确加载出面板的GameObject！如果是模块默认加载则请检查资源名称以及检查是否已经初始化UIKit! name:{0}", name).LogInfo(Log.E);
                return null;
            }
            var panelObj = loader.Load<T>(name);                
            if (panelObj == null)
            {
                string.Format("没有正确加载出面板的Panel组件！请检查是否在该面板(GameObject)挂载了需要的脚本！name:{0}",name).LogInfo(Log.E);
                return null;
            }

            return panelObj;
        }     

        public static void OpenPanelAsync<T>(string name,Action<T> onCompleted) where T : BasePanel
        {
            UIManager uiMgr = UIManager.I;
            var panelCore = uiMgr.GetPanelCore<T>();

            if (panelCore == null)
            {
                loader.LoadAsync<T>(name, panel => 
                {
                    uiMgr.AddPanelCore(panel);
                    onCompleted?.Invoke(OpenPanelExecute(panel));
                });
                return;
            }
            onCompleted?.Invoke(OpenPanelExecute(panelCore));

        }

        private static T OpenPanelExecute<T>(T panelCore) where T : BasePanel
        {
            UIManager uiMgr = UIManager.I;
            var panel = disposables.Find(x => x.GetType().Equals(typeof(T))) as T;

            if (panel != null) 
                return panel;

            panel = GetPanel<T>(panelCore.Level);

            if (panel == null)
            {
                panel = Object.Instantiate(panelCore, uiMgr.GetPanelLevel(panelCore.Level), false);
                uiMgr.SetPanelFieldAndProperty(panel);
                panel.OnInit();

                if (panel.IsPanelCache)
                    creativityPanels[panel.Level].Add(new PanelInfo()
                    {
                        panel = panel,
                        panelType = typeof(T),
                        cacheLoadTime = Time.time
                    });
                else disposables.Add(panel);
            }


            if (!IsPanelActive<T>(panel.Level))
            {
                panel.transform.SetAsLastSibling();
                AddStackPanel(panel, panel.Level);
            }
            return panel;
        }

        public static void ClosePanel(string name,UILevel level = UILevel.Common)
        {
            if (uiLevelPanelDicts.TryGetValue(level, out var stack))
            {
                ///如果层级内存在已经打开的面板
                if (stack.Count > 0)
                {
                    ///先得到置顶的面板
                    BasePanel panel = stack.Peek();
                    IPanel pop = null;
                    //面板名字如果跟传进来的匹配
                    if (panel.name.Equals(name))
                    {
                        //弹出顶层
                        pop = stack.Pop();
                        pop.Exit();

                        DisposableInternal(pop as BasePanel);
                        if (stack.Count > 0)
                        {
                            IPanel peek = stack.Peek();
                            peek.Resume();
                        }
                    }
                    else
                    {
                        //否则的话查找层级内有的面板退出
                        pop = stack.FirstOrDefault(x => x.name.Equals(name));
                        if (pop == null) return;
                        pop.Exit();
                        DisposableInternal(pop as BasePanel);
                    }
                }               
            }
        }

        private static void DisposableInternal(BasePanel panel)
        {
            if (!panel.IsPanelCache)
            {
                disposables.Remove(panel);
                Object.Destroy(panel.gameObject);
            }
        }

        public static void ClosePanel<T>(UILevel level = UILevel.Common) where T : BasePanel 
        {
            ClosePanel(typeof(T).Name, level);
        }

        public static void ClosePanel(BasePanel panel)
            => ClosePanel(panel.name,panel.Level);

        /// <summary>
        /// 通过层级获取已经加载出来的面板(优先返回缓存面板)
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
            BasePanel panel = null;          
            creativityPanels.TryGetValue(level, out var list);
            var info = list.Find(x => x.panelType.Equals(type));
            if (info == null) 
            {
                panel = disposables.Find(x => x.GetType().Equals(type) && x.Level.Equals(level));
                return panel;              
            }
            panel = info.panel;
            info.cacheLoadTime = Time.time;
            return panel;
        }

        /// <summary>
        /// 强行获取面板(面板如果在场景中不存在则会创建一个,如果是缓存面板会先进行关闭)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T GetPanel<T>(string path) where T : BasePanel
        {
            T panel;         
            for (int i = 0; i < (int)UILevel.System; i++)
            {
                UILevel level = (UILevel)Enum.GetValues(typeof(UILevel)).GetValue(i);
                panel = GetPanel<T>(level);

                if (panel != null)
                    return panel;
            }           
            panel = OpenPanel<T>(path);
            ///如果面板是缓存面板
            if (panel.IsPanelCache)            
                ClosePanel(panel);      
            return panel;          
        }

        /// <summary>
        /// 判断面板是否处于激活状态(返回开启缓存的面板)
        /// </summary>
        /// <param name="level">这个面板所在的层级</param>
        /// <returns>如果面板从没被加载过或者是退出的状态则返回False,否则返回True</returns>
        public static bool IsPanelActive<T>(UILevel level) where T : BasePanel
        {
            return IsPanelActiveInternal(typeof(T), level);
        }

        internal static bool IsPanelActiveInternal(Type type, UILevel level)
        {
            IPanel panel = GetPanelInternal(type,level);

            if (panel == null) return false;

            return panel.IsActive;
        }

        /// <summary>
        /// 判断面板是否是暂停的状态(返回开启缓存的面板)
        /// </summary>     
        /// <param name="level">这面板所在的层级</param>
        /// <returns>如果面板不存在或者不是暂停的状态就返回False,否则返回True</returns>
        public static bool IsPanelPaused<T>(UILevel level) where T : BasePanel
        {
            return IsPanelActiveInternal(typeof(T), level);
        }

        internal static bool IsPanelInternal(Type type, UILevel level)
        {
            IPanel panel = GetPanelInternal(type,level);

            if (panel == null) return false;

            return panel.IsPaused;
        }

        private static void AddStackPanel<T>(T panel, UILevel level) where T : BasePanel
        {
            uiLevelPanelDicts.TryGetValue(level, out var list);

            if (list.Count > 0)
            {
                IPanel peek = list.Peek();
                peek.Pause();
            }
            (panel as IPanel).Enter();
            list.Push(panel);         
        }      

        public static void Release()
        {        
            uiLevelPanelDicts.Clear();           
            creativityPanels.Clear();
            disposables.Clear();
            realeasePanels.Clear();
            isInit = false;
            isLevelInited = false;
            Default = false;
        }
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
    }
}