#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace XFABManager
{

    public class SelectProjectTreeViewHeader : MultiColumnHeader
    {
        public SelectProjectTreeViewHeader(MultiColumnHeaderState state, SelectProjectTreeView treeView) :base(state) {
            this.treeView = treeView;
        }

        private bool isSelectAll = true;
        private Rect column3;

        private bool lastSelect = false;

        private SelectProjectTreeView treeView;

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {


            if (columnIndex == 2)
            {
                column3.Set(headerRect.x+3, headerRect.y+7, headerRect.width, headerRect.height);
                //GUI.Label(headerRect, "TestA");
                isSelectAll = GUI.Toggle(column3, isSelectAll, column.headerContent);

                if (lastSelect != isSelectAll && this.treeView != null)
                {
                    this.treeView.SelectAll(isSelectAll);
                    lastSelect = isSelectAll;
                }

            }
            else {
                base.ColumnHeaderGUI(column, headerRect, columnIndex);
            }

        }

    }

    public class SelectProjectTreeView : TreeView
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

        private Dictionary<string, bool> projects = new Dictionary<string, bool>();

        public List<XFABProject> BuildProjects
        {
            get
            {

                List<XFABProject> list = new List<XFABProject>();

                foreach (var item in XFABProjectManager.Instance.Projects)
                {
                    if (projects.ContainsKey(item.name) && projects[item.name])
                    {
                        list.Add(item);
                    }
                }

                return list;
            }
        }

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
            retVal[0].minWidth = 50;
            retVal[0].width = 150;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = false;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("DisplayName", "项目显示名!");
            retVal[1].minWidth = 50;
            retVal[1].width = 200;
            retVal[1].maxWidth = 1000;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = false;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("是否打包", "是否打包");
            retVal[2].minWidth = 30;
            retVal[2].width = 100;
            retVal[2].maxWidth = 1000;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = false;
            retVal[2].autoResize = true;

            return retVal;
        }

        public SelectProjectTreeView(TreeViewState state, MultiColumnHeaderState mchs, EditorWindow window) : base(state, new MultiColumnHeader(mchs))
        {
            m_Mchs = mchs;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.window = window;

            projects.Clear();
            foreach (var item in XFABProjectManager.Instance.Projects)
            {
                projects.Add(item.name, true);
            }


            this.multiColumnHeader = new SelectProjectTreeViewHeader(mchs, this);

        }

        protected override TreeViewItem BuildRoot()
        {
            if (textStyle == null)
            {
                textStyle = new GUIStyle(EditorStyles.label);
                textStyle.richText = true;
            }

            TreeViewItem root = new TreeViewItem(-1, -1);

            foreach (var item in projects.Keys)
            {
                TreeViewItem groupItem = new TreeViewItem(item.GetHashCode(), 0, item);
                root.AddChild(groupItem);
            }


            return root;
        }
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                // 如果正在重命名 就不用绘制当前这一行了
                if (args.isRenaming) { continue; }
                CellGUI(args.GetCellRect(i), args.item, m_Mchs.visibleColumns[i], ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            XFABProject project = XFABProjectManager.Instance.Projects.Where(x => x.name.Equals(item.displayName)).FirstOrDefault();
            if (project == null) { return; }

            switch (column)
            {
                case 0:         // Name
                    CellGUIDisplayName(cellRect, item);
                    break;
                case 1:         // DisplayName
                    GUI.Label(cellRect, project.displayName);



                    break;
                case 2:         // 更新模式
                    if (projects.ContainsKey(project.name))
                    {
                        projects[project.name] = EditorGUI.Toggle(cellRect, projects[project.name]);
                    }

                    break;


                    //}

            }

        }

        private void CellGUIDisplayName(Rect cellRect, TreeViewItem item)
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


        public void SelectAll(bool select) {

            //foreach (var item in projects.Keys)
            //{
            //    projects[item] = select;
            //}

            foreach (var item in XFABProjectManager.Instance.Projects)
            {
                if (projects.ContainsKey(item.name)) {
                    projects[item.name] = select;
                }
            }

        }

        //protected override void ContextClicked()
        //{

        //    if (isContextClickItem)
        //    {
        //        isContextClickItem = false;
        //        return;
        //    }
        //    GenericMenu menu = new GenericMenu();

        //    //if (!AssetBundleModel.Model.DataSource.IsReadOnly())
        //    //{
        //    menu.AddItem(new GUIContent("Add new group"), false, AddNewGroup, null);
        //    menu.ShowAsContext();
        //}
        //protected override void ContextClickedItem(int id)
        //{

        //    //Debug.Log(" context click item: "+id);
        //    isContextClickItem = true;
        //    GenericMenu menu = new GenericMenu();

        //    TreeViewItem item = FindItem(id, rootItem);

        //    if (item != null)
        //    {

        //        if (item.depth == 0)
        //        {
        //            menu.AddItem(new GUIContent("Add new group"), false, AddNewGroup, null);
        //            menu.AddItem(new GUIContent("Add new profile"), false, CreateNewProfile, item);
        //            menu.AddItem(new GUIContent("rename"), false, RenameBundle, id);
        //            menu.AddItem(new GUIContent("delete"), false, DeleteBundle, id);
        //        }
        //        else if (item.depth == 1)
        //        {
        //            //menu.AddItem(new GUIContent("Add new profile"), false, CreateNewProfile, null);
        //            menu.AddItem(new GUIContent("rename"), false, RenameBundle, id);
        //            menu.AddItem(new GUIContent("delete"), false, DeleteBundle, id);
        //        }
        //    }


        //    menu.ShowAsContext();
        //}

        protected override bool CanRename(TreeViewItem item)
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
                    TreeViewItem item = FindItem(args.itemID, rootItem);
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

        void CreateNewProfile(object item)
        {

            string name = GetProfileName(((TreeViewItem)item).displayName);

            Profile profile = new Profile(name);
            profile.GroupName = ((TreeViewItem)item).displayName;
            XFABManagerSettings.Instances.Profiles.Add(profile);

            ReloadAndSelect(string.Format("{0}{1}", profile.GroupName, name).GetHashCode(), true);
        }

        void RenameBundle(object id)
        {
            ReloadAndSelect((int)id, true);
        }

        void DeleteBundle(object id)
        {

            TreeViewItem item = FindItem((int)id, rootItem);

            if (item.displayName.Equals("Default"))
            {
                this.window.ShowNotification(new GUIContent("Default Profile 不能删除!"));
                return;
            }

            if (item != null)
            {
                if (item.depth == 0)
                {
                    // 删除 Group 
                    XFABManagerSettings.Instances.Groups.RemoveAll(x => x.Equals(item.displayName));
                    XFABManagerSettings.Instances.Profiles.RemoveAll(x => x.GroupName.Equals(item.displayName));
                }
                else if (item.depth == 1)
                {
                    // 删除 Profile
                    XFABManagerSettings.Instances.Profiles.RemoveAll(x => x.name.Equals(item.displayName));
                }

                Reload();
            }
        }

        void AddNewGroup(object id)
        {
            string name = GetGroupName();

            //Profile profile = new Profile(name);
            //XFABManagerSettings.Settings.Profiles.Add(profile);

            XFABManagerSettings.Instances.Groups.Add(name);

            Profile profile = new Profile();
            profile.GroupName = name;
            XFABManagerSettings.Instances.Profiles.Add(profile);

            ReloadAndSelect(name, true);
        }

        private void ReloadAndSelect(string name, bool rename)
        {
            Reload();

            var selection = new List<int>();
            TreeViewItem item = FindItemByName(name);
            if (item == null) { return; }
            selection.Add(item.id);
            ReloadAndSelect(selection);
            if (rename)
            {
                BeginRename(FindItem(item.id, rootItem));
            }
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
        public string GetProfileName(string group)
        {

            int index = 0;
            string name = null;
            do
            {
                index++;
                name = string.Format("new profile{0}", index);
            } while (XFABManagerSettings.Instances.IsContainsProfileName(name, group));

            return name;
        }

        public string GetGroupName()
        {
            int index = 0;
            string name = null;
            do
            {
                index++;
                name = string.Format("new group{0}", index);
            } while (XFABManagerSettings.Instances.Groups.Contains(name));

            return name;
        }

        private TreeViewItem FindItemByName(string name)
        {

            foreach (var item in rootItem.children)
            {
                if (item.displayName.Equals(name))
                {
                    return item;
                }
            }

            return null;
        }

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
