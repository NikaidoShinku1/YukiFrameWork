using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.XFABManager
{

    public class AssetBundleTools
    {
#if UNITY_EDITOR 
        /// <summary>
        /// 判断一个资源文件是否能够打包进AssetBundle(仅在编辑器模式可用)
        /// </summary>
        /// <param name="asset_path"></param>
        /// <returns></returns>
        public static bool IsValidAssetBundleFile(string asset_path)
        {

            if (asset_path.EndsWith( ".dll") || asset_path.EndsWith( ".cs") ||
                asset_path.EndsWith( ".meta") || asset_path.EndsWith(".js") ||
                asset_path.EndsWith( ".boo") || asset_path.EndsWith( ".jar") ||
                asset_path.EndsWith( ".mm") || asset_path.EndsWith( ".m") ||
                asset_path.EndsWith( ".asmdef") /*|| ext.Equals( ".asset")*/ ||
                /*asset_path.EndsWith("") ||*/ asset_path.EndsWith(".bat") || asset_path.EndsWith("shader")) 
            {
                return false;
            }
             
            if (asset_path.EndsWith(".asset")) {
                System.Type type = AssetDatabase.GetMainAssetTypeAtPath(asset_path);
                if (type.FullName.StartsWith("UnityEditor."))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 判断一个资源是不是需要单独打包 (暂时认为 .prefab .ttf .fontsettings .asset .TTF 结尾的文件单独打包 或者单个文件大小大于1MB)
        /// </summary>
        /// <param name="asset_path"></param>
        /// <returns></returns>
        public static bool IsNeedsToBePackagedSeparately(string asset_path) {

            if (new System.IO.FileInfo(asset_path).Length > 1048576 )
                return true;
            
            if (asset_path.EndsWith(".prefab") || asset_path.EndsWith(".ttf") ||
                asset_path.EndsWith(".fontsettings") || asset_path.EndsWith(".asset") ||
                asset_path.EndsWith(".TTF") || asset_path.EndsWith(".unity") || asset_path.EndsWith(".UNITY"))
            {
                return true;
            }

            return false;
        }
        internal static System.Type GetAssetType(string asset_path) { 
            
            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(asset_path);

            if (type == typeof(Texture2D))
            {
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(asset_path); // 获取文件 
                if (importer.textureType == TextureImporterType.Sprite)
                {
                    return typeof(Sprite);
                }
            }
            else if (type == typeof(UnityEditor.SceneAsset))
            {
                return typeof(UnityEngine.SceneManagement.Scene);
            }
            else if (type.FullName == "UnityEditor.Audio.AudioMixerController") { 
                return typeof(UnityEngine.Audio.AudioMixer);
            }

            return type;
        }

        /// <summary>
        /// 根据asset_path 获取到 
        /// </summary>
        /// <param name="asset_path"></param>
        /// <returns></returns>
        internal static string GetAssetNameWithType(string asset_path)
        {
            return GetAssetNameWithType(System.IO.Path.GetFileNameWithoutExtension(asset_path), GetAssetType(asset_path));
        }
#endif 
        internal static string GetAssetNameWithType(string assetName, System.Type type) {
            return string.Format("{0}_{1}", assetName, type.FullName);
        }

    }




}
