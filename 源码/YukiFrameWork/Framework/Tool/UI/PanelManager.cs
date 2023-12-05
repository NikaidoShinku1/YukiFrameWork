using System;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Pools;
using YukiFrameWork.Res;

namespace YukiFrameWork.UI
{
    public interface IPanelManager
    {
        /// <summary>
        /// 面板入栈
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="type">层级类型</param>
        /// <param name="parentPanel">是否拥有父级面板</param>
        /// <returns></returns>
        T PushPanel<T>(UIPanelType type) where T : BasePanel;
        TPanel PushChildPanel<TPanel>(Type type) where TPanel : BasePanel;
        void PopPanel(UIPanelType type, bool isDestroy = false);
        void PopChildPanel<TParent>(bool isDestroy = false);
      
    }
    public class PanelManager : IPanelManager
    {
        private static UIManager UIManager => UIManager.Instance;

        public PanelManager(string panelPath,LoadMode mode)
        {
            UIManager.Init(panelPath, mode);
        }
        private readonly static Dictionary<UIPanelType, Stack<BasePanel>> uiStacks = DictionaryPools<UIPanelType, Stack<BasePanel>>.Get();

        private readonly static Dictionary<Type, Stack<BasePanel>> childStacks = DictionaryPools<Type, Stack<BasePanel>>.Get();      

        public T PushPanel<T>(UIPanelType type) where T : BasePanel
        {
            return (T)OnPushPanel<T>(type);

        }

        public IAsyncExtensionCore InitAsync(Action onFinish)
            => UIManager.InitAsync().Start(onFinish);

        private BasePanel OnPushPanel<T>(UIPanelType type) where T : BasePanel
        {
            if (!uiStacks.TryGetValue(type, out var panelStack))
            {
                panelStack = new Stack<BasePanel>();
                uiStacks.Add(type, panelStack);
            }
            else
            {
                if (panelStack.Count > 0)
                {
                    panelStack.Peek().OnPause();
                }
            }

            var panel = UIManager.GetPanel(typeof(T));
            if (panel == null)
            {
                Debug.LogError("这个面板没有成功加载，请使用UIKit重新配置！");
                return null;
            }           
            panel.OnEnter();
            panelStack.Push(panel);          
            panel.transform.SetAsLastSibling();
            Debug.Log("面板入栈！该面板 " + panel + " 为层级面板," + "：" + type);
            return panel;
        }

        public void PopPanel(UIPanelType type, bool isDestroy = false)
        {
            OnPopPanel(type, isDestroy);
        }

        private void OnPopPanel(UIPanelType type,bool isDestroy = false)
        {          

            if (!uiStacks.TryGetValue(type, out var panelStack))
            {
                Debug.LogError("当前层级没有初始化该堆栈，请尝试重试");                
            }
            else
            {
               
                if (panelStack.Count > 0)
                {
                    var tempPanel = panelStack.Pop();
                    tempPanel.OnExit();
                    if (childStacks.TryGetValue(tempPanel.GetType(), out var child))
                    {
                        foreach (var p in child)
                        {
                            p.OnExit();
                        }
                    }
                    if (isDestroy)
                    {
                        UIManager.RemovePanel(tempPanel.GetType());
                        GameObject.Destroy(tempPanel.gameObject);
                    }
                }

                if (panelStack.Count > 0)
                {
                    panelStack.Peek().OnResume();
                }
            }
        }       

        public TPanel PushChildPanel<TPanel>(Type type) where TPanel : BasePanel
        {
            return (TPanel)OnPushChildPanel<TPanel>(type);
        }

        private BasePanel OnPushChildPanel<TPanel>(Type type) where TPanel : BasePanel
        {            
            if (!childStacks.TryGetValue(type, out var stack))
            {
                stack = new Stack<BasePanel>();
                childStacks.Add(type, stack);            
            }
            else
            {
                if (stack.Count > 0)
                {
                    stack.Peek().OnPause();
                }          
            }          
            var panel = UIManager.GetPanel(typeof(TPanel));           
            stack.Push(panel);
            panel.OnEnter();
            panel.transform.SetParent(UIManager.GetPanel(type).transform);
            Debug.Log("面板入栈！该面板 " + panel + " 为子面板," + "该分支权限最高级面板为：" + type);
            return panel;
        }

        public void PopChildPanel<TParent>(bool isDestroy = false)
        {
            OnPopChildPanel<TParent>(isDestroy);
        }

        private void OnPopChildPanel<TParent>(bool isDestroy = false)
        {
            Type type = typeof(TParent);
            if (!childStacks.TryGetValue(type, out var panelStack))
            {
                Debug.LogError("当前层级没有初始化该堆栈，请尝试重试");
            }
            else
            {                
                if (panelStack.Count > 0)
                {
                    var tempPanel = panelStack.Pop();
                    tempPanel.OnExit();
                    tempPanel.transform.SetParent(null);
                    if (isDestroy) 
                    {                       
                        UIManager.RemovePanel(tempPanel.GetType());
                        GameObject.Destroy(tempPanel.gameObject);
                    }
                }

                if (panelStack.Count > 0)
                {
                    panelStack.Peek().OnResume();
                }               
            }
        }

      
    }
}
