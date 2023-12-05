using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using YukiFrameWork.Res.Editors;
#if UNITY_EDITOR
namespace YukiFrameWork.Res
{
    public class BuildingWindow : EditorWindow
    {
        private static BuildingWindow instance;     
        public static Rect rect;
        private static string configPath = @"Assets/GameData";
        private static string configName = "ABConfig";
        private static string dataPath = @"Assets/GameData/Data";
        private static ABConfig abConfig = null;    
        private static SerializedObject serializedObject;
      
        private static bool loadFromAssetBundle;
        private static SerializedProperty property;

        [MenuItem(itemName: "YukiFrameWork/ResKit资源配置工具")]
        private static void AddBuildAssetBundle()
        {
            instance = GetWindow<BuildingWindow>();
            instance.titleContent = new GUIContent("ResKit");
            rect = new Rect(0, 0, 600, 800);
            instance.position = rect;
            Init();
            abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(configPath + @"\" + string.Format("{0}.asset", configName));
        }

        private static void Init()
        {
            serializedObject = new SerializedObject(instance);
            property = serializedObject.FindProperty("abConfig");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("AB配置表");
            abConfig = EditorGUILayout.ObjectField(abConfig, typeof(ABConfig),true) as ABConfig;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            CreateConfig();
            GUILayout.Space(10);
            Build();
        }

        private void OnValidate()
        {
            if (abConfig == null)
            {
                abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(configPath + @"\" + string.Format("{0}.asset", configName));
            }
        }

        private void CreateConfig()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("配置表的存放文件夹路径(注意：路径末尾不能带斜杠)");
            configPath = EditorGUILayout.TextField(configPath);
            GUILayout.Label("配置表的名称(注意：从此加载后不允许在外部修改名称，防止出现问题)");
            configName = EditorGUILayout.TextField(configName);
            GUILayout.Space(10);
            var btn = GUILayout.Button("创建配置表");
           
            if (btn)
            {
                if (abConfig == null)
                {
                    abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(configPath + @"\" + string.Format("{0}.asset", configName));
                    if (abConfig == null)
                    {
                        abConfig = CreateInstance<ABConfig>();

                        if (!Directory.Exists(configPath))
                        {
                            Directory.CreateDirectory(configPath);
                        }

                        AssetDatabase.CreateAsset(abConfig, configPath + @"\" + string.Format("{0}.asset", configName));
                        AssetDatabase.Refresh();
                    }
                }
                else
                {
                    Debug.LogWarning("配置表已存在，不需要创建 abConfig：" + abConfig);
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(20);
            string description;
          
            if(loadFromAssetBundle)
                description = "当前模式下将正常使用AssetBundle加载资源,符合正常开发打包后的加载，需要在使用前对资源进行打包处理!";
            else           
                description = "当前模式下将在编辑器下脱离assetbundle加载，适合在开发时选择";
                              
            GUILayout.Label(description);
            loadFromAssetBundle = EditorGUILayout.Toggle("真机模式", loadFromAssetBundle);
          
            GUILayout.Space(10);

            GUILayout.Label("创建资源依赖文件所在的路径(注意：依赖项不要与配置表在同一层级下)");
            dataPath = EditorGUILayout.TextField(dataPath);
        }

        private void Build()
        {
            if (GUILayout.Button("打包并生成依赖文件"))
            {
                if (abConfig == null)
                {
                    Debug.LogError("当前没有生成配置表，请重试！");
                    return;
                }
                if (Directory.Exists(dataPath))
                {
                    BundleEditor.Build(configPath,configName,dataPath,loadFromAssetBundle);
                }
                else
                {
                    Debug.LogError("指派的文件夹路径不存在,需要先生成路径！ConfigPath: " + dataPath);
                }
            }
        }
    }

}
#endif