using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XFABManager;

public class BundleInfoTree : TreeView
{

    #region 字段

    //private XFABProject project;
    private XFAssetBundleProjectMain mainWindow;
    private string bundle_name;
    private string asset;
    #endregion

    #region 属性

    private XFABAssetBundle Bundle
    {

        get
        {
            return mainWindow.Project.GetAssetBundle(bundle_name);
        }
    }

    #endregion


    public BundleInfoTree(TreeViewState state, XFAssetBundleProjectMain mainWindow) : base(state)
    {
        showBorder = true;
        //this.project = project;
        this.mainWindow = mainWindow;
    }

    protected override TreeViewItem BuildRoot()
    {
        TreeViewItem root = new TreeViewItem(-1, -1);
        UpdateInfo(root);
        return root;
    }
    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        return base.BuildRows(root);
    }

    private void UpdateInfo(TreeViewItem root)
    {

        //if ( Bundle != null ) {

        //    TreeViewItem bundleRoot = new TreeViewItem(0, 0,Bundle.bundle_name);

        //    TreeViewItem size = new TreeViewItem(1, 0, string.Format( "Size:{0}",Bundle.SizeString));
        //    bundleRoot.AddChild(size);

        //    //List<XFABAssetBundle> bundles =  mainWindow.Project.GetDependenciesBundles(Bundle);

        //    //TreeViewItem dependence = new TreeViewItem(2, 0, string.Format("Dependent On : {0}",bundles.Count == 0 ? "None":bundles.Count.ToString()));

        //    //for (int i = 0; i < bundles.Count; i++)
        //    //{
        //    //    TreeViewItem item = new TreeViewItem(Random.Range(int.MinValue, int.MaxValue),1,bundles[i].bundle_name);

        //    //    dependence.AddChild(item);
        //    //}

        //    //bundleRoot.AddChild(dependence);
        //    root.AddChild(bundleRoot);
        //}


        if (!string.IsNullOrEmpty(asset))
        {

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(asset);

            TreeViewItem bundleRoot = new TreeViewItem(0, 0, Bundle.bundle_name);
             
            TreeViewItem size = new TreeViewItem(1, 0, string.Empty);
            if (fileInfo.Exists && fileInfo.Length != 0)
            {
                size.displayName = string.Format("Size:{0}", EditorUtility.FormatBytes(fileInfo.Length));
            }
            else
            {
                size.displayName = "Size:--";
            }

            bundleRoot.AddChild(size);
            root.AddChild(bundleRoot);
        }


        //List<XFABAssetBundle> bundles =  mainWindow.Project.GetDependenciesBundles(Bundle);

        //TreeViewItem dependence = new TreeViewItem(2, 0, string.Format("Dependent On : {0}",bundles.Count == 0 ? "None":bundles.Count.ToString()));

        //for (int i = 0; i < bundles.Count; i++)
        //{
        //    TreeViewItem item = new TreeViewItem(Random.Range(int.MinValue, int.MaxValue),1,bundles[i].bundle_name);

        //    dependence.AddChild(item);
        //}

        //bundleRoot.AddChild(dependence);


    }

    protected override void RowGUI(RowGUIArgs args)
    {

        Color old = GUI.color;
        if (args.item.depth == 1)
            GUI.color = Color.grey * 1.5f;
        base.RowGUI(args);
        GUI.color = old;
    }

    public void SetSelectBundle(string bundle_name)
    {
        this.bundle_name = bundle_name;
        this.asset = null;
        Reload();
    }

    public void SetSelectAssets(string asset)
    {
        this.asset = asset;
        //Debug.LogFormat("AssetPath:{0}",asset);
        Reload();
    }

}
