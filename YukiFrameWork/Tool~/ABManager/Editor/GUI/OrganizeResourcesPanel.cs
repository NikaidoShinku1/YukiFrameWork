using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

// 整理资源页面
public class OrganizeResourcesPanel : EditorWindow
{

    [SerializeField]
    public UnityEditor.DefaultAsset res_forlder;

    [SerializeField]
    public UnityEditor.DefaultAsset organize_forlder;

    private int CommonCount = 5;

    private void OnGUI() {
        GUILayout.Space(50);
        res_forlder = (DefaultAsset)EditorGUILayout.ObjectField(new GUIContent("资源文件夹","资源所在的文件夹!"), res_forlder,typeof(DefaultAsset),false);
        GUILayout.Space(50);
        organize_forlder = (DefaultAsset)EditorGUILayout.ObjectField(new GUIContent("整理文件夹","需要整理的文件夹!"), organize_forlder, typeof(DefaultAsset), false);
        GUILayout.Space(50);

        CommonCount = EditorGUILayout.IntField(new GUIContent("CommonCount","当一个资源被多少个资源引用时 会放到通用文件夹!"),CommonCount);
        
        GUILayout.Space(50);


        if (GUILayout.Button("开始整理")) {
            Organize();
            //UnityEngine.Debug.Log("整理资源!");
        } 
    }

    private void Organize() {

        string res_folder_path = AssetDatabase.GetAssetPath(res_forlder);

        if (res_forlder == null || !AssetDatabase.IsValidFolder(res_folder_path)) {
            this.ShowNotification(new GUIContent("请选择资源文件夹!"));
            return;
        }

        string organize_forlder_path = AssetDatabase.GetAssetPath(organize_forlder);

        if (organize_forlder == null || !AssetDatabase.IsValidFolder(organize_forlder_path))
        {
            this.ShowNotification(new GUIContent("请选择需要整理的资源文件夹!"));
            return;
        }

        // 创建文件夹 并清理文件夹
        string organize_folder = string.Format("{0}/{1}(AutoGeneration)", organize_forlder_path, System.IO.Path.GetFileNameWithoutExtension(organize_forlder_path));

        if (AssetDatabase.IsValidFolder(organize_folder))
        {
            // 如果当前已经有这个文件夹了 把这个文件夹中的所有文件移出来
            string[] organize_folder_files = AssetDatabase.FindAssets("*", new string[] { organize_folder });

            foreach (string organize_file in organize_folder_files)
            { 
                string oldPath = AssetDatabase.GUIDToAssetPath(organize_file); 
                //if (AssetDatabase.IsValidFolder(oldPath)) continue; // 文件夹不需要移动 
                string newPath = string.Format("{0}/{1}", organize_forlder_path, System.IO.Path.GetFileName(oldPath)); 
                AssetDatabase.MoveAsset(oldPath, newPath);
            }

            AssetDatabase.DeleteAsset(organize_folder);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 查询资源文件夹下所有的资源
        string[] all_asset = AssetDatabase.FindAssets("*", new string[] { res_folder_path });

        for (int i = 0; i < all_asset.Length; i++) {
            all_asset[i] = AssetDatabase.GUIDToAssetPath(all_asset[i]);
            //Debug.Log(all_asset[i]);
        }
        
        Dictionary<string, List<string>> asset_depend_on = new Dictionary<string, List<string>>();

        foreach (string asset in all_asset) {
              
            List<string> asset_depend = new List<string>();
            asset_depend.AddRange(AssetDatabase.GetDependencies(asset, true));
            if ( asset_depend.Count == 0 || asset_depend_on.ContainsKey(asset)) continue;
            asset_depend_on.Add(asset, asset_depend);
        }
        // 获取需要整理的文件夹下所有的文件
        string[] all_need_organize_res = AssetDatabase.FindAssets("*", new string[] { organize_forlder_path });

        for (int i = 0; i < all_need_organize_res.Length; i++)
        {
            all_need_organize_res[i] = AssetDatabase.GUIDToAssetPath(all_need_organize_res[i]);
        }

        Dictionary<string,List<string>> asset_referenced = new Dictionary<string, List<string>>();
        for (int i = 0; i < all_need_organize_res.Length; i++)
        {
            List<string> list = new List<string>();
            foreach (var item in asset_depend_on.Keys)
            {
                if (asset_depend_on[item].Contains( all_need_organize_res[i])) {
                    list.Add(item);
                }
            }

            if (list.Count != 0 )
                asset_referenced.Add(all_need_organize_res[i], list);
        }

        Dictionary<string, List<string>> folder_assets = new Dictionary<string, List<string>>();

        // 分类到文件夹
        foreach (var item in asset_referenced.Keys)
        {
            if (asset_referenced[item].Count > CommonCount)
            {
                // 归类到Common
                AddToDictionary("Common(AutoGeneration)", item,folder_assets);
            }
            else {
                // 归类到目标文件夹
                AddToDictionary(ListToString(asset_referenced[item]), item, folder_assets);
            }
        }

        Debug.LogFormat("资源整理完成!{0}", folder_assets.Count);
         
        AssetDatabase.CreateFolder(organize_forlder_path, string.Format("{0}(AutoGeneration)", System.IO.Path.GetFileNameWithoutExtension(organize_forlder_path)));
 
        foreach (var item in folder_assets.Keys)
        {
            // 创建文件夹
            AssetDatabase.CreateFolder(organize_folder, item); 
            foreach (var file in folder_assets[item])
            {
                //Debug.LogFormat("-------文件:{0}", file); 
                // 移动文件
                AssetDatabase.MoveAsset(file, string.Format("{0}/{1}/{2}", organize_folder, item,System.IO.Path.GetFileName(file)));
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 清理掉空的文件夹
        string[] sub_folders = AssetDatabase.GetSubFolders(organize_forlder_path);
        for (int i = 0; i < sub_folders.Length; i++)
        {
            if ( AssetDatabase.IsValidFolder( sub_folders[i]) && AssetDatabase.FindAssets("*",new string[] { sub_folders[i] }).Length == 0) {
                AssetDatabase.DeleteAsset(sub_folders[i]);
            }
        }

    }

    private void AddToDictionary(string folder, string filePath, Dictionary<string, List<string>> target) {

        if (string.IsNullOrEmpty(folder)) return;
        if (string.IsNullOrEmpty(filePath)) return;
        
        if (target.ContainsKey(folder))
        {
            target[folder].Add(filePath);
        }
        else {
            List<string> list = new List<string>();
            list.Add(filePath);
            target.Add(folder, list);
        }
    }

    private string ListToString(List<string> list) {
        if (list.Count == 0) return string.Empty;
        StringBuilder builder = new StringBuilder();
        foreach (var item in list) { 
            builder.Append( System.IO.Path.GetFileNameWithoutExtension( item));
            builder.Append("_");
        }
        builder.Append("(AutoGeneration)");

        return builder.ToString();
    }


}
