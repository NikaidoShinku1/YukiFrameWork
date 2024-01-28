#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.ABManager
{


    [System.Serializable]
    public enum BundleFileType
    {

        File,       // 文件 
        Directory  // 目录 档期这个文件夹下所有的文件

    }

    [System.Serializable]
    public enum BundleType
    {

        Bundle, // 一个AB包
        Group   // 一组AB包

    }


    [System.Serializable]
    public enum BundlePackgeType {
        /// <summary>
        /// 所有的资源打包到一个ab包中
        /// </summary>
        One,
        /// <summary>
        /// 每个资源都单独打包
        /// </summary>
        Multiple
    }

    [System.Serializable]
    public class FileInfo {

        public static string[] FilterTypes = new string[] {"All", "AnimationClip", "AudioClip", "AudioMixer", "ComputeShader", 
            "Font", "GUISkin", "Material", "Mesh", "Model", "PhysicMaterial", "Prefab","Scene","Script","Shader","Sprite","Texture","VideoClip" };

        public string guid;
        
        public string displayName => Path.GetFileNameWithoutExtension(AssetPath);

        public BundleFileType type;

        //private long size;
        private System.IO.FileInfo fileInfo;
        public string AssetPath {
            get {
                return AssetDatabase.GUIDToAssetPath(guid);
            }
        }

        public bool Exists
        {
            get
            {
                string assetPath = AssetPath;
                if (string.IsNullOrEmpty(assetPath))
                {
                    return false;
                }
                 
                return File.Exists(assetPath) || System.IO.Directory.Exists(assetPath);

                //if (type == BundleFileType.File)
                //{
                //    return File.Exists(AssetPath);
                //}
                //else
                //{ 
                //    return AssetDatabase.IsValidFolder(AssetPath);
                //}
            }
        }

        /// <summary>
        /// 返回所有的文件路径
        /// </summary>
        public string[] Files
        {
            get
            {
                List<string> files = new List<string>();
                if (type == BundleFileType.Directory)
                {

                    string[] guids = null;

                    if (string.IsNullOrEmpty(filter) || filter.Equals("All") || filter.Equals("all") || filter.Equals("ALL"))
                        guids = AssetDatabase.FindAssets("", new string[] { AssetPath });
                    else 
                        guids = AssetDatabase.FindAssets(string.Format("t:{0}",filter), new string[] { AssetPath });
                    
                    // 去除重复的
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string file_path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        if (!files.Contains(file_path) && AssetBundleTools.IsValidAssetBundleFile(file_path))
                            files.Add(file_path);
                    }
                }
                else if (type == BundleFileType.File)
                    files.Add(AssetPath);

                return files.ToArray();
            }
        }
        public FileInfo() { }

        public FileInfo(string guid) {
            this.guid = guid;
            //this.displayName = Path.GetFileNameWithoutExtension( AssetPath );
            //DirectoryInfo directoryInfo = new DirectoryInfo(AssetPath);
            this.type = AssetDatabase.IsValidFolder(AssetPath) ? BundleFileType.Directory : BundleFileType.File;
        }

        public string filter = string.Empty;
    }

    [System.Serializable]
    public class ABAssetBundle
    {

        public string bundle_name;
        public List<FileInfo> files;
        public string projectName;
        private long size;

        //public long Size {

        //    get {
        //        size = 0;
        //        for (int i = 0; i < files.Count; i++)
        //        {
        //            size += files[i].Size;
        //        }

        //        return size;
        //    }
        //}

        public BundleType bundleType = BundleType.Bundle;

        public string group_name;  // 当前这个包是属于哪一组的

        //public string SizeString
        //{
        //    get
        //    {
        //        if (Size == 0)
        //            return "--";
        //        return EditorUtility.FormatBytes(size);
        //    }
        //}

        private ABProject project;

        private ABProject Project {
            get {
                if (project == null) {
                    project = ABProjectManager.Instance.GetProject(projectName);
                }
                return project;
            }
        }

        private Dictionary<string, string> FileAssetPathCache = null;

        public BundlePackgeType bundlePackgeType = BundlePackgeType.One;

        public ABAssetBundle(string projectName) {
            files = new List<FileInfo>();
            this.projectName = projectName;
        }

        public ABAssetBundle(string bundleName, string projectName) {
            bundle_name = bundleName;
            files = new List<FileInfo>();
            this.projectName = projectName;
        }

        public void AddFile(string assetPath,bool ignoreRemoveOther = false) {
            //string asset_path = AssetDatabase.GUIDToAssetPath(guid);
            if ( string.IsNullOrEmpty(assetPath) )
            {
                Debug.Log("文件不存在!");
                return;
            }

            if (!ignoreRemoveOther) 
            {  
                // 判断这个文件是不是存在别的AssetBundle中 ( 一个文件 只能存在一个AssetBundle中 )
                RemoveOtherBundleFile(assetPath);

                // 如果是文件夹 判断这个文件夹下所有的文件
                if (AssetDatabase.IsValidFolder(assetPath))
                {

                    string[] guids = AssetDatabase.FindAssets("", new string[] { assetPath });

                    for (int j = 0; j < guids.Length; j++)
                    {
                        RemoveOtherBundleFile(AssetDatabase.GUIDToAssetPath(guids[j]));
                    }
                }
            }



            FileInfo fileInfo = new FileInfo(AssetDatabase.AssetPathToGUID(assetPath));
            AddFile(fileInfo);
        }

        // 移除其他 bundle 中的文件
        private void RemoveOtherBundleFile(string asset_path)
        {
            if ( Project == null ) {
                Debug.LogErrorFormat("{0} is null! ",projectName);
                return;
            }
            ABAssetBundle currentFileBundle = Project.GetBundleByContainFile(asset_path);
            if (currentFileBundle != null && currentFileBundle != this)
            {
                currentFileBundle.RemoveFile(AssetDatabase.AssetPathToGUID(asset_path));
            }
        }

        // 添加文件 
        private void AddFile(FileInfo fileInfo) {

            // 判断是不是已经存在的
            if ( IsContainFile(fileInfo.guid) ) {

                Debug.LogErrorFormat( "文件{0}已经存在,不能重复添加!",fileInfo.AssetPath);
                return;
            }

            if ( IsDuplicateNames(fileInfo)) 
            {
                Debug.LogErrorFormat("名称{0}重复,不能添加同名文件!不区分大小写!", fileInfo.displayName);
                return;
            }

            //Debug.Log(fileInfo.type);

            if (fileInfo.type == BundleFileType.Directory)
            {
                // 判断当前这个文件夹下的文件 是否 存在 这个Bundle中 ， 如果存在 需要移除
                string[] assets = AssetDatabase.FindAssets("",new string[] { fileInfo.AssetPath });

                for (int i = 0; i < assets.Length; i++)
                {
                    if ( IsContainFile(assets[i]) ) {
                        RemoveFile(assets[i]);
                    }
                }
            }
            else if ( fileInfo.type == BundleFileType.File ) {
                // 判断是否在 当前 Bundle 的文件夹中 
                if ( IsContainFileInFolder(fileInfo.guid) ) {
                    Debug.LogWarning(string.Format("文件{0}已经存在文件夹中,不需要添加!", fileInfo.AssetPath));
                    return;
                }
            }

            files.Add(fileInfo);
        }
        // 移除文件
        private void RemoveFile(FileInfo file) {

            if ( file != null ) {
                files.Remove(file);
            }

        }

        public void RemoveFile(string guid) {
            RemoveFile(GetFileInfo(guid));
        }

        public FileInfo GetFileInfo(string guid) {

            for (int i = 0; i < files.Count; i++)
            {
                if (guid.Equals(files[i].guid))
                {
                    return files[i];
                }
            }
            return null;
        }

        // 判断是不是已经包含文件
        public bool IsContainFile(string guid) {

            //string fileName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));
            string filePath = AssetDatabase.GUIDToAssetPath(guid);

            for (int i = 0; i < files.Count; i++)
            {

                switch (files[i].type)
                {
                    case BundleFileType.File:
                        if (guid.Equals(files[i].guid))
                            return true;
                        break;
                    case BundleFileType.Directory:
                        if (filePath.StartsWith(files[i].AssetPath)) return true;
                        break; 
                }
            }

            return false;
        }

        // 判断是不是有同名文件
        public bool IsDuplicateNames(FileInfo fileInfo) {

            for (int i = 0; i < files.Count; i++)
            {
                if (fileInfo.displayName.ToLower().Equals(files[i].displayName.ToLower()) 
                    && fileInfo.type == BundleFileType.File 
                    && files[i].type == BundleFileType.File) {
                    // 除了文件名相同之外 后缀也必须相同 才认为是相同的文件
                    return System.IO.Path.GetExtension(fileInfo.AssetPath).Equals(System.IO.Path.GetExtension(files[i].AssetPath));
                }
            }

            return false;
        }

        // 判断 这个Bundle中的文件夹 是否包含某个文件
        public bool IsContainFileInFolder(string guid) {

            List<string> folders = new List<string>();

            for (int i = 0; i < files.Count; i++)
            {
                if ( files[i].type == BundleFileType.Directory ) {

                    // 获取这个文件夹下所有的资源 
                    folders.Add(files[i].AssetPath);
                }
            }

            if (folders.Count > 0)
            {
                string[] assets = AssetDatabase.FindAssets("", folders.ToArray());
                for (int i = 0; i < assets.Length; i++)
                {
                    if (guid.Equals(assets[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<FileInfo> GetFileInfos() {

            UpdateFileInfos();
            return files;
        }

        public string[] GetFilePaths() {

            List<string> paths = new List<string>();
            List<string> names = new List<string>(); // 用来检测是否有同名文件

            List<FileInfo> fileInfos = GetFileInfos();
 
            for (int i = 0; i < fileInfos.Count; i++)
            { 
                foreach (var filePath in fileInfos[i].Files)
                {
                    if (!paths.Contains(filePath))
                    {
                        string fileName = Path.GetFileName(filePath);
                        if (names.Contains(fileName)) {
                            Debug.LogErrorFormat("检测到同名文件:{0},请修改文件名称!",filePath);
                            continue;
                        }

                        paths.Add(filePath);
                        names.Add(fileName);
                    }
                    else 
                        Debug.LogErrorFormat("检测到重复文件:{0}",filePath);
                }
                
            }

            return paths.ToArray();
        }

        // 更新文件列表
        public void UpdateFileInfos() {

            for (int i = 0; i < files.Count; i++)
            {
                // 判断文件是否存在 如果不在就移除
                if (!files[i].Exists)
                {
                    files.Remove(files[i]);
                }
            }

        }

        public string GetAssetPathByFileName(string fileName,Type type)
        {
            if (string.IsNullOrEmpty(fileName)) {
                return null;
            }

            if (FileAssetPathCache == null) FileAssetPathCache = new Dictionary<string, string>();

            string fileKey = AssetBundleTools.GetAssetNameWithType(fileName, type);

            if (FileAssetPathCache.ContainsKey(fileKey))
                return FileAssetPathCache[fileKey];

            for (int i = 0; i < files.Count; i++)
            {
                if (files[i].type == BundleFileType.File)
                {
                    if (files[i].displayName.Equals(fileName))
                    { 
                        FileAssetPathCache.Add(fileKey, files[i].AssetPath);
                        return files[i].AssetPath;
                    }
                }
                else if (files[i].type == BundleFileType.Directory)
                {
                    string filter = string.Empty;

                    if (string.IsNullOrEmpty(files[i].filter) || files[i].filter.Equals("All") || files[i].filter.Equals("all") || files[i].filter.Equals("ALL"))
                        filter = fileName;
                    else 
                        filter = string.Format("t:{0} {1}",files[i].filter,fileName);
                     
                    // 获取到目录下面的所有文件 来比较
                    string[] assets = AssetDatabase.FindAssets(filter, new string[] { files[i].AssetPath });

                    foreach (var asset in assets)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(asset);
                        
                        if(!AssetBundleTools.IsValidAssetBundleFile(assetPath)) continue;
                        
                        if ( Path.HasExtension(assetPath) && Path.GetFileNameWithoutExtension(assetPath).Equals(fileName) )
                        {
                            // 判断类型是否符合
                            Type assetType = AssetBundleTools.GetAssetType(assetPath);
                            if (assetType != type) continue; 

                            FileAssetPathCache.Add(AssetBundleTools.GetAssetNameWithType(fileName,assetType), assetPath);
                            return assetPath;
                        }
                    }

                }
            }
            return null;
        }


        public string[] GetAllAssetPaths() {

            //string[] paths = new string[files.Count];

            List<string> paths = new List<string>(files.Count);

            for (int i = 0; i < files.Count; i++)
            {
                if (files[i].type == BundleFileType.File)
                {
                    if (!paths.Contains(files[i].AssetPath)) { 
                        paths.Add(files[i].AssetPath);
                    }
                }
                else if(files[i].type == BundleFileType.Directory) {
                    // 获取到这个目录下面的所有文件
                    foreach (var filePath in files[i].Files)
                    {
                        if (!paths.Contains(filePath))
                            paths.Add(filePath);
                    }
                }
            }

            return paths.ToArray();

        }

    }


}

#endif