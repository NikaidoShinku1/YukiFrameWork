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
using YukiFrameWork.XFABManager;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
namespace YukiFrameWork.UI
{
    public class UIKit
    {
        private static IUIConfigLoader loader = null;
        //层级堆栈
        private readonly static Dictionary<UILevel, Stack<BasePanel>> uiLevelPanelDicts = DictionaryPools<UILevel, Stack<BasePanel>>.Get();

        //储存已经被创造出来的panel
        private readonly static Dictionary<UILevel,List<BasePanel>> creativityPanels = DictionaryPools<UILevel,List<BasePanel>>.Get();    

        //检查是否完成初始化
        private static bool isInit = false;

        public static bool Default { get; private set; } = false;

        public static string CanvasName { get; private set; }
        /// <summary>
        /// UI模块初始化方法,模块使用框架资源管理插件ABManager加载
        /// 注意：使用ABManager加载模块,必须前置准备好资源模块的初始化以及准备,否则无法使用。
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="canvasNameOrPath">画布所在场景的默认Canvas的名称</param>
        public static bool Init(string projectName,string canvasName= "Canvas")
        {
            if (isInit)
            {
                CheckCanvasAndEventSystem();
                "UI模块已经完成初始化，无需再次调用!".LogInfo();
                return false;
            }
            CanvasName = canvasName;
            InitUILevel();
            loader = new ABManagerUILoader(projectName);
            Default = true;
            isInit = true;
            return isInit;
        }

        /// <summary>
        /// UI模块初始化方法,传入自定义的UI加载模块,设定画布如果有自设名称则需要修改CanvasPath
        /// </summary>
        /// <param name="loader">自定义加载器</param>
        /// <param name="canvasPath">画布所在场景的默认Canvas的名称</param>        
        public static bool Init(IUIConfigLoader loader,string canvasName = "Canvas")
        {
            if (isInit)
            {
                CheckCanvasAndEventSystem();
                "UI模块已经完成初始化，无需再次调用!".LogInfo();
                return false;
            }
            CanvasName = canvasName;
            InitUILevel();
            UIKit.loader = loader;
            isInit = true;
            Default = false;
            return isInit;
        }

        /// <summary>
        /// 检查场景是否多出了一样的画布以及EventSystem是否唯一
        /// </summary>
        private static void CheckCanvasAndEventSystem()
        {         
            foreach (var canvas in Object.FindObjectsOfType<Canvas>())
            {                             
                if (!canvas.Equals(UIManager.I.Canvas.Value) && canvas.gameObject.name.Equals(CanvasName))
                {
                    Object.Destroy(canvas.gameObject);
                }
            }

            foreach (var eventSystem in Object.FindObjectsOfType<EventSystem>())
            {
                if (!eventSystem.Equals(UIManager.I.DefaultEventSystem))
                {
                    Object.Destroy(eventSystem.gameObject);
                }
            }
        }

        private static void InitUILevel()
        {
            for (int i = 0; i < (int)UILevel.Top; i++)
            {
                UILevel level = (UILevel)Enum.GetValues(typeof(UILevel)).GetValue(i);
                uiLevelPanelDicts.Add(level, new Stack<BasePanel>());
            }

            for (int i = 0; i < (int)UILevel.Top; i++)
            {
                UILevel level = (UILevel)Enum.GetValues(typeof(UILevel)).GetValue(i);
                creativityPanels.Add(level, new List<BasePanel>());
            }

            UIManager.I.InitLevel();
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

        private static T CreatePanelCore<T>(string name) where T : BasePanel
        {
            if (loader == null)
            {
                string.Format("没有正确加载出面板的GameObject！如果是模块默认加载则请检查资源名称以及检查是否已经初始化UIKit! name:{0}", name).LogInfo(Log.E);
                return null;
            }
            var panelObj = loader.Load<T>(name);
          
            var panelCore = panelObj.GetComponent<T>();

            if (panelCore == null)
            {
                string.Format("没有正确加载出面板的Panel组件！请检查是否在该面板(GameObject)挂载了需要的脚本！name:{0}",name).LogInfo(Log.E);
                return null;
            }

            return panelCore;
        }     

        public static void OpenPanelAsync<T>(string name,Action<T> onCompleted) where T : BasePanel
        {
            UIManager uiMgr = UIManager.I;
            var panelCore = uiMgr.GetPanelCore<T>();

            if (panelCore == null)
            {
                loader.LoadAsync<T>(name, panel => onCompleted?.Invoke(OpenPanelExecute(panel)));
                return;
            }
            onCompleted?.Invoke(OpenPanelExecute(panelCore));

        }

        private static T OpenPanelExecute<T>(T panelCore) where T : BasePanel
        {
            UIManager uiMgr = UIManager.I;
            var panel = GetPanel<T>(panelCore.Level);
            if (panel == null)
            {
                panel = Object.Instantiate(panelCore, uiMgr.GetPanelLevel(panelCore.Level), false);
                uiMgr.SetPanelFieldAndProperty(panel);
                panel.OnInit();
                creativityPanels[panel.Level].Add(panel);
            }
            AddStackPanel(panel, panel.Level);
            return panel;
        }

        public static void ClosePanel(UILevel level = UILevel.Common)
        {
            if (uiLevelPanelDicts.TryGetValue(level, out var stack))
            {
                if (stack.Count > 0)
                    stack.Pop()?.OnExit();
                if (stack.Count > 0)
                    stack.Peek()?.OnResume();
            }
        }     
        public static T GetPanel<T>(UILevel level) where T : BasePanel
        {
            creativityPanels.TryGetValue(level, out var list);
            return list.Find(x => x.GetType().Equals(typeof(T))) as T;
        }

        /// <summary>
        /// 强行获取面板(面板如果在场景中不存在则会创建一个,并且会处于关闭状态)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T GetPanel<T>(string path) where T : BasePanel
        {
            T panel = null;
            for (int i = 0; i < (int)UILevel.System; i++)
            {
                UILevel level = (UILevel)Enum.GetValues(typeof(UILevel)).GetValue(i);
                panel = GetPanel<T>(level);

                if (panel != null)
                    return panel;
            }           
            panel = OpenPanel<T>(path);
            ClosePanel(panel.Level);
            return panel;          
        }

        private static void AddStackPanel<T>(T panel, UILevel level) where T : BasePanel
        {
            uiLevelPanelDicts.TryGetValue(level, out var list);

            if (list.Count > 0)
                list.Peek()?.OnPause();
            panel.OnEnter();
            list.Push(panel);         
        }      

        public static void Release()
        {        
            uiLevelPanelDicts.Clear();
            foreach (var core in creativityPanels.Values)
            {
                foreach (var c in core)
                {
                    if (Default)
                        AssetBundleManager.UnloadAsset(c.gameObject);
                    else
                        Object.Destroy(c.gameObject);
                }

                core.Clear();
            }
            creativityPanels.Clear();
            isInit = false;
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