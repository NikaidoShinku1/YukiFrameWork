using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;



namespace XFABManager
{
    public class AssetTreeItem : TreeViewItem
    {

        private FileInfo fileInfo;
        
        public FileInfo FileInfo {

            get {

                return fileInfo;
            }
        }

        public AssetTreeItem() : base(-1, -1) { }
        public AssetTreeItem(FileInfo a,int depeth = 0) : base(a != null ? a.guid.GetHashCode() : Random.Range(int.MinValue, int.MaxValue), depeth, a != null ? a.displayName : "failed")
        {
            fileInfo = a;
            if (a != null)
                icon = AssetDatabase.GetCachedIcon(a.AssetPath) as Texture2D;
        }



        //private Color m_color = new Color(0, 0, 0, 0);
        //internal Color itemColor
        //{
        //    get
        //    {
        //        if (m_color.a == 0.0f && fileInfo != null)
        //        {
        //            m_color = fileInfo.GetColor();
        //        }
        //        return m_color;
        //    }
        //    set { m_color = value; }
        //}
        //internal Texture2D MessageIcon()
        //{
        //    return MessageSystem.GetIcon(HighestMessageLevel());
        //}
        //internal MessageType HighestMessageLevel()
        //{
        //    return m_asset != null ?
        //        m_asset.HighestMessageLevel() : MessageType.Error;
        //}

        //internal bool ContainsChild(AssetInfo asset)
        //{
        //    bool contains = false;
        //    if (children == null)
        //        return contains;

        //    if (asset == null)
        //        return false;
        //    foreach (var child in children)
        //    {
        //        var c = child as AssetTreeItem;
        //        if (c != null && c.asset != null && c.asset.fullAssetName == asset.fullAssetName)
        //        {
        //            contains = true;
        //            break;
        //        }
        //    }

        //    return contains;
        //}

    }


}

