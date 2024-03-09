using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic; 
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine; 

namespace XFABManager
{
    /// <summary>
    /// 资源模块打包工具类
    /// </summary>
    public class ProjectBuild
    {

        private static List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();

        /// <summary>
        /// 打包某一个模块的AssetBundle(仅可在Editor模式下调用)
        /// </summary>
        /// <param name="project">资源模块</param>
        /// <param name="buildTarget">目标平台</param>
        /// <param name="revealInFinder">打包完成后是否打开AssetBundle所在的文件夹</param>
        public static void Build(XFABProject project, BuildTarget buildTarget,bool revealInFinder = true) {

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            // 打包之前
            OnBuildBefore(project, buildTarget);

            // 刷新资源
            AssetDatabase.Refresh();
            // 刷新文件列表 主要清除一些不存在的文件
            project.RefreshFiles();  
            // 对 project 内容做一个保存
            project.Save();

            List<XFABAssetBundle> allAssetBundle = project.GetAllAssetBundles();

            Dictionary<string, List<string>> dependent_bundles = ComputingDependentResources(project,allAssetBundle);
  
            BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;
            for (int i = 0; i < project.buildAssetBundleOptions.Count; i++)
            {
                if (project.buildAssetBundleOptions[i].isOn)
                {
                    buildAssetBundleOptions |= project.buildAssetBundleOptions[i].option;
                }
            }

            bundles.Clear();
            // 添加bundle
            for (int i = 0; i < allAssetBundle.Count; i++)
            {
                if (allAssetBundle[i].bundleType == XFBundleType.Group) continue;
                 
                string[] asset_paths = allAssetBundle[i].GetAllAssetPaths();
                if (asset_paths.Length > 0)
                {
                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = string.Format("{0}_{1}{2}", project.name.ToLower(), allAssetBundle[i].bundle_name,project.suffix);
                    build.assetNames = asset_paths;
                    bundles.Add(build);
                }
            }
            // 添加依赖bundle
            foreach (var bundleName in dependent_bundles.Keys)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = string.Format("{0}{1}", bundleName, project.suffix);
                build.assetNames = dependent_bundles[bundleName].ToArray();
                bundles.Add(build);
            }

            if (bundles.Count == 0)
            {
                Debug.LogErrorFormat(" 项目: {0} 文件列表为空,请添加需要打包的文件!",project.name);
                return;
            }

             
            string out_path = project.temp_out_path(buildTarget);
            // 如果目录存在 并且清空的话 就删除输出目录
            if (project.isClearFolders)
            {
                DeleteFolders(out_path);
            }
            
            // 清空数据目录
            DeleteFolders(XFABTools.DataPath(project.name));

            // 构建映射
            BuildAssetBundleMapping(project, buildTarget);

            // 保存后缀信息
            SaveSuffix(project, buildTarget);

            // 如果输出目录不存在 创建目录
            if (!Directory.Exists(out_path))
            {
                Directory.CreateDirectory(out_path);
            }

            // 记录ab包列表
            SaveBundleListToDisk(out_path);

            if (project.isAutoPackAtlas) {
                PackAtlas(project,allAssetBundle, dependent_bundles);
            }

            // 在打包之前先对目录已有的ab包做一个清理
            CleanOutputPath(bundles, out_path,project.suffix);

            var buildManifest = BuildPipeline.BuildAssetBundles(out_path, bundles.ToArray(), buildAssetBundleOptions,buildTarget);

            if (buildManifest == null)
            {
                Debug.LogError("Error in build");
                return;
            }
            AssetDatabase.Refresh();

            ProjectBuildInfo buildInfo = new ProjectBuildInfo();
            buildInfo.bundleInfos = BuildBundlesInfo(project,buildTarget);
            buildInfo.dependenceProject = project.GetAllDependencies();
            buildInfo.displayName = project.displayName;
            buildInfo.projectName = project.name;
            buildInfo.suffix = project.suffix;
            buildInfo.update_message = project.update_message;
            buildInfo.version = project.version;
            buildInfo.update_date = DateTime.Now.ToString("yyyy/MM/dd");

            string project_build_info = string.Format("{0}/{1}", out_path, XFABConst.project_build_info);
            if (!File.Exists(project_build_info)) File.Create(project_build_info).Close();
             
            File.WriteAllText(project_build_info, JsonConvert.SerializeObject(buildInfo));


            // 判断是否需要加密
            if (!string.IsNullOrEmpty(project.secret_key))
            {
                Debug.Log("需要加密:" + project.secret_key);
                 
                string des_path = string.Format("{0}Secret", out_path);

                FileTools.CopyDirectory(out_path, des_path, null);
                 
                string project_build_info_path = des_path + "/project_build_info.json";
                 
                ProjectBuildInfo build_info_des = JsonConvert.DeserializeObject<ProjectBuildInfo>(File.ReadAllText(project_build_info_path));

                string[] files = Directory.GetFiles(des_path, string.Format("*{0}", project.suffix));

                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    byte[] datas = EncryptTools.Encrypt(File.ReadAllBytes(file), project.secret_key);
                    File.WriteAllBytes(file, datas);
                    EditorUtility.DisplayProgressBar("加密文件", string.Format("正在加密:{0}", file), ((float)i + 1) / files.Length);

                    string bundleName = System.IO.Path.GetFileName(file);
                    long length = new System.IO.FileInfo(file).Length;
                    string md5 = XFABTools.md5file(file);
                    build_info_des.UpdateBundleFile(bundleName, length, md5);

                }
                EditorUtility.ClearProgressBar();
                
                File.WriteAllText(project_build_info_path, JsonConvert.SerializeObject(build_info_des));

                // 把临时目录的资源复制到正式目录
                CopyTempAsset(des_path, project.assetbundle_out_path(buildTarget));
            }
            else {
                // 把临时目录的资源复制到正式目录
                CopyTempAsset(out_path, project.assetbundle_out_path(buildTarget));
            }

            if (project.isCompressedIntoZip)
            {
                CompressedIntoZip(project,buildTarget);
            }
             
            if (project.isCopyToStreamingAssets)
            {
                CopyToStreamingAssets(project, buildTarget);
            }

            timer.Stop();
            Debug.Log(string.Format("打包完成! module:{0} target:{1} version:{2} 耗时:{3}s", project.name, buildTarget, project.version, timer.ElapsedMilliseconds / 1000.0f));

            EditorApplication.delayCall += ()=> {
                OnBuildComplete(project, buildTarget); // 打包之后
            };

            if(revealInFinder) EditorUtility.RevealInFinder(project.assetbundle_out_path(buildTarget));
        }

        private static void SaveBundleListToDisk(string out_path) {
            string path = string.Format("{0}/bundle_list.txt",out_path);
            StringBuilder builder = new StringBuilder();
            foreach (var item in bundles)
            {
                builder.AppendLine(item.assetBundleName);
                foreach (var asset in item.assetNames)
                {
                    builder.Append("  ").AppendLine(asset);
                }
            }

            File.WriteAllText(path,builder.ToString());
        }

        // 清理输出目录
        private static void CleanOutputPath(List<AssetBundleBuild> bundles,string output_path,string suffix) {

            if (!System.IO.Directory.Exists(output_path)) return;

            // 读取到输出目录下面的所有资源
            string[] files = System.IO.Directory.GetFiles(output_path,"*", SearchOption.AllDirectories);
            foreach (var item in files)
            {

                if (item.EndsWith(suffix) ) {
                      
                    string fileName = System.IO.Path.GetFileName(item);
                    bool delete = true;
                    foreach (var bundle in bundles)
                    {
                        if (fileName.Equals(bundle.assetBundleName)) {
                            delete = false;
                            break;
                        }
                    }

                    if (delete) { 
                        if(File.Exists(item))
                            File.Delete(item);
                        if(File.Exists(string.Format("{0}.manifest", item)))
                            File.Delete(string.Format("{0}.manifest", item));
                    }
                }

            }
            // 判断输出目录下的资源是否在列表中 如果不在则删除
            
        }

        private static void BuildAssetBundleMapping(XFABProject project, BuildTarget buildTarget) {

            project.AssetBundleNameMapping.Clear();  // 先清空资源 
            project.AssetBundleNameMappingWithType.Clear();
            
            StringBuilder builder = new StringBuilder();

            foreach (var item in project.AssetBundleNameMappingWithType.Keys)
            {
                builder.Append(item).Append("|").Append(project.AssetBundleNameMappingWithType[item]).Append("\n");
            }
            // 如果输出目录不存在 创建目录
            string out_path = project.temp_out_path(buildTarget);
            if (!Directory.Exists(out_path))
            {
                Directory.CreateDirectory(out_path);
            }
            string path = string.Format("{0}/{1}", out_path, XFABConst.asset_bundle_mapping);
            File.WriteAllText(path, builder.ToString());

            //Debug.Log("映射构建成功!"+path);
        }

        private static void SaveSuffix(XFABProject project, BuildTarget buildTarget) {
            string out_path = project.temp_out_path(buildTarget);
            if (!Directory.Exists(out_path))
            {
                Directory.CreateDirectory(out_path);
            }
            string path = string.Format("{0}/{1}", out_path, XFABConst.bundles_suffix_info);
            File.WriteAllText(path, project.suffix);
        }

        // 删除文件夹
        private static bool DeleteFolders(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (System.IO.IOException)
                {
                    Debug.LogError( string.Format("操作无法完成\n{0}\n因为其中的文件或文件夹 已在另一个程序中打开!\n请关闭后重试!", path));
                    return false;
                }
            }
            return true;
        }

        // 构建Bundle文件列表
        private static BundleInfo[] BuildBundlesInfo(XFABProject project,BuildTarget buildTarget)
        {
            //string file_list = string.Format("{0}/{1}", output_path, XFABConst.files);
            //if (File.Exists(file_list)) File.Delete(file_list);

            bundles.Add(new AssetBundleBuild() { assetBundleName = buildTarget.ToString() });
            bundles.Add(new AssetBundleBuild() { assetBundleName = "AssetBundleNameMapping.txt" }); // 文件名称与bundle的映射
            bundles.Add(new AssetBundleBuild() { assetBundleName = XFABConst.bundles_suffix_info }); // 文件名称与bundle的映射
            //List<BundleInfo> bundleInfos = new List<BundleInfo>( bundles.Count );

            BundleInfo[] bundleInfos = new BundleInfo[bundles.Count];
            // 构建bundle的
            for (int i = 0; i < bundles.Count; i++)
            {
                string file = string.Format("{0}/{1}", project.temp_out_path(buildTarget), bundles[i].assetBundleName);
                if (!File.Exists(file)) continue;
                bundleInfos[i] = BuildBundleFileInfo(file, bundles[i].assetBundleName);
            }

            return bundleInfos;
        }

        private static BundleInfo BuildBundleFileInfo(string filePath, string fileName)
        {

            string md5 = XFABTools.md5file(filePath);
            long length = 0;
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo.Exists)
                length = fileInfo.Length;
            else
            {
                Debug.LogErrorFormat("文件:{0} 不存在!", filePath);
            }
            return new BundleInfo(fileName, length, md5);
        }

        /// <summary>
        /// 把打包完成的AssetBundle文件复制到StreamingAssets目录(仅可在Editor模式下调用)
        /// </summary>
        /// <param name="project"></param>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static bool CopyToStreamingAssets( XFABProject project,BuildTarget buildTarget )
        {

            string targetPath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, project.name, buildTarget.ToString());

            // 清空目标文件夹的内容
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }

            Directory.CreateDirectory(targetPath); 
              
            bool isSuccess = false;

     
            isSuccess = FileTools.CopyDirectory(project.assetbundle_out_path(buildTarget), targetPath, (fileName, progress) =>
            {
                EditorUtility.DisplayProgressBar("复制到StreamingAssets", string.Format("正在复制文件:{0}", fileName), progress);
            }, new string[] { ".zip" });
            EditorUtility.ClearProgressBar();
            //}
            AssetDatabase.Refresh();

            return isSuccess;
        }

        /// <summary>
        /// 把打包完成的AssetBundle文件压缩成zip(仅可在Editor模式下调用)
        /// </summary>
        /// <param name="project"></param>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static bool CompressedIntoZip(XFABProject project,BuildTarget buildTarget)
        {
            string zipPath = string.Format("{0}/{1}.zip", project.assetbundle_out_path(buildTarget), project.name);
            string zip_md5_path = string.Format("{0}/{1}_md5.txt", project.assetbundle_out_path(buildTarget), project.name);

            bool zip = ZipTools.CreateZipFile(project.assetbundle_out_path(buildTarget), zipPath);
            if (zip)
            {
                // 压缩成功计算压缩文件md5
                string md5 = XFABTools.md5file(zipPath);
                File.WriteAllText(zip_md5_path, md5);
            }
            else { 
                File.Delete(zipPath);
            }
            return zip;
        }

        private static void CopyTempAsset(string temp_path,string final_path) 
        {

            // 清空目标文件夹的内容
            if (Directory.Exists(final_path))
            {
                Directory.Delete(final_path, true);
            }

            Directory.CreateDirectory(final_path);
            
            FileTools.CopyDirectory(temp_path, final_path, (fileName, progress) =>
            {
                EditorUtility.DisplayProgressBar("移动文件到目标目录", string.Format("正在复制文件:{0}", fileName), progress);
            }, new string[] { ".zip" });
            EditorUtility.ClearProgressBar(); 
            AssetDatabase.Refresh(); 
        }

        private static void OnBuildBefore(XFABProject project, BuildTarget buildTarget) {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            { 
                foreach (var type in item.GetTypes())
                {
                    foreach (var t in type.GetInterfaces())
                    {
                        if (t == typeof(IPreprocessBuildProject))
                        {
                            IPreprocessBuildProject onBuild = Activator.CreateInstance(type) as IPreprocessBuildProject;
                            onBuild.OnPreprocess(project.name, project.assetbundle_out_path(buildTarget), buildTarget);
                        }
                    }
                }
            }
        }
        
        // 
        private static void OnBuildComplete(XFABProject project,BuildTarget buildTarget) {

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            {
                foreach (var type in item.GetTypes())
                {
                    foreach (var t in type.GetInterfaces())
                    {
                        if (t == typeof(IPostprocessBuildProject))
                        {
                            IPostprocessBuildProject onBuild = Activator.CreateInstance(type) as IPostprocessBuildProject;
                            onBuild.OnPostprocess(project.name, project.assetbundle_out_path(buildTarget), buildTarget);
                        }
                    }
                }
            }

        }



        // 计算依赖的资源
        private static Dictionary<string, List<string>> ComputingDependentResources(XFABProject project, List<XFABAssetBundle> allAssetBundle) {

            // 收集之前先清空缓存
            project.AssetBundleNameMapping.Clear();

            Dictionary<string, List<string>> needOptimizeFiles = new Dictionary<string, List<string>>();
            // 收集依赖资源
            for (int i = 0; i < allAssetBundle.Count; i++)
            {
                XFABAssetBundle assetBundle = allAssetBundle[i];

                 
                for (int j = 0; j < assetBundle.files.Count; j++)
                {
                    FileInfo fileInfo = assetBundle.files[j];

                    //fileInfo.Files

                    foreach (var assetPath in fileInfo.Files)
                    {
                        // 获取到这个文件 依赖的所有文件
                        string[] dependens = AssetDatabase.GetDependencies(assetPath, true);
                        if (dependens.Length == 0) continue;

                        foreach (string dependenc in dependens)
                        {
                            if (dependenc.Equals(assetPath)) continue;

                            // 前提是这个文件没有被打进 AssetBundle 如果已经打进AssetBundle ，就会形成依赖 不会产生资源冗余
                            // 如果不是有效的资源文件 也不需要往后判断
                            if (!AssetBundleTools.IsValidAssetBundleFile(dependenc) || project.IsContainFile(dependenc)) continue;

                            if (needOptimizeFiles.ContainsKey(dependenc))
                            {
                                // 把 bundle_name 加进去
                                if (!needOptimizeFiles[dependenc].Contains(assetBundle.bundle_name))
                                    needOptimizeFiles[dependenc].Add(assetBundle.bundle_name);
                            }
                            else
                                needOptimizeFiles.Add(dependenc, new List<string>() { assetBundle.bundle_name });



                        }
                    }

                    

                    EditorUtility.DisplayProgressBar("计算依赖资源", "收集依赖数据", (float)i / allAssetBundle.Count + (float)j / assetBundle.files.Count / allAssetBundle.Count);
                }
            }

            EditorUtility.ClearProgressBar();

            // 把收集到的所有的bundle排个序
            foreach (var item in needOptimizeFiles.Values)
            {
                item.Sort();
            }

            Dictionary<string, List<string>> calculate_bundles = new Dictionary<string, List<string>>(); // 计算出来的bundle(非手动添加的)

            // 单独打包
            List<string> separately = new List<string>();

            // 判断文件是否需要单独打包
            foreach (string key in needOptimizeFiles.Keys)
            {
                if (AssetBundleTools.IsNeedsToBePackagedSeparately(key))
                {
                    separately.Add(key);
                }
            }

            foreach (var item in separately)
            {
                string bundleName = string.Format("{0}_separately(auto)", AssetDatabase.AssetPathToGUID(item));
                AddToDictionary(calculate_bundles, bundleName, item);

                string[] dependences = AssetDatabase.GetDependencies(item, true);
                string bundleNameRes = string.Format("{0}_separately_depend_res(auto)", AssetDatabase.AssetPathToGUID(item));

                List<string> dependencesList = new List<string>();

                foreach (var depend_item in dependences)
                {
                    if (!AssetBundleTools.IsValidAssetBundleFile(depend_item) || project.IsContainFile(depend_item)) continue;
                    if (depend_item.Equals(item)) continue;
                    if (!needOptimizeFiles.ContainsKey(depend_item)) continue; // 如果这个文件不在需要优化的文件列表中 说明已经被打到别的包中了 不需要重复添加了
                    if (separately.Contains(depend_item)) continue; // 如果依赖的这个资源 已经被划分为需要单独打包 就不需要添加了
                    dependencesList.Add(depend_item);
                }
                if (dependencesList.Count != 0)
                    AddToDictionary(calculate_bundles, bundleNameRes, dependencesList.ToArray());


                // 移除需要单独打包的文件
                if (needOptimizeFiles.ContainsKey(item))
                    needOptimizeFiles.Remove(item);

                // 移除需要单独打包文件的依赖文件
                foreach (string d in dependences)
                {
                    if (needOptimizeFiles.ContainsKey(d))
                        needOptimizeFiles.Remove(d);
                }
            }

            

            // 找到只被一个Bundle依赖的资源 单独放到一个包中(目的:比如一个预制体依赖的所有资源 单独打包 ，预制体单独打包，这样修改预制体时,只更新预制体对应的包就可以了！)
            List<string> remove_files = new List<string>();
            foreach (string key in needOptimizeFiles.Keys)
            {
                if (needOptimizeFiles[key].Count == 1)
                {
                    remove_files.Add(key);

                    string bundleName = string.Format("{0}_depend_on_res(auto)", needOptimizeFiles[key][0]);

                    AddToDictionary(calculate_bundles, bundleName, key);
                }
            }

            // 移除单独放到一个包中的资源
            foreach (var item in remove_files)
            {
                needOptimizeFiles.Remove(item);
            }

            

            Dictionary<string,List<string>> bundle_assets = new Dictionary<string, List<string>>();
            // 转换资源为 包 -> assets 
            foreach (var asset in needOptimizeFiles.Keys)
            {
                //needOptimizeFiles[asset] 
                string bundleName = XFABTools.md5(string.Join("", needOptimizeFiles[asset]));
                //Debug.LogFormat("needOptimizeFiles bundleName:{0} asset:{1}", bundleName,asset);
                AddToDictionary(bundle_assets, bundleName, asset);
            } 

            // 再把包里面的资源 按照类型进行划分
            foreach (var item in bundle_assets.Keys)
            {
                Dictionary<Type, List<string>> type_assets = new Dictionary<Type, List<string>>();

                foreach (var asset in bundle_assets[item]) {

                    Type type = AssetDatabase.GetMainAssetTypeAtPath(asset); 

                    if (type_assets.ContainsKey(type))
                        type_assets[type].Add(asset);
                    else
                        type_assets.Add(type,new List<string> {asset});
                }

                foreach (var asset in type_assets.Keys) {
                    string assetsName = GetAssetsName(type_assets[asset]);
                    //Debug.LogFormat("assetsName:{0}",assetsName);
                    AddToDictionary(calculate_bundles, assetsName, type_assets[asset].ToArray());
                }

            }

            // 计算完毕
            return calculate_bundles;
        }


        private static void AddToDictionary(Dictionary<string,List<string>> dic,string bundleName ,string asset) {
            if (dic.ContainsKey(bundleName))
                dic[bundleName].Add(asset);
            else
                dic.Add(bundleName, new List<string>() { asset });
        }

        private static void AddToDictionary(Dictionary<string, List<string>> dic, string bundleName, string[] assets)
        {
            if (dic.ContainsKey(bundleName))
                dic[bundleName].AddRange(assets);
            else { 
                List<string> list = new List<string>();
                dic.Add(bundleName, list);
                list.AddRange(assets);
            }
        }

        private static string GetAssetsName(List<string> assetPaths) { 
            List<string> guids = new List<string>(); 
            foreach (string assetPath in assetPaths) { 
                guids.Add(AssetDatabase.AssetPathToGUID(assetPath));
            }
            string guid = string.Join("", guids); 
            string name = XFABTools.md5(guid); 
            return name;
        }
 
        /// <summary>
        /// 打包某一个资源模块的图集
        /// </summary>
        /// <param name="project"></param>
        /// <param name="allAssetBundles"></param>
        /// <param name="caculate_bundles"></param>
        internal static void PackAtlas(XFABProject project,List<XFABAssetBundle> allAssetBundles,Dictionary<string,List<string>> caculate_bundles = null) {
            AssetDatabase.Refresh();

            string forlderName = "Atlas(AutoGenerate)";

            string atlasPath = string.Format("Assets/{0}/{1}",forlderName, project.name);

            if (AssetDatabase.IsValidFolder(atlasPath))
                AssetDatabase.DeleteAsset(atlasPath);

            if (!AssetDatabase.IsValidFolder(string.Format("Assets/{0}", forlderName))) 
                AssetDatabase.CreateFolder("Assets", forlderName);

            AssetDatabase.CreateFolder(string.Format("Assets/{0}", forlderName), project.name);

            AssetDatabase.Refresh();

            Dictionary<string,string[]> atlas = new Dictionary<string, string[]>();

            // 添加bundle
            for (int i = 0; i < allAssetBundles.Count; i++)
            {
                if (allAssetBundles[i].bundleType == XFBundleType.Group) continue;
                string[] asset_paths = allAssetBundles[i].GetAllAssetPaths();
                if (asset_paths.Length > 0)
                { 
                    string assetBundleName = string.Format("{0}_{1}{2}", project.name.ToLower(), allAssetBundles[i].bundle_name, project.suffix);
                    atlas.Add(assetBundleName, asset_paths);
                }
            }

            if (caculate_bundles == null) {
                caculate_bundles = ComputingDependentResources(project, allAssetBundles);
            }

            // 添加依赖bundle
            foreach (var bundleName in caculate_bundles.Keys)
            { 
                string assetBundleName = string.Format("{0}{1}", bundleName, project.suffix);
                atlas.Add(assetBundleName, caculate_bundles[bundleName].ToArray()); 
            }

            float index = 0;

            foreach (string bundleName in atlas.Keys)
            {
                string atlasName = string.Format("{0}/{1}_Atlas.spriteatlas", atlasPath, bundleName);
               
                TextureImporterPlatformSettings platformSetting = new TextureImporterPlatformSettings();
                platformSetting.maxTextureSize = 2048;
                platformSetting.format = TextureImporterFormat.Automatic;
                platformSetting.crunchedCompression = project.use_crunch_compression;

                switch (project.atlas_compression_type)
                {
                    case AtlasCompressionType.None:
                        platformSetting.textureCompression = TextureImporterCompression.Uncompressed;
                        break;
                    case AtlasCompressionType.LowQuality:
                        platformSetting.textureCompression = TextureImporterCompression.CompressedLQ;
                        break;
                    case AtlasCompressionType.NormalQuality:
                        platformSetting.textureCompression = TextureImporterCompression.Compressed;
                        break;
                    case AtlasCompressionType.HighQuality:
                        platformSetting.textureCompression = TextureImporterCompression.CompressedHQ;
                        break; 
                }

                
                platformSetting.compressionQuality = project.compressor_quality; 
                AtlasPacking.CreateAtlasByAssetPath(atlas[bundleName], atlasName, platformSetting);

       

                EditorUtility.DisplayProgressBar("正在打包图集",String.Format("打包:{0}...", bundleName), index / atlas.Count);
                index++;
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 打包某一个资源模块的图集(仅可在Editor模式下调用)
        /// </summary>
        /// <param name="project"></param>
        /// <param name="allAssetBundles"></param>
        /// <param name="caculate_bundles"></param>
        public static void PackAtlas(XFABProject project) {
            PackAtlas(project, project.GetAllAssetBundles());
        }

    }

}

