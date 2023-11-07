using UnityEngine;
using UnityEditor;
using System.IO;
#if UNITY_EDITOR
namespace YukiFrameWork.Editors.Explorer
{ 
    public class BuildingAssetBundle : EditorWindow
    {
        private static BuildingAssetBundle instance;
        private static string assetBundleDirectory = "AssetBundles";
        private static BuildAssetBundleOptions options;
        private static BuildTarget target;
        public static Rect rect;

        [MenuItem(itemName: "YukiFrameWork/Build AssetBundle")]
        private static void AddBuildAssetBundle()
        {
            instance = (BuildingAssetBundle)GetWindow(typeof(BuildingAssetBundle));
            instance.titleContent = new GUIContent( "AB包生成管理器");
            rect = new Rect(0, 0, 400, 200);
            instance.position = rect;
        }

        private void OnGUI()
        {                   
            GUILayout.Label("请输入打包路径:");

            assetBundleDirectory = EditorGUILayout.TextField("路径：Assets/", assetBundleDirectory);

            if (GUILayout.Button("生成AB包存放路径"))
            {
                string path = Application.streamingAssetsPath + "/" + assetBundleDirectory;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Debug.Log("生成成功");                  
                }
                else
                {
                    Debug.LogWarning("文件夹已存在");
                }               
                string scriptContext = $"{"{\"AssetBundlePath\""}:\"{assetBundleDirectory}\""+"}";
                string jsonPath = "AssetBundlesPath.Json";               
                File.WriteAllText(Application.streamingAssetsPath + "/" + jsonPath, scriptContext);
                AssetDatabase.Refresh();
            }

            options = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("请选择打包选项：", options);
            target = (BuildTarget)EditorGUILayout.EnumPopup("请选择构建的目标平台：", target);

            if (GUILayout.Button("打包AB包"))
            {
                Build();
            }
            
        }
        private static void Build()
        {
            if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath, assetBundleDirectory)))
            {
                Debug.LogError("文件夹未创建，打包失败");
                return;
            }

            if ((int)target == 0)
            {
                Debug.LogError("平台未选择，无法进行打包请重试");
                return;
            }

            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/" + assetBundleDirectory,
            options,
            target);
            AssetDatabase.Refresh();
        }

    }

}
#endif