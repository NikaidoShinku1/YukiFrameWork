using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XFABManager;

public class BundleListTree : TreeView
{

    #region 变量

    

    //private XFABProject project;
    private XFAssetBundleProjectMain mainWindow;
    private bool isContextClickItem = false;

    private AssetBundlesPanel bundlesPanel;

    string nameMatch = "^[A-Za-z0-9-_]+$";

    Texture2D folderIcon = AssetDatabase.GetCachedIcon("Assets") as Texture2D;
    Rect iconRect;


    private GUIContent bundlePackageType = new GUIContent();

    #endregion


    #region 重写方法


    protected override TreeViewItem BuildRoot()
    {
        return CreateView();
    }
    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        return base.BuildRows(root);
    }

    protected override void ContextClicked()
    {
       
        if ( isContextClickItem ) {
            isContextClickItem = false;
            return;
        }
        GenericMenu menu = new GenericMenu();

        //if (!AssetBundleModel.Model.DataSource.IsReadOnly())
        //{
        menu.AddItem(new GUIContent("Add new bundle"), false, CreateNewBundle, null);
        menu.AddItem(new GUIContent("Add new group"), false, CreateNewGroup, null); 
        menu.ShowAsContext();
    }
    protected override void ContextClickedItem(int id)
    {

        //Debug.Log(" context click item: "+id);
        isContextClickItem = true;

        TreeViewItem item = FindItem(id, rootItem);

        if (item == null) return;

        XFABAssetBundle bundle = mainWindow.Project.GetAssetBundle(item.displayName);

        if (bundle == null) return;

        GenericMenu menu = new GenericMenu();

        if (item.depth == 0)
        { 
            menu.AddItem(new GUIContent("Add new bundle"), false, CreateNewBundle, bundle);
            menu.AddItem(new GUIContent("Add new group"), false, CreateNewGroup, null);
        }

        menu.AddItem(new GUIContent("rename"), false, RenameBundle, id);
        menu.AddItem(new GUIContent("delete"), false, DeleteBundle, id);
        menu.ShowAsContext();
    }

    protected override void RenameEnded(RenameEndedArgs args)
    {
        base.RenameEnded(args);
        if (args.newName.Length > 0)
        {

            //Debug.Log( string.Format( " newName: {0} originalName:{1} ",args.newName ,args.originalName ) );

            if ( !args.newName.ToLower().Equals( args.originalName )  ) {

                if (!Regex.IsMatch(args.newName, nameMatch))
                {
                    this.mainWindow.ShowNotification(new GUIContent("AssetBundle名称只能为 字母 数字 -和_ 。"));
                    return;
                }

                // 重命名
                if (mainWindow.Project.RenameAssetBundle(args.newName.ToLower(), args.originalName)) { 
                    ReloadAndSelect(args.newName.ToLower().GetHashCode(),false);
                }
            }
        }
        else
        {
            args.acceptedRename = false;
        }
    }

    protected override bool CanRename(TreeViewItem item)
    {
        return item != null && item.displayName.Length > 0;
    }

    protected override bool CanMultiSelect(TreeViewItem item)
    {
        return true;
    }

    protected override void SelectionChanged(IList<int> selectedIds) {

        string bundle_name = null;

        if (selectedIds != null && selectedIds.Count != 0)
        {

                var item = FindItem(selectedIds[0], rootItem);
                if (item != null )
                {
                    bundle_name = item.displayName;
                }
        }

        //Debug.Log( string.Format( " select bundle name {0} ",bundle_name));
        bundlesPanel.UpdateSelectBundle(bundle_name);
        //m_Controller.UpdateSelectedBundles(selectedBundles);

    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
        {
            SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
        }
 
        if ( this.rootItem.children != null && mainWindow.Project.assetBundles.Count != this.rootItem.children.Count) {
            Reload();
        }
 

    }

    protected override void RowGUI(RowGUIArgs args)
    {
        XFABAssetBundle bundle = mainWindow.Project.GetAssetBundle(args.item.displayName);
        if (bundle != null && bundle.bundleType == XFBundleType.Group)
        {
            iconRect.Set(15, args.rowRect.y, 15, 15);
            GUI.DrawTexture(iconRect, folderIcon, ScaleMode.ScaleToFit);
        }
        args.rowRect.x += 30;
        if (args.item.depth == 1) { 
            args.rowRect.x += 10;
            args.rowRect.width -= 10;
        }

        int width = 100;

        Rect labelRect = new Rect(args.rowRect.x,args.rowRect.y,args.rowRect.width - width ,args.rowRect.height );

        DefaultGUI.Label(labelRect, args.item.displayName, args.selected, args.focused);

        //EditorGUILayout.DropdownButton()  


        if (bundle != null && bundle.bundleType == XFBundleType.Bundle) 
        {
            // 绘制下拉按钮 让用户选择是单独打包 还是打到一个包中 
            Rect r = new Rect(args.rowRect.x + args.rowRect.width - width, args.rowRect.y, width - 30, args.rowRect.height);

            bundlePackageType.text = bundle.bundlePackgeType.ToString();
            if (bundle.bundlePackgeType == XFBundlePackgeType.One)
                bundlePackageType.tooltip = "所有资源打到一个ab包中!";
            else
                bundlePackageType.tooltip = "每个资源都单独打包!";

            if (EditorGUI.DropdownButton(r, bundlePackageType, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();

                foreach (var item in Enum.GetValues(typeof(XFBundlePackgeType)))
                {

                    menu.AddItem(new GUIContent(item.ToString()), (XFBundlePackgeType)item == bundle.bundlePackgeType, () =>
                    {
                        bundle.bundlePackgeType = (XFBundlePackgeType)item;
                        mainWindow.Project.Save();
                    });
                }

                menu.DropDown(r);
            }
        }

        

    }



    #endregion

    #region 方法

    public BundleListTree(TreeViewState state, XFAssetBundleProjectMain mainWindow, AssetBundlesPanel bundlesPanel  ) : base(state )
    { 
        showBorder = true;
        //this.project = mainWindow.Project;
        this.mainWindow = mainWindow;
        this.bundlesPanel = bundlesPanel;
    }

    public TreeViewItem CreateView()
    {
        TreeViewItem root = new TreeViewItem(0, -1);

        if (mainWindow.Project != null)
        {

            for (int i = 0; i < mainWindow.Project.assetBundles.Count; i++)
            {
                XFABAssetBundle bundle = mainWindow.Project.assetBundles[i];
                if (string.IsNullOrEmpty(bundle.group_name) == false) continue;

                TreeViewItem child = new TreeViewItem(bundle.bundle_name.GetHashCode(), 0, bundle.bundle_name);

                if (bundle.bundleType == XFBundleType.Group) {
                    // 查询到这个 group 下面所有的包
                    XFABAssetBundle[] bundles = mainWindow.Project.GetAssetBundlesFromGroup(bundle.bundle_name);
                    if (bundles != null) {
                        foreach (var item in bundles)
                        {
                            TreeViewItem group_child = new TreeViewItem(item.bundle_name.GetHashCode(), 1, item.bundle_name);
                            child.AddChild(group_child);
                        }
                    }
                }
                
                root.AddChild(child);
            }

        }

        return root;
    }
    public string GetBundleName(string template= "new_bundle")
    {

        int index = 0;
        string name = null;
        do
        {
            name = string.Format("{0}{1}",template, index == 0 ? string.Empty : index.ToString());
            index++;
        } while (mainWindow.Project.IsContainAssetBundleName(name));

        return name;
    }

    void CreateNewBundle(object context)
    {

        XFABAssetBundle bundle = context as XFABAssetBundle;
         
        string name = GetBundleName();

        Debug.Log(" CreateNewBundle : " + name);

        XFABAssetBundle assetBundle = new XFABAssetBundle(name, mainWindow.Project.name);

        if (bundle != null && bundle.bundleType == XFBundleType.Group) {
            assetBundle.group_name = bundle.bundle_name;
        }

        mainWindow.Project.AddAssetBundle(assetBundle);
        ReloadAndSelect(name.GetHashCode(), true);
    }

    void CreateNewGroup(object context)
    {

        string name = GetBundleName("new_group");

        Debug.Log(" CreateNewGroup : " + name);

        XFABAssetBundle assetBundle = new XFABAssetBundle(name, mainWindow.Project.name);
        assetBundle.bundleType = XFBundleType.Group;
        mainWindow.Project.AddAssetBundle(assetBundle);
        ReloadAndSelect(name.GetHashCode(), true);
    }

    void RenameBundle(object id) {
        ReloadAndSelect((int)id, true);
    }

    void DeleteBundle(object id) {

        //List<int> ids = GetSelection() as List<int>;

        foreach (var item in GetSelection())
        {
            mainWindow.Project.RemoveAssetBundle(item);
        }

        //mainWindow.Project.RemoveAssetBundle((int)id);
        Reload();
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

    protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
    {
        if (IsValidDragDrop())
        {
            if (args.performDrop)
            {

                string filename = Path.GetFileNameWithoutExtension(DragAndDrop.paths[0]).ToLower();

                if (mainWindow.Project.IsContainAssetBundleName(filename))
                {
                    XFABAssetBundle b = mainWindow.Project.GetAssetBundle(filename);
                    if (b.files.Count == 1 && b.files[0].AssetPath == DragAndDrop.paths[0])
                    {
                        mainWindow.ShowNotification(new GUIContent("资源已经存在,请勿重复添加!"));
                        return DragAndDropVisualMode.None;
                    }
                }
                 
                string bundleName = GetBundleName(filename).ToLower();
                XFABAssetBundle bundle = new XFABAssetBundle(bundleName,mainWindow.Project.name);

                for (int i = 0; i < DragAndDrop.paths.Length; i++)
                {
                    bundle.AddFile(DragAndDrop.paths[i]);
                }

                mainWindow.Project.AddAssetBundle(bundle);
                // 保存
                mainWindow.Project.Save();

                //Debug.LogFormat("bundleName:{0} contain:{1}", bundleName, mainWindow.Project.IsContainAssetBundleName(bundleName));

                Reload();
            }

            return DragAndDropVisualMode.Copy;
        }
        return DragAndDropVisualMode.None;
    }


    protected bool IsValidDragDrop()
    {
        //can't drag nothing
        if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
            return false;

        for (int i = 0; i < DragAndDrop.paths.Length; i++)
        {
            // 是无效的AssetBundle文件 并且不是目录
            if (!AssetBundleTools.IsValidAssetBundleFile(DragAndDrop.paths[i]) && AssetDatabase.IsValidFolder(DragAndDrop.paths[i]) == false) { return false; }
        }
        return true;
    }

    #endregion
}
#endif