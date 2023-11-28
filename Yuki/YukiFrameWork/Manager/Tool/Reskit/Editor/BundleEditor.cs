using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace YukiFrameWork.Res.Editors
{
#if UNITY_EDITOR
    public class BundleEditor
    {
       //private const string ABConfigPath = "Assets/Editor/ABConfig.asset";
        private readonly static string BuildTargetPath = Application.streamingAssetsPath;       
        /// <summary>
        /// Key:AB包名,Value：路径
        /// </summary>
        private readonly static Dictionary<string, string> m_FileABDir = new Dictionary<string, string>();

        private static string dataPath;      

        /// <summary>
        /// 依赖项过滤
        /// </summary>
        private readonly static List<string> m_FileAB = new List<string>();

        private static bool loadFromAssetBundle = false;

        /// <summary>
        /// 储存所有的有效路径
        /// </summary>
        private readonly static List<string> configFiles = new List<string>();

        /// <summary>
        /// 单个Prefab的依赖项路径
        /// </summary>
        private readonly static Dictionary<string, List<string>> m_PrefabsDir = new Dictionary<string, List<string>>();
       
        public static void Build(string configPath,string configName,string dataPath,bool LoadFromAssetBundle)
        {
            loadFromAssetBundle = LoadFromAssetBundle;           
            BundleEditor.dataPath = dataPath;          
            configFiles.Clear();
            m_FileABDir.Clear();
            m_FileAB.Clear();
            m_PrefabsDir.Clear();
            ABConfig aBConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(configPath + @"\" + string.Format("{0}.asset", configName));

            if (!Directory.Exists(BuildTargetPath))
            {
                Directory.CreateDirectory(BuildTargetPath);
            }

            if (!Directory.Exists(dataPath + @"/ABDepend"))
            {
                Directory.CreateDirectory(dataPath + @"/ABDepend");
                AssetDatabase.Refresh();
            }

            AddFileABDirPath(aBConfig);
            AddAllPrefabPaths(aBConfig);
            Debug.Log("当前打包模式：" + (loadFromAssetBundle ? "真机模式,正常通过AssetBundle加载资源" : "编辑模式,在使用assetbundle加载之前可将通过编辑器进行加载"));
        }

        /// <summary>
        /// 添加配置AB包
        /// </summary>
        /// <param name="aBConfig">配置文件</param>
        private static void AddFileABDirPath(ABConfig aBConfig)
        {
            foreach (var path in aBConfig.ABPackageFiles)
            {
                if (m_FileABDir.ContainsKey(path.abName))
                {
                    Debug.LogWarning($"当前AB包配置名字重复！将不会再度配置！,包名为：{path.abName}");
                }
                else
                {
                    m_FileABDir.Add(path.abName, path.abPath);
                    m_FileAB.Add(path.abPath);
                    configFiles.Add(path.abPath);
                }
            }
        }

        /// <summary>
        /// 添加配置所有的Prefab
        /// </summary>
        /// <param name="aBConfig">配置文件</param>
        private static void AddAllPrefabPaths(ABConfig aBConfig)
        { 
            string[] allPaths = AssetDatabase.FindAssets("t:Prefab", aBConfig.AllPrefabPath.ToArray());

            for (int i = 0; i < allPaths.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(allPaths[i]);                
                EditorUtility.DisplayProgressBar("查找Prefab", "Prefab" + path, i * 1.0f / allPaths.Length);
                configFiles.Add(path);
                if (!ContainsFileAB(path))
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    //获取资源依赖项
                    string[] allDepend = AssetDatabase.GetDependencies(path);

                    List<string> dependPath = new List<string>();
                    for (int j = 0; j < allDepend.Length; j++)
                    {                      
                        ///如果过滤器里没有这个资源和不是脚本文件
                        if (!ContainsFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs"))
                        {
                            m_FileAB.Add(allDepend[j]);
                            dependPath.Add(allDepend[j]);
                        }                      
                    }
                    if (m_PrefabsDir.ContainsKey(obj.name))
                    {
                        Debug.LogWarning("存在相同名字的Prefab，Prefab名字： " + obj.name);
                    }
                    else
                    {
                        Debug.Log("添加Prefab成功，Prefab名称：" + obj.name);
                        m_PrefabsDir.Add(obj.name, dependPath);
                    }
                }                           
            }
            foreach (var name in m_FileABDir.Keys)
            {
                SetABName(name, m_FileABDir[name]);
            }

            foreach (var name in m_PrefabsDir.Keys)
            {
                SetABName(name, m_PrefabsDir[name]);
            }

            BuildAssetBundle();
            RemoveAllABName();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void BuildAssetBundle()
        {          
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();           
            ///key：全路径 value:包名
            Dictionary<string, string> resPathDic = new Dictionary<string, string>();                             

            for (int i = 0; i < allBundles.Length; i++)
            {             
                string[] allBundlesPath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);        
                for (int j = 0; j < allBundlesPath.Length; j++)
                {                  
                    if (allBundlesPath[j].EndsWith(".cs"))
                        continue;

                    Debug.Log("此AB包：" + allBundles[i] + "下面包含的资源文件路径为：" + allBundlesPath[j]);
                    if(VaildPath(allBundlesPath[j]))
                        resPathDic.Add(allBundlesPath[j], allBundles[i]);                   
                }
            }

            DeleteAssetBundle();
            ///生成配置表
            WriteData(resPathDic);           
            BuildPipeline.BuildAssetBundles(BuildTargetPath,
                BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);           

        }

        /// <summary>
        /// 写入配置表
        /// </summary>
        /// <param name="resPathDic">路径字典</param>
        private static void WriteData(Dictionary<string,string> resPathDic)
        {
            AssetBundleConfig bundleConfig = new AssetBundleConfig();
            bundleConfig.ABList = new List<AssetBundleBase>();
            bundleConfig.LoadFromAssetBundle = loadFromAssetBundle;
            foreach (var path in resPathDic.Keys)
            {
                AssetBundleBase bundleBase = new AssetBundleBase();
                bundleBase.Path = path;
                bundleBase.Crc = CRC32.GetCRC32(path);                
                bundleBase.ABName = resPathDic[path];
                bundleBase.AssetsName = path.Remove(0, path.LastIndexOf('/') + 1);
                bundleBase.ABDependce = new List<string>();
                //获取这个资源所有的依赖项
                string[] resDepence = AssetDatabase.GetDependencies(path);               
                for (int i = 0; i < resDepence.Length; i++)
                {
                    string tempPath = resDepence[i]; 
                    if (tempPath == path || tempPath.EndsWith(".cs"))
                        continue;                   
                    if (resPathDic.TryGetValue(tempPath, out var abName))
                    {
                        Debug.Log(abName + resPathDic[path]);
                        if (abName == resPathDic[path])
                            continue;

                        if (!bundleBase.ABDependce.Contains(abName))
                        {                          
                            bundleBase.ABDependce.Add(abName);
                        }
                    }
                }              
                bundleConfig.ABList.Add(bundleBase);               
            }

            //写入Xml
            string xmlPath = dataPath + @"\AssetBundleConfig.xml";
            if (File.Exists(xmlPath)) File.Delete(xmlPath);

            using FileStream fileStream = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

            using StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.UTF8);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AssetBundleConfig));
            xmlSerializer.Serialize(writer, bundleConfig);
            writer.Close();
            fileStream.Close();


            //写入二进制
            foreach (var abBase in bundleConfig.ABList)
            {
                abBase.Path = string.Empty;
            }
            string bytesPath = dataPath + @"\ABDepend"+  @"\AssetBundleConfig.bytes";
            if (!Directory.Exists(dataPath + @"\ABDepend"))
                Directory.CreateDirectory(dataPath + @"\ABDepend");
            using FileStream bytesFileStream = new FileStream(bytesPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            bytesFileStream.Seek(0, SeekOrigin.Begin);
            bytesFileStream.SetLength(0);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(bytesFileStream, bundleConfig);
            bytesFileStream.Close();
            AssetDatabase.Refresh();
            //防止编辑器误判而没有刷新这个ab包 
            SetABName("assetbundleconfig", bytesPath);
        }

        /// <summary>
        /// 删除重复的ab包
        /// </summary>
        private static void DeleteAssetBundle()
        {
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();

            DirectoryInfo directory = new DirectoryInfo(BuildTargetPath);
            FileInfo[] fileInfos = directory.GetFiles("*", SearchOption.AllDirectories);

            for (int i = 0; i < fileInfos.Length; i++)
            {
                if (ContainsABName(fileInfos[i].Name, allBundles) || fileInfos[i].Name.EndsWith(".meta") || fileInfos[i].Name.EndsWith(".manifest") || fileInfos[i].Name.EndsWith("assetbundleconfig"))
                    continue;
                else
                {
                    Debug.Log($"此AB包已经被删或者改名了，AB包名为：{fileInfos[i].Name}");
                    if (File.Exists(fileInfos[i].FullName))
                    {
                        File.Delete(fileInfos[i].FullName);
                    }
                }
            }
        }

        /// <summary>
        /// 删除所有ab包名
        /// </summary>
        private static void RemoveAllABName()
        {
            string[] oldabNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < oldabNames.Length; i++)
            {
                AssetDatabase.RemoveAssetBundleName(oldabNames[i], true);
                EditorUtility.DisplayProgressBar("清除AB包名", "ab包名称：" + oldabNames[i], i * 1.0f / oldabNames.Length);
            }

        }

        /// <summary>
        /// 自动设置所有的ab包名
        /// </summary>
        /// <param name="name">包名</param>
        /// <param name="path">路径</param>
        private static void SetABName(string name, string path)
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter == null)
            {
                Debug.LogError("不存在此路径的文件，路径为：" + path);
            }
            else
            {
                assetImporter.assetBundleName = name;
            }
        }

        private static void SetABName(string name, List<string> paths)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                SetABName(name, paths[i]);
            }
        }

        /// <summary>
        /// 遍历文件夹对里面所有的AB包进行判断检查
        /// </summary>
        /// <param name="name">包名</param>
        /// <param name="strs">路径</param>
        /// <returns></returns>
        private static bool ContainsABName(string name, string[] strs)
        {
            for (int i = 0; i < strs.Length; i++)
            {
                if (name == strs[i])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含在已经有的AB包里，用来做ab包冗余剔除
        /// </summary>
        /// <param name="path">地址</param>      
        private static bool ContainsFileAB(string path)
        {
            for (int i = 0; i < m_FileAB.Count; i++)
            {
                if (m_FileAB[i] == path || (path.Contains(m_FileAB[i]) && path.Replace(m_FileAB[i],"")[0] == '/'))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否是有效路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool VaildPath(string path)
        {
            for (int i = 0; i < configFiles.Count; i++)
            {
                if (path.Contains(configFiles[i]))
                    return true;
            }
            return false;
        }
    }
#endif
}