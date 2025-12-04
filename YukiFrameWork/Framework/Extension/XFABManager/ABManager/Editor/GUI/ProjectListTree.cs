#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XFABManager
{

#if UNITY_6000_2_OR_NEWER

    public class ProjectTreeItem :TreeViewItem<int>
#else
    public class ProjectTreeItem : TreeViewItem
#endif

    {

        public XFABProject Project { get; private set; }

        public ProjectTreeItem(XFABProject project) : base(project.name.GetHashCode(), 0, project.name)
        {
            Project = project;
        }

    }


#if UNITY_6000_2_OR_NEWER
    public class ProjectListTree : TreeView<int>
#else
    public class ProjectListTree : TreeView
#endif

    {
        private string url;
        private UpdateMode updateModel;
        private LoadMode loadModel;
        private bool isUseDefault;
        private string projectName;

        private bool isContextClickItem;

        MultiColumnHeaderState m_Mchs;

        private EditorWindow window;

        private GUIStyle textStyle;

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),

            };
            retVal[0].headerContent = new GUIContent("Name", "项目名!");
            retVal[0].minWidth = 100;
            retVal[0].width = 300;
            retVal[0].maxWidth = 500;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = false;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("DisplayName", "项目显示名!");
            retVal[1].minWidth = 100;
            retVal[1].width = 300;
            retVal[1].maxWidth = 500;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = false;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("版本", "项目当前的版本!");
            retVal[2].minWidth = 100;
            retVal[2].width = 200;
            retVal[2].maxWidth = 300;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = false;
            retVal[2].autoResize = true;

            return retVal;
        }


#if UNITY_6000_2_OR_NEWER
        public ProjectListTree(TreeViewState<int> state, MultiColumnHeaderState mchs, EditorWindow window ) : base(state, new MultiColumnHeader(mchs))
#else
        public ProjectListTree(TreeViewState state, MultiColumnHeaderState mchs, EditorWindow window) : base(state, new MultiColumnHeader(mchs))
#endif

        {
            m_Mchs = mchs;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.window = window;


        }
#if UNITY_6000_2_OR_NEWER
        protected override TreeViewItem<int> BuildRoot()
#else
        protected override TreeViewItem BuildRoot()
#endif

        {
            if (textStyle == null)
            {
                textStyle = new GUIStyle(EditorStyles.label);
                textStyle.richText = true;
            }
#if UNITY_6000_2_OR_NEWER 
            TreeViewItem<int> root = new TreeViewItem<int>(-1, -1); 
#else
            TreeViewItem root = new TreeViewItem(-1, -1);
#endif

            foreach (var item in XFABProjectManager.Instance.Projects)
            {
                ProjectTreeItem groupItem = new ProjectTreeItem(item);
                root.AddChild(groupItem);
            }


            return root;
        }

#if UNITY_6000_2_OR_NEWER

        protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
        {
            return base.BuildRows(root);
        }

#else 
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }
#endif

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                // 如果正在重命名 就不用绘制当前这一行了
                if (args.isRenaming) { continue; }
                CellGUI(args.GetCellRect(i), args.item, m_Mchs.visibleColumns[i], ref args);
            }
        }

#if UNITY_6000_2_OR_NEWER 
        private void CellGUI(Rect cellRect, TreeViewItem<int> item, int column, ref RowGUIArgs args)

#else
        private void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
#endif

        {
            CenterRectUsingSingleLineHeight(ref cellRect);


            ProjectTreeItem groupItem = item as ProjectTreeItem;

            if (groupItem == null) { return; }

            switch (column)
            {
                case 0:         // Name
                    CellGUIDisplayName(cellRect, item);
                    break;
                case 1:         // DisplayName
                    GUI.Label(cellRect, groupItem.Project.displayName);
                    break;
                case 2:         // 更新模式

                    GUI.Label(cellRect, groupItem.Project.version);

                    break;


                    //}

            }



            if (rootItem.hasChildren && rootItem.children.Count != XFABProjectManager.Instance.Projects.Count)
            {

                Reload();
            }

        }


#if UNITY_6000_2_OR_NEWER
        private void CellGUIDisplayName(Rect cellRect, TreeViewItem<int> item)
#else
        private void CellGUIDisplayName(Rect cellRect, TreeViewItem item)
#endif


        {

            cellRect.x += 20;
            cellRect.width -= 20;

            if (item.depth == 1)
            {
                cellRect.x += 10;
                cellRect.width -= 10;
            }

            EditorGUI.LabelField(cellRect, item.displayName);
        }


#if UNITY_6000_2_OR_NEWER
        protected override bool CanRename(TreeViewItem<int> item)
#else
        protected override bool CanRename(TreeViewItem item)
#endif

        {
            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
            if (args.newName.Length > 0)
            {
                if (!args.newName.Equals(args.originalName))
                {

                    // 默认的配置不能改名
                    if (args.originalName.Equals("Default"))
                    {
                        this.window.ShowNotification(new GUIContent("Default Profile 不能改名!"));
                        return;
                    }

#if UNITY_6000_2_OR_NEWER
                    TreeViewItem<int> item = FindItem(args.itemID, rootItem);
#else
                    TreeViewItem item = FindItem(args.itemID, rootItem);
#endif

                    // 判断是重命名Group  还是重命名 Profile
                    if (item.depth == 0)
                    {
                        // Group
                        if (XFABManagerSettings.Instances.Groups.Contains(args.newName))
                        {
                            Debug.LogWarningFormat("重命名失败!名称{0}已经存在!", args.newName);
                            return;
                        }

                        int index = XFABManagerSettings.Instances.Groups.IndexOf(args.originalName);
                        XFABManagerSettings.Instances.Groups[index] = args.newName;

                        // 修改Profile 
                        var profiles = XFABManagerSettings.Instances.Profiles.Where(x => x.GroupName.Equals(args.originalName));

                        foreach (var profile in profiles)
                        {
                            profile.GroupName = args.newName;
                        }

                        ReloadAndSelect(args.newName.GetHashCode(), false);
                        //XFABManagerSettings.Settings.Save();
                    }
                    else if (item.depth == 1)
                    {
                        // Profile
                        // 新名称已经存在
                        if (XFABManagerSettings.Instances.IsContainsProfileName(args.newName, item.parent.displayName))
                        {
                            Debug.LogWarningFormat("重命名失败!名称{0}已经存在!", args.newName);
                            return;
                        }

                        Profile profile = XFABManagerSettings.Instances.Profiles.Where(a => a.name.Equals(args.originalName)).Single();
                        // 重命名
                        profile.name = args.newName;

                        ReloadAndSelect(string.Format("{0}{1}", profile.GroupName, args.newName).GetHashCode(), false);
                    }
                    XFABManagerSettings.Instances.Save();

                }
            }
            else
            {
                args.acceptedRename = false;
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            var item = FindItem(id, rootItem);
            //Debug.Log(item.displayName);
            var project = XFABProjectManager.Instance.Projects.Where(x => x.name.Equals(item.displayName)).FirstOrDefault();

            if (project == null)
            {
                return;
            }

            BaseShowProjects.OpenProject(project);
        }
        private void ReloadAndSelect(int hashCode, bool rename)
        {
            var selection = new List<int>();
            selection.Add(hashCode);
            ReloadAndSelect(selection);
            if (rename)
            {
                BeginRename(FindItem(hashCode, rootItem), 0.25f);
            }
        }

        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes, TreeViewSelectionOptions.RevealAndFrame);
            SelectionChanged(hashCodes);
        }
        //public string GetProfileName(string group)
        //{

        //    int index = 0;
        //    string name = null;
        //    do
        //    {
        //        index++;
        //        name = string.Format("new profile{0}", index);
        //    } while (XFABManagerSettings.Settings.IsContainsProfileName(name, group));

        //    return name;
        //}

        //public string GetGroupName()
        //{
        //    int index = 0;
        //    string name = null;
        //    do
        //    {
        //        index++;
        //        name = string.Format("new group{0}", index);
        //    } while (XFABManagerSettings.Settings.Groups.Contains(name));

        //    return name;
        //}

        //private TreeViewItem FindItemByName(string name)
        //{

        //    foreach (var item in rootItem.children)
        //    {
        //        if (item.displayName.Equals(name))
        //        {
        //            return item;
        //        }
        //    }

        //    return null;
        //}

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }

        }

    }

}
#endif
