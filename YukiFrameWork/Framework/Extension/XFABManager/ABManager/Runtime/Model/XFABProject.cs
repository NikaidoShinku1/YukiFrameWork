
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XFABManager
{
    [System.Serializable]
    public enum AtlasCompressionType {
        None = 0,
        LowQuality,
        NormalQuality,
        HighQuality,
    }


    /// <summary>
    /// 资源项目模块
    /// </summary>
    [System.Serializable]
    public class XFABProject : ScriptableObject
    {

        /// <summary>
        /// 资源项目名称
        /// </summary>
        [HideInInspector]
        public new string name;     // 要唯一 不能重复

        /// <summary>
        /// 显示名称
        /// </summary>
        public string displayName;

        /// <summary>
        /// 当前项目创建的所有AB包 
        /// </summary>
        [HideInInspector]
        public List<XFABAssetBundle> assetBundles;

        private Dictionary<string, XFABAssetBundle> _bundles = null;

        private Dictionary<string, XFABAssetBundle> Bundles
        {
            get {
                if (_bundles == null) { 
                    _bundles = new Dictionary<string, XFABAssetBundle>();
                    foreach (var item in assetBundles)
                    {
                        _bundles.Add(item.bundle_name, item);
                    }
                }

                return _bundles;
            }
        }


        private string _out_path = null;

        /// <summary>
        /// 打包后的AssetBundle存放路径
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public string assetbundle_out_path(BuildTarget buildTarget)
        {
            if (string.IsNullOrEmpty(_out_path))
            {
                _out_path = string.Format("{0}AssetBundles", Application.dataPath.Substring(0, Application.dataPath.Length - 6));
                //_out_path = Application.dataPath.Replace("Assets", "AssetBundles");
            }
            return string.Format("{0}/{1}/{2}/{3}", _out_path, name, version, buildTarget);
        }

        /// <summary>
        /// 打包时AssetBundle的临时存放目录
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public string temp_out_path(BuildTarget buildTarget)
        {
            if (string.IsNullOrEmpty(_out_path))
            {
                _out_path = string.Format("{0}AssetBundles", Application.dataPath.Substring(0, Application.dataPath.Length - 6));
                //_out_path = Application.dataPath.Replace("Assets", "AssetBundles");
            }
            return string.Format("{0}/{1}/{2}/{3}", _out_path, name, "Temps", buildTarget);
        }

        /// <summary>
        /// 依赖的项目名称的集合
        /// </summary>
        [HideInInspector]
        public List<string> dependenceProject;

        /// <summary>
        /// AssetBundle资源文件后缀（默认.unity3d）
        /// </summary>
        [HideInInspector] 
        public string suffix;

        /// <summary>
        /// 当前资源模块的版本
        /// </summary>
        [HideInInspector]
        public string version; 

        /// <summary>
        /// 在打包资源时是否清空文件夹
        /// </summary>
        [HideInInspector]
        public bool isClearFolders;

        /// <summary>
        /// 打包完成后是否把资源复制到StreamingAssets目录
        /// </summary>
        [HideInInspector]
        public bool isCopyToStreamingAssets; 

        /// <summary>
        /// 打包完成后是否把资源压缩成压缩包
        /// </summary>
        [HideInInspector]
        public bool isCompressedIntoZip;

        /// <summary>
        /// 是否自动打包图集（默认true）
        /// </summary>
        [HideInInspector] 
        public bool isAutoPackAtlas = true;

        /// <summary>
        /// 图集压缩类型
        /// </summary>
        [HideInInspector]
        public AtlasCompressionType atlas_compression_type = AtlasCompressionType.NormalQuality;

        /// <summary>
        /// 是否使用Crunch压缩
        /// </summary>
        [HideInInspector]
        public bool use_crunch_compression = true;

        /// <summary>
        /// 压缩质量（范围:0-100,默认50）
        /// </summary>
        [Range(0,100)]
        [HideInInspector]
        public int compressor_quality = 50;

        /// <summary>
        /// 更新信息
        /// </summary>
        [HideInInspector]
        public string update_message;// 更新信息

        /// <summary>
        /// 打包资源的日期
        /// </summary>
        [HideInInspector]
        public string update_date; // 更新日期
        
        /// <summary>
        /// AssetBundle打包选项
        /// </summary>
        [HideInInspector]
        public List<BuildOptionToggleData> buildAssetBundleOptions;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(name);
                if (!string.IsNullOrEmpty(displayName))
                {
                    builder.Append("/").Append(displayName);
                }
                return builder.ToString();
            }
        }


        private Dictionary<string,string> asset_bundle_name_mapping = null; // 文件路径 和 所在AssetBundle的映射

        /// <summary>
        /// asset_path和bundleName的映射
        /// </summary>
        public Dictionary<string, string> AssetBundleNameMapping
        {
            get {
                if (asset_bundle_name_mapping == null || asset_bundle_name_mapping.Count == 0) 
                {
                    if(asset_bundle_name_mapping == null)
                        asset_bundle_name_mapping = new Dictionary<string, string>();

                    List<XFABAssetBundle> allAssetBundle = GetAllAssetBundles();

                    // 把所有的文件全都加进来
                    foreach (var assetbundle in allAssetBundle)
                    {
                        foreach (var assetpath in assetbundle.GetAllAssetPaths())
                        {
                            if (!asset_bundle_name_mapping.ContainsKey(assetpath))
                                asset_bundle_name_mapping.Add(assetpath, assetbundle.bundle_name);
                            else 
                            {
                                EditorUtility.ClearProgressBar();

                                StringBuilder builder = new StringBuilder();
                                builder.Append("文件:").Append(assetpath).AppendLine("同时存在多个AssetBundle中!");

                                foreach (var item in assetBundles)
                                {
                                    bool contain = item.IsContainFile(AssetDatabase.AssetPathToGUID(assetpath));
                                    if (!contain) 
                                        continue;
                                    builder.AppendLine(item.bundle_name);
                                } 

                                throw new System.Exception(builder.ToString());
                            }
                        }
                    }
                     
                }

                return asset_bundle_name_mapping;
            }
        }

        private Dictionary<string, string> asset_bundle_name_mapping_with_type = null;

        private Dictionary<string, string> asset_name_type_mapping_with_asset_path = null;

        /// <summary>
        /// assetname_type 与 bundle_name 的映射
        /// </summary>
        public Dictionary<string, string> AssetBundleNameMappingWithType
        {
            get { 
                if (asset_bundle_name_mapping_with_type == null || asset_bundle_name_mapping_with_type.Count == 0)
                {
                    if(asset_bundle_name_mapping_with_type == null)
                        asset_bundle_name_mapping_with_type = new Dictionary<string, string>();

                    if (asset_name_type_mapping_with_asset_path == null)
                        asset_name_type_mapping_with_asset_path = new Dictionary<string, string>();

                    asset_name_type_mapping_with_asset_path.Clear();

                    foreach (var asset_path in AssetBundleNameMapping.Keys)
                    { 
                        //Debug.LogFormat("asset:{0} exist:{1}",asset_path, System.IO.File.Exists(asset_path)); 
                        if (string.IsNullOrEmpty(asset_path) || !File.Exists(asset_path)) continue;
                        //if (!System.IO.File.Exists(asset_path)) continue;
                        string asset_name_with_type = AssetBundleTools.GetAssetNameWithType(asset_path);
                        if (!asset_bundle_name_mapping_with_type.ContainsKey(asset_name_with_type))
                        {

                            asset_bundle_name_mapping_with_type.Add(asset_name_with_type, AssetBundleNameMapping[asset_path]);

                            asset_name_type_mapping_with_asset_path.Add(asset_name_with_type, asset_path);

                            // 判断类型
                            if (AssetBundleTools.GetAssetType(asset_path) == typeof(Sprite))
                            {
                                // 如果是sprite类型 有可能有多张  都要加进来 
                                string assetName = Path.GetFileNameWithoutExtension(asset_path);

                                asset_name_with_type = AssetBundleTools.GetAssetNameWithType(assetName, typeof(Texture2D));
                                if (!asset_bundle_name_mapping_with_type.ContainsKey(asset_name_with_type))
                                    asset_bundle_name_mapping_with_type.Add(asset_name_with_type, AssetBundleNameMapping[asset_path]);

                            }
                        }
                        else
                        {
                            StringBuilder message = new StringBuilder();
                            message.Append("发现同名文件:").AppendLine().Append(asset_path).AppendLine();
                            if(asset_name_type_mapping_with_asset_path.ContainsKey(asset_name_with_type))
                                message.AppendLine(asset_name_type_mapping_with_asset_path[asset_name_with_type]); 
                            message.AppendLine("请保证资源名称唯一,否则可能会导致资源加载异常!");

                            throw new System.Exception(message.ToString());
                        }
                    }
                }

                return asset_bundle_name_mapping_with_type;
            }

        }
 
        #region AssetBundleMapping 编辑器加载时使用 不是最终的

        private Dictionary<string, string> asset_bundle_name_mapping_editor_load = null; // 文件路径 和 所在AssetBundle的映射 编辑器加载时用的 不是最终的assetbundle 

        /// <summary>
        /// asset_path 和 bundleName 的映射
        /// </summary>
        public Dictionary<string, string> AssetBundleNameMappingEditorLoad
        {
            get
            {
                if (asset_bundle_name_mapping_editor_load == null || asset_bundle_name_mapping_editor_load.Count == 0)
                {
                    asset_bundle_name_mapping_editor_load = new Dictionary<string, string>();
                    // 把所有的文件全都加进来
                    foreach (var assetbundle in assetBundles)
                    {
                        foreach (var assetpath in assetbundle.GetAllAssetPaths())
                        {
                            if (!asset_bundle_name_mapping_editor_load.ContainsKey(assetpath))
                                asset_bundle_name_mapping_editor_load.Add(assetpath, assetbundle.bundle_name);
                            else
                                Debug.LogErrorFormat("文件:{0}/{1} 已经存在bundle: {2} 中!", assetbundle.bundle_name, assetpath, asset_bundle_name_mapping_editor_load[assetpath]);
                        }
                    }

                }

                return asset_bundle_name_mapping_editor_load;
            }
        }

        private Dictionary<string, string> asset_bundle_name_mapping_with_type_editor_load = null;

        // assetname_type 与 bundle_name 的映射
        public Dictionary<string, string> AssetBundleNameMappingWithTypeEditorLoad
        {
            get
            {
                if (asset_bundle_name_mapping_with_type_editor_load == null || asset_bundle_name_mapping_with_type_editor_load.Count == 0)
                {
                    asset_bundle_name_mapping_with_type_editor_load = new Dictionary<string, string>();
                    foreach (var asset_path in AssetBundleNameMappingEditorLoad.Keys)
                    {
                        //Debug.LogFormat("asset:{0} exist:{1}",asset_path, System.IO.File.Exists(asset_path)); 
                        if (string.IsNullOrEmpty(asset_path) || !File.Exists(asset_path)) continue;
                        //if (!System.IO.File.Exists(asset_path)) continue;
                        string asset_name_with_type = AssetBundleTools.GetAssetNameWithType(asset_path);
                        if (!asset_bundle_name_mapping_with_type_editor_load.ContainsKey(asset_name_with_type))
                        {

                            asset_bundle_name_mapping_with_type_editor_load.Add(asset_name_with_type, AssetBundleNameMappingEditorLoad[asset_path]);

                            // 判断类型
                            if (AssetBundleTools.GetAssetType(asset_path) == typeof(Sprite))
                            {
                                // 如果是sprite类型 有可能有多张  都要加进来
                                string assetName = Path.GetFileNameWithoutExtension(asset_path);

                                asset_name_with_type = AssetBundleTools.GetAssetNameWithType(assetName, typeof(Texture2D));
                                if (!asset_bundle_name_mapping_with_type_editor_load.ContainsKey(asset_name_with_type))
                                    asset_bundle_name_mapping_with_type_editor_load.Add(asset_name_with_type, AssetBundleNameMappingEditorLoad[asset_path]);
                            }
                        }
                        else
                            throw new System.Exception(string.Format("发现同名文件:{0} bundleName:{1} {2}", asset_path, AssetBundleNameMappingEditorLoad[asset_path], asset_bundle_name_mapping_with_type_editor_load[asset_name_with_type]));
                    }
                }

                return asset_bundle_name_mapping_with_type_editor_load;
            }

        }
        #endregion

        /// <summary>
        /// 密钥
        /// </summary>
        [HideInInspector]
        public string secret_key = string.Empty;



        public XFABProject()
        {
            assetBundles = new List<XFABAssetBundle>();
            dependenceProject = new List<string>();
            InitBuildOptions();
        }

        private void InitBuildOptions() {

            buildAssetBundleOptions = new List<BuildOptionToggleData>();
            buildAssetBundleOptions.Add(new BuildOptionToggleData("None", "不使用任何特殊选项构建 assetBundle。", true, BuildAssetBundleOptions.None));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("UncompressedAssetBundle", "创建资源包时不压缩数据。", false, BuildAssetBundleOptions.UncompressedAssetBundle));
            //buildAssetBundleOption.Add(new ToggleData("CollectDependencies", "", true, BuildAssetBundleOptions.CollectDependencies));
            //buildAssetBundleOption.Add(new ToggleData("CompleteAssets", "", true, BuildAssetBundleOptions.CompleteAssets));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("DisableWriteTypeTree", "不包括 AssetBundle 中的类型信息。", false, BuildAssetBundleOptions.DisableWriteTypeTree));
#if !UNITY_2021_1_OR_NEWER
            buildAssetBundleOptions.Add(new BuildOptionToggleData("DeterministicAssetBundle", "使用存储在资源包中对象的 ID 的哈希构建资源包。", false, BuildAssetBundleOptions.DeterministicAssetBundle));
#endif
            buildAssetBundleOptions.Add(new BuildOptionToggleData("ForceRebuildAssetBundle", "强制重新构建 assetBundle。", false, BuildAssetBundleOptions.ForceRebuildAssetBundle));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("IgnoreTypeTreeChanges", "在执行增量构建检查时忽略类型树更改。", false, BuildAssetBundleOptions.IgnoreTypeTreeChanges));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("AppendHashToAssetBundleName", "向 assetBundle 名称附加哈希。", false, BuildAssetBundleOptions.AppendHashToAssetBundleName));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("ChunkBasedCompression", "创建 AssetBundle 时使用基于语块的 LZ4 压缩。", false, BuildAssetBundleOptions.ChunkBasedCompression));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("StrictMode", "如果在此期间报告任何错误，则构建无法成功。", false, BuildAssetBundleOptions.StrictMode));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("DryRunBuild", "进行干运行构建。", false, BuildAssetBundleOptions.DryRunBuild));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("DisableLoadAssetByFileName", "禁用按照文件名称查找资源包 LoadAsset。", false, BuildAssetBundleOptions.DisableLoadAssetByFileName));
            buildAssetBundleOptions.Add(new BuildOptionToggleData("DisableLoadAssetByFileNameWithExtension", "禁用按照带扩展名的文件名称查找资源包 LoadAsset。", false, BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension));

#if UNITY_2019_1_OR_NEWER
            buildAssetBundleOptions.Add(new BuildOptionToggleData("AssetBundleStripUnityVersion", "在构建过程中删除存档文件和序列化文件头中的 Unity 版本号。", false, BuildAssetBundleOptions.AssetBundleStripUnityVersion));
#endif
        }

        // 是否依赖于某个项目
        public bool IsDependenceProject(string name  ) {


            // 当前这个项目是否依赖这个项目 
            if (dependenceProject.Contains(name)) {
                return true;
            }

            // 当前这个项目 依赖的项目 是否依赖这个项目 
            for (int i = 0; i < dependenceProject.Count; i++)
            {
                XFABProject project = XFABProjectManager.Instance.GetProject(dependenceProject[i]);
                if (project != null &&  project.IsDependenceProject(name)) {
                    return true;
                }
            }
            return false;
        }

        public void Save() {
            EditorUtility.SetDirty(this);
        }

        public bool AddAssetBundle(XFABAssetBundle bundle ) {

            // 判断名称是否重复 
            if ( IsContainAssetBundleName( bundle.bundle_name ) ) {
                Debug.LogError("添加失败! AssetBundle 名称重复!");
                return false;
            }

            // 判断当前这个AssetBundle中的文件是否再其他的AssetBundle中存在 如果存在需要移除
            List<FileInfo> files = bundle.GetFileInfos();
            for (int i = 0; i < files.Count; i++)
            {
                for (int j = 0; j < assetBundles.Count; j++)
                {
                    if (assetBundles[j].IsContainFile(files[i].guid)) {
                        assetBundles[j].RemoveFile(files[i].guid);
                    }
                }
            }


            assetBundles.Add(bundle);
            _bundles = null; // 清空assetbundle的缓存
            Save();

            return true;

        }

        public bool IsContainAssetBundleName(string name) {


            for (int i = 0; i < assetBundles.Count; i++)
            {
                if ( name.Equals( assetBundles[i].bundle_name) ) {
                    return true;
                }
            }

            return false;

        }

        // 判断这个Project 是不是包含某个文件
        public bool IsContainFile(string asset_path) {
            return AssetBundleNameMapping.ContainsKey(asset_path);
        }

        public bool RenameAssetBundle(string newName, string originName)
        {
            XFABAssetBundle assetBundle = GetAssetBundle(originName);

            if (IsContainAssetBundleName(newName))
            {

                Debug.LogError(" 新名称已经存在!不能重复! ");
                return false;
            }

            if (assetBundle != null)
            {
                assetBundle.bundle_name = newName;
                _bundles = null; // 清空assetbundle的缓存
                if (assetBundle.bundleType == XFBundleType.Group)
                {
                    XFABAssetBundle[] bundles = GetAssetBundlesFromGroup(originName);
                    if (bundles != null)
                    {
                        foreach (var item in bundles)
                        {
                            item.group_name = newName;
                        }
                    }
                } 
                Save();
            }
            return true;
        }

        public XFABAssetBundle GetAssetBundle(string bundle_name) {

            if ( string.IsNullOrEmpty(bundle_name) ) {

                //Debug.Log( string.Format( " bundle_name:{0} 异常! ",bundle_name ) );
                return null;
            }

            if(Bundles.ContainsKey(bundle_name))
                return Bundles[bundle_name];

            return null;
        }

        public XFABAssetBundle GetAssetBundle(int nameHashCode) {

            for (int i = 0; i < assetBundles.Count; i++)
            {
                if (nameHashCode == assetBundles[i].bundle_name.GetHashCode())
                {
                    return assetBundles[i];
                }
            }
            return null;
        }


        // 获取某个组的所有AssetBundle
        public XFABAssetBundle[] GetAssetBundlesFromGroup(string group_name) {

            if (string.IsNullOrEmpty(group_name)) return null;

            List<XFABAssetBundle> bundles = new List<XFABAssetBundle>();
            for (int i = 0; i < assetBundles.Count; i++)
            {
                if ( group_name.Equals( assetBundles[i].group_name)) 
                    bundles.Add(assetBundles[i]); 
            }
            return bundles.ToArray();
        }

        public void RemoveAssetBundle(XFABAssetBundle bundle) {

            assetBundles.Remove(bundle);
            _bundles = null; // 清空assetbundle的缓存
        }

        // 删除Bundle
        public void RemoveAssetBundle(string bundle_name) {

            if ( IsContainAssetBundleName(bundle_name) )
            {
                RemoveAssetBundle( GetAssetBundle(bundle_name) );
                Save();
            }

        }

        public void RemoveAssetBundle(int nameHashCode)
        {

            XFABAssetBundle bundle = GetAssetBundle(nameHashCode);
            if (bundle != null)
            {
                RemoveAssetBundle(bundle);
            }

            if (bundle.bundleType == XFBundleType.Group)
            {
                XFABAssetBundle[] bundles = GetAssetBundlesFromGroup(bundle.bundle_name);
                if (bundles != null)
                { 
                    foreach (XFABAssetBundle item in bundles)
                    {
                        RemoveAssetBundle(item);
                    }
                }
            }

            Save();
        }

        // 获取Bundle 通过 它包含的文件
        public XFABAssetBundle GetBundleByContainFile(string asset_path) {
            for (int i = 0; i < assetBundles.Count; i++)
            {
                if (assetBundles[i].IsContainFile( AssetDatabase.AssetPathToGUID( asset_path ) )) {
                    return assetBundles[i];
                }
            }

            return null;
        }

        // 获取依赖的AssetBundle
        public List<XFABAssetBundle> GetDependenciesBundles(XFABAssetBundle bundle) {

            List<XFABAssetBundle> bundles = new List<XFABAssetBundle>();

            // 获取bundle中所有的文件 
            string[] asset_paths = AssetDatabase.GetDependencies(bundle.GetFilePaths(), true);

            // 遍历所有的AssetBundle
            for (int i = 0; i < assetBundles.Count; i++)
            {
                if (assetBundles[i] != bundle) {

                    for (int j = 0; j < asset_paths.Length; j++)
                    {
                        if (assetBundles[i].IsContainFile(AssetDatabase.AssetPathToGUID(asset_paths[j]))) {
                            bundles.Add(assetBundles[i]);
                            break;
                        }
                    }
                
                }
            }

            return bundles;

        }


        /// <summary>
        /// 获取依赖的所有项目
        /// </summary>
        /// <returns></returns>
        public string[] GetAllDependencies() {

            List<string> dependence = new List<string>();
            dependence.AddRange(dependenceProject);

            for (int i = 0; i < dependenceProject.Count; i++)
            {
                XFABProject project = XFABProjectManager.Instance.GetProject(dependenceProject[i]);
                if (project==null) { continue; }
                dependence.AddRange(project.GetAllDependencies());
            }

            return dependence.ToArray();
        }


        public void RefreshFiles() {
            foreach (var item in assetBundles)
            {
                item.UpdateFileInfos();
            }
        }


        /// <summary>
        /// 查询所有AssetBundle
        /// </summary>
        /// <returns></returns>
        public List<XFABAssetBundle> GetAllAssetBundles() {

            List<XFABAssetBundle> bundles = new List<XFABAssetBundle>();

            foreach (var item in assetBundles)
            {
                 
                switch (item.bundlePackgeType)
                {
                    case XFBundlePackgeType.One:
                        bundles.Add(item);
                        break;
                    case XFBundlePackgeType.Multiple:
                        // 这种模式是当前bundle中的每一个资源都单独打包
                        string[] files = item.GetAllAssetPaths();
                        foreach (var file in files)
                        {
                            // 过滤掉文件夹
                            if (AssetDatabase.IsValidFolder(file))
                                continue;

                            XFABAssetBundle bundle = new XFABAssetBundle(item.projectName);
                            bundle.bundle_name = XFABTools.md5(System.IO.Path.GetFileName(file)); // 把文件名转成 md5 
                            // 这里的AssetBundle是为了打包临时构建的,直接把文件加入就可以了
                            // 不需要判断文件是否已经存在别的XFABAssetBundle中
                            bundle.AddFile(file,true);
                            bundles.Add(bundle);
                        }
                        break;
                }
            }

            return bundles;
        }

    }

}

#endif