///=====================================================
/// - FileName:      UITable.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/23 15:23:36
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using YukiFrameWork.Pools;
using System.Linq;
namespace YukiFrameWork.UI
{
	public class UITable : IDisposable
	{
		private ActivityPanelTable activityTable;
        private StackPanelTable stackPanelTable;   
        public UITable()
        {
            activityTable = new ActivityPanelTable();
            stackPanelTable = new StackPanelTable();
        }
        public void Dispose()
        {
            activityTable.Dispose();
            stackPanelTable.Dispose();
        }

        internal IPanel GetActivityPanel(string name,UILevel level,PanelOpenType openType)
        {
            return activityTable[level, name,openType];
        }

        internal IPanel GetActivityPanel(Type type,UILevel level,PanelOpenType openType)
        {
            return activityTable[level,type,openType];
        }

        internal IPanel[] GetActivityPanels(string name, UILevel level)
        {
            return activityTable[level, name];
        }

        internal IPanel[] GetActivityPanels(Type type, UILevel level)
        {
            return activityTable[level, type];
        }

        internal void ChangeLevelByActivityRemove(IPanel panel)
        {
            activityTable.ChangeLevelByRemove(panel);
        }

        internal void RemoveActivityPanel(PanelInfo info)
        {
            activityTable.Remove(info);
        }

        internal IPanel GetPanelByLevel(string name)
        {
            return activityTable.GetPanelByLevel(name);
        }

        public void AddActivityPanel(IPanel panel)
        {
            PanelInfo info = new PanelInfo();
            info.panel = panel;
            info.panelType = panel.GetType();
            info.panelName = panel.gameObject.name;
            info.level = panel.Level;          
            activityTable.Add(info);
        }

        public void PushPanel(PanelInfo panelInfo)
        {
            stackPanelTable.Add(panelInfo);
        }

        public void PopPanel(PanelInfo panelInfo)
        {
            stackPanelTable.Remove(panelInfo);
        }
    }

    public abstract class UIKitTable<Enumerable> : IDisposable where Enumerable : IEnumerable
    {        
        protected Dictionary<UILevel, Enumerable> container = new Dictionary<UILevel, Enumerable>();

        public void Dispose()
        {
            container.Clear();
        }

        public abstract void Add(PanelInfo info);
        public abstract void Remove(PanelInfo info);
    }

	public class ActivityPanelTable : UIKitTable<Dictionary<string, List<IPanel>>>
	{	
		public ActivityPanelTable()
		{
			for (UILevel i = UILevel.BG; i < UILevel.Top + 1; i++)
			{
				container.Add(i, new Dictionary<string, List<IPanel>>());
			}
		}
        private Dictionary<Type,string> panelTypeDicts = new Dictionary<Type,string>();       

        public override void Add(PanelInfo info)
        {
            UILevel level = info.level;
            if (!container[level].TryGetValue(info.panelName, out var list))
            {
                list = ListPools<IPanel>.Get();
                container[level][info.panelName] = list;                
            }
            panelTypeDicts[info.panelType] = info.panelName;
            list.Add(info.panel);          
        }

        public override void Remove(PanelInfo info)
        {
            UILevel level = info.level;
            IPanel panel = info.panel;
            string name = info.panelName;
            if (container[level].TryGetValue(name, out var list))
            {                
                list.Remove(panel);                        
                if (list.Count == 0)
                {
                    container[level].Remove(name);
                }
            }
        }

        public IPanel GetPanelByLevel(string name)
        {
            for (UILevel i = UILevel.BG; i < UILevel.Top + 1; i++)
            {
                if (container.TryGetValue(i, out var valuePairs))
                {
                    if (valuePairs.TryGetValue(name, out var list))
                    {
                        return list.FirstOrDefault();
                    }
                }
            }
            return null;
        }

        public void ChangeLevelByRemove(IPanel panel)
        {
           
            for (UILevel i = UILevel.BG; i <= UILevel.Top; i++)
            {
                if (i == panel.Level) continue;
                PanelInfo panelInfo = new PanelInfo()
                {
                    panel = panel,
                    panelName = panel.gameObject.name,
                    level = i
                };

                Remove(panelInfo);
            }
        }

        public IPanel this[UILevel level, string name,PanelOpenType openType]
        {
            get
            {
                try
                {
                    return openType == PanelOpenType.Multiple ? container[level][name].Find(x => x.IsActive == false)
                        : container[level][name].FirstOrDefault();
                }
                catch {  return null;}
            }
        }

        public IPanel this[UILevel level,Type type, PanelOpenType openType]
        {
            get
            {
                try
                {
                    string name = panelTypeDicts[type];
                    return openType == PanelOpenType.Multiple ? container[level][name].Find(x => x.IsActive == false)
                       : container[level][name].FirstOrDefault();
                }
                catch { return null; }
            }
        }

        public IPanel[] this[UILevel level, string name]
        {
            get
            {
                try
                {
                    return container[level][name].ToArray();
                }
                catch
                {
                    return null;
                }
            }
        }

        public IPanel[] this[UILevel level, Type type]
        {
            get
            {
                try
                {
                    string name = panelTypeDicts[type];
                    return container[level][name].ToArray();
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    public class PanelElementTable : IEnumerable, IEnumerable<IPanel>, IGlobalSign
    {
        private List<IPanel> panels = new List<IPanel>();
        public Type panelType;
        public bool IsNull => panels.Count == 0;
        public int index;
        [DisableEnumeratorWarning]
        public IEnumerator GetEnumerator()
        {
            return panels.GetEnumerator();
        }

        public int Count => panels.Count;

        public bool IsMarkIdle { get; set; }

        public IPanel this[int index]
            => panels[index];

        public void Add(IPanel panel) => panels.Add(panel);
        public void Remove(IPanel panel) => panels.Remove(panel);
        public void RemoveAt(int index) => panels.RemoveAt(index);

        public void OnPause()
        {
            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].Pause();
            }
        }

        public void OnResume()
        {
            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].Resume();
            }
        }

        public void Clear()
        {
            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].Exit();                
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (!panels[i].IsPanelCache)
                {
                    panels[i].gameObject.Destroy();
                }
            }
            panels.Clear();
        }

        public IPanel Find(Predicate<IPanel> func) => panels.Find(func);

        IEnumerator<IPanel> IEnumerable<IPanel>.GetEnumerator()
        {
            return panels.GetEnumerator();
        }

        void IGlobalSign.Init()
        {
            
        }

        void IGlobalSign.Release()
        {
            panels.Clear();
            panelType = null;
            index = -1;
        }
    }

    public class StackPanelTable : UIKitTable<PanelStack>
    {            
        public StackPanelTable()
        {
            for (UILevel i = UILevel.BG; i < UILevel.Top + 1; i++)
            {
                container.Add(i, new PanelStack());
            }
        }        

        public override void Add(PanelInfo info)
        {
            var panel = info.panel;           
            var stack = container[panel.Level];

            if (stack.Count > 0)
            {
                var list = stack.Peek();

                if (list.panelType == panel.GetType() && panel.OpenType == PanelOpenType.Multiple)
                {
                    panel.Enter(info.param);
                    panel.gameObject.SetParent(UIManager.Instance.GetPanelLevel(panel.Level));
                    panel.gameObject.transform.SetAsLastSibling();
                    list.Add(panel);
                    return;
                }
                else
                {
                    list.OnPause();
                }
            }           
            panel.Enter(info.param);
            panel.gameObject.SetParent(UIManager.Instance.GetPanelLevel(panel.Level));
            panel.gameObject.transform.SetAsLastSibling();           
            var core = GlobalObjectPools.GlobalAllocation<PanelElementTable>();
            core.panelType = panel.GetType();
            core.Add(panel);
            stack.Push(core);          
        }    
       
        public override void Remove(PanelInfo info)
        {                      
            var stack = container[info.level];          
            if (stack.Count > 0)
            {
                var list = stack.Peek();
                if (!info.levelClear)
                {
                    var current = stack.Find(x => x.panelType == info.panelType);
                    if (current != null
                        && list == current)
                    {
                        if (info.panel != null)
                        {
                            info.panel.Exit();
                            list.Remove(info.panel);
                            if (!info.panel.IsPanelCache)
                                info.panel.gameObject.Destroy();
                        }
                        else
                        {
                            list.Clear();
                        }
                        if (list.Count == 0)
                        {
                            var core = stack.Pop();
                            GlobalObjectPools.GlobalRelease(core);
                        }

                        if (stack.Count > 0)
                        {
                            list = stack.Peek();
                            list.OnResume();
                        }
                    }
                    else if (current != null)
                    {
                        if (info.panel != null)
                        {
                            info.panel.Exit();
                            current.Remove(info.panel);
                            if (!info.panel.IsPanelCache)
                                info.panel.gameObject.Destroy();
                        }
                        else
                        {
                            current.Clear();
                        }
                        if (current.Count == 0)
                        {
                            stack.Remove(current);
                            GlobalObjectPools.GlobalRelease(current);
                        }
                    }
                }
                else
                {
                    stack.Clear();
                }
            }          
        }
    }

    public class PanelStack : IEnumerable
    {
        private FastList<PanelElementTable> mTables = new FastList<PanelElementTable>();
       
        public void Push(PanelElementTable elementTable)
        {
            elementTable.index = mTables.Count;
            mTables.Add(elementTable);    
        }

        public int Count => mTables.Count;

        public PanelElementTable Pop()
        {          
            if (mTables.Count == 0) return null;
            PanelElementTable table = mTables[Count - 1];
            mTables.Remove(table);           
            return table;      
        }

        public PanelElementTable Peek()
        {
            return mTables.LastOrDefault();
        }
        [DisableEnumeratorWarning]
        public IEnumerator GetEnumerator()
        {
            return mTables.GetEnumerator();
        }

        public PanelElementTable Find(Func<PanelElementTable, bool> func)
        {
            foreach (var item in mTables)
            {
                if (func.Invoke(item))
                {
                    return item;
                }
            }

            return null;
        }

        public bool Remove(PanelElementTable elementTable)
        {
            mTables.Remove(elementTable);

            for (int i = 0; i < mTables.Count; i++)
            {
                mTables[i].index = i;
            }

            return true;
        }

        public void Clear()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                var table = mTables[i];
                table.Clear();
            }
            mTables.Clear();
        }
    }
}
