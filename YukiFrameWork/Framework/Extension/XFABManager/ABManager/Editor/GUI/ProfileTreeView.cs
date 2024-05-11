#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XFABManager
{
    public class ProfileTreeView : TreeView
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
                new MultiColumnHeaderState.Column(), 
                new MultiColumnHeaderState.Column(),
            };
            retVal[0].headerContent = new GUIContent("Name", "配置文件名称!");
            retVal[0].minWidth = 50;
            retVal[0].width = 150;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = false;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Url", "AssetBundle 更新地址,AssetBundles文件夹所在的网络路径!");
            retVal[1].minWidth = 50;
            retVal[1].width = 200;
            retVal[1].maxWidth = 1000;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = false;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("UpdateModel", "更新模式:Debug(不检测更新,使用本地资源!),Update(检测更新)");
            retVal[2].minWidth = 30;
            retVal[2].width = 100;
            retVal[2].maxWidth = 1000;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = false;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("LoadModel", "加载模式:\t\nAssets 从编辑器资源加载\t\nAssetBundle 从AssetBundle文件加载!(仅在编辑器模式下有用!正式环境只能从AssetBundle加载!)");
            retVal[3].minWidth = 30;
            retVal[3].width = 100;
            retVal[3].maxWidth = 1000;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = false;
            retVal[3].autoResize = true;

            //retVal[4].headerContent = new GUIContent("Use Default GetProjectVersion", "是否使用默认方式获取项目版本,具体用法详见:xxx!");
            //retVal[4].minWidth = 30;
            //retVal[4].width = 200;
            //retVal[4].maxWidth = 1000;
            //retVal[4].headerTextAlignment = TextAlignment.Left;
            //retVal[4].canSort = false;
            //retVal[4].autoResize = true;

            retVal[4].headerContent = new GUIContent("ProjectName", "项目名,当前这个配置所作用的项目!");
            retVal[4].minWidth = 30;
            retVal[4].width = 200;
            retVal[4].maxWidth = 1000;
            retVal[4].headerTextAlignment = TextAlignment.Left;
            retVal[4].canSort = false;
            retVal[4].autoResize = true;

            return retVal;
        }

        public ProfileTreeView(TreeViewState state, MultiColumnHeaderState mchs,EditorWindow window) : base(state, new MultiColumnHeader(mchs))
        {
            m_Mchs = mchs;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.window = window;
        }

        protected override TreeViewItem BuildRoot()
        {
            if (textStyle == null) {
                textStyle = new GUIStyle(EditorStyles.label);
                textStyle.richText = true;
            }

            TreeViewItem root = new TreeViewItem(-1, -1);


            for (int i = 0; i < XFABManagerSettings.Instances.Groups.Count; i++)
            {
                string group = XFABManagerSettings.Instances.Groups[i];
                TreeViewItem groupItem = new TreeViewItem(group.GetHashCode(), 0, group);

                var profiles = XFABManagerSettings.Instances.Profiles.Where(x => x.GroupName.Equals(group));

                foreach (var profile in profiles)
                {
                    TreeViewItem profileItem = new TreeViewItem(string.Format("{0}{1}", profile.GroupName, profile.name).GetHashCode(), 1, profile.name);
                    groupItem.AddChild(profileItem);
                }
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
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i) {
                // 如果正在重命名 就不用绘制当前这一行了
                if (args.isRenaming) { continue; }
                CellGUI(args.GetCellRect(i), args.item, m_Mchs.visibleColumns[i], ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            if (item.depth == 0)
            {
                // Group 
                if (column == 0)
                    CellGUIDisplayName(cellRect, item);
            }
            else if(item.depth == 1){
                // Profile 
                Profile profile = XFABManagerSettings.Instances.Profiles.Where(a => a.name.Equals(item.displayName) && a.GroupName.Equals(item.parent.displayName)).FirstOrDefault();
                //Profile profileDefault = XFABManagerSettings.Settings.Profiles.Where(a => a.name.Equals("Default") && a.GroupName.Equals(item.parent.displayName)).FirstOrDefault();
                if (profile == null  )
                {
                    return;
                }

                url = profile.url;
                updateModel = profile.updateModel;
                loadModel = profile.loadMode;
                //isUseDefault = profile.useDefaultGetProjectVersion;
                projectName = profile.ProjectName;

                switch (column)
                {
                    case 0:         // Name
                        CellGUIDisplayName(cellRect, item);
                        break;
                    case 1:         // Url

                        if (item.depth == 0) { return; }

                        profile.url = GUI.TextField(cellRect, profile.url);

                        if (!profile.url.Equals(url))
                        {
                            XFABManagerSettings.Instances.Save();
                        }

                        break;
                    case 2:         // 更新模式

                        if (item.depth == 0) { return; }

                        profile.updateModel = (UpdateMode)EditorGUI.EnumPopup(cellRect, profile.updateModel);
                        if (profile.updateModel != updateModel)
                        {
                            XFABManagerSettings.Instances.Save();
                        }
                        break;
                    case 3:

                        if (item.depth == 0) { return; }

                        //DefaultGUI.Label(cellRect, item.FileInfo.AssetPath, args.selected, args.focused);
                        profile.loadMode = (LoadMode)EditorGUI.EnumPopup(cellRect, profile.loadMode);
                        if (profile.loadMode != loadModel)
                        {
                            XFABManagerSettings.Instances.Save();
                        }
                        break;
                    //case 4:

                    //    if (item.depth == 0 ) { return; }

                    //    if (!item.displayName.Equals("Default")) {
                    //        GUI.Label(cellRect, "<color=grey>该选项只能在 Default 中配置!</color>", textStyle);
                    //        return;
                    //    }

                    //    profile.useDefaultGetProjectVersion = EditorGUI.Toggle(cellRect, profile.useDefaultGetProjectVersion);

                    //    if (profile.useDefaultGetProjectVersion != isUseDefault)
                    //    {
                    //        XFABManagerSettings.Instances.Save();
                    //    }
                    //    break;

                    case 4:

                        if (item.depth == 0 ) { return; }

                        if (item.displayName.Equals("Default"))
                        {
                            GUI.Label(cellRect, "<color=grey> Default 中不能配置 ProjectName!</color>", textStyle);
                            return;
                        }

                        profile.ProjectName = EditorGUI.TextField(cellRect, profile.ProjectName);

                        if (profile.ProjectName != projectName)
                        {
                            XFABManagerSettings.Instances.Save();
                        }

                        break;

                }

            }

        }

        private void CellGUIDisplayName(Rect cellRect,TreeViewItem item) {

            cellRect.x += 20;
            cellRect.width -= 20;

            if (item.depth == 1)
            {
                cellRect.x += 10;
                cellRect.width -= 10;
            }

            EditorGUI.LabelField(cellRect, item.displayName);
        }

        protected override void ContextClicked()
        {

            if (isContextClickItem)
            {
                isContextClickItem = false;
                return;
            }
            GenericMenu menu = new GenericMenu();

            //if (!AssetBundleModel.Model.DataSource.IsReadOnly())
            //{
            menu.AddItem(new GUIContent("Add new group"), false, AddNewGroup, null);
            menu.ShowAsContext();
        }
        protected override void ContextClickedItem(int id)
        {

            //Debug.Log(" context click item: "+id);
            isContextClickItem = true;
            GenericMenu menu = new GenericMenu();

            TreeViewItem item = FindItem(id, rootItem);

            if (item != null) {

                if (item.depth == 0) {
                    menu.AddItem(new GUIContent("Add new group"), false, AddNewGroup, null);
                    menu.AddItem(new GUIContent("Add new profile"), false, CreateNewProfile, item);
                    menu.AddItem(new GUIContent("rename"), false, RenameBundle, id);
                    menu.AddItem(new GUIContent("delete"), false, DeleteBundle, id);
                } else if (item.depth == 1) {
                    //menu.AddItem(new GUIContent("Add new profile"), false, CreateNewProfile, null);
                    menu.AddItem(new GUIContent("rename"), false, RenameBundle, id);
                    menu.AddItem(new GUIContent("delete"), false, DeleteBundle, id);
                }
            }

            
            menu.ShowAsContext();
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item != null && item.displayName.Length > 0 ;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
            if (args.newName.Length > 0)
            {
                if (!args.newName.Equals(args.originalName))
                {
                    
                    // 默认的配置不能改名
                    if ( args.originalName.Equals("Default") ) {
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
                    else if(item.depth == 1)
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

                        ReloadAndSelect(string.Format("{0}{1}",profile.GroupName,args.newName).GetHashCode(), false);
                    }
                    XFABManagerSettings.Instances.Save();
                    
                }
            }
            else
            {
                args.acceptedRename = false;
            }

            XFABManagerSettings.Instances.Save();
        }

        void CreateNewProfile(object item)
        {

            string name = GetProfileName(((TreeViewItem)item).displayName);

            Profile profile = new Profile(name);
            profile.GroupName = ((TreeViewItem)item).displayName;
            XFABManagerSettings.Instances.Profiles.Add(profile);

            ReloadAndSelect(string.Format("{0}{1}",profile.GroupName,name).GetHashCode(), true);

            XFABManagerSettings.Instances.Save();
        }

        void RenameBundle(object id)
        {
            ReloadAndSelect((int)id, true);
        }

        void DeleteBundle(object id)
        {

            TreeViewItem item = FindItem((int)id, rootItem);

            if (item.displayName.Equals("Default")) {
                this.window.ShowNotification(new GUIContent("Default Profile 不能删除!"));
                return;
            }

            if ( item != null )
            {
                if (item.depth == 0) {
                    // 删除 Group 
                    XFABManagerSettings.Instances.Groups.RemoveAll(x => x.Equals(item.displayName));
                    XFABManagerSettings.Instances.Profiles.RemoveAll(x => x.GroupName.Equals(item.displayName));
                } else if (item.depth == 1) { 
                    // 删除 Profile
                    XFABManagerSettings.Instances.Profiles.RemoveAll(x => x.name.Equals(item.displayName));
                }

                Reload();
            }

            XFABManagerSettings.Instances.Save();
        }

        void AddNewGroup(object id) {
            string name = GetGroupName();

            //Profile profile = new Profile(name);
            //XFABManagerSettings.Settings.Profiles.Add(profile);

            XFABManagerSettings.Instances.Groups.Add(name);

            Profile profile = new Profile();
            profile.GroupName = name;
            XFABManagerSettings.Instances.Profiles.Add(profile);

            ReloadAndSelect(name, true);
            XFABManagerSettings.Instances.Save();
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
            } while ( XFABManagerSettings.Instances.IsContainsProfileName(name,group) );

            return name;
        }

        public string GetGroupName() {
            int index = 0;
            string name = null;
            do
            {
                index++;
                name = string.Format("new group{0}", index);
            } while ( XFABManagerSettings.Instances.Groups.Contains(name) );

            return name;
        }

        private TreeViewItem FindItemByName(string name) {

            foreach (var item in rootItem.children)
            {
                if (item.displayName.Equals(name)) {
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
