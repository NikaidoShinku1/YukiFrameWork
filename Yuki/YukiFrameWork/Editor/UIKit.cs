using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using YukiFrameWork.UI;
using YukiFrameWork.Res;
using LitJson;
using System.Text;
using UnityEditorInternal;
#if UNITY_EDITOR
namespace YukiFrameWork.Editors.UI
{   
    public class UIKit : EditorWindow
    {
        private static UIKit instance;

        private readonly string fixationScriptPath = @"Assets\";

        private static string panelPath = "";

        private static string scriptPath = @"Scripts\";    

        private static string assetBundleName;      

        private static Attribution loadType;

        private static bool isAutoLayerChange;

        private static ReorderableList stringList;

        private static SerializedProperty serializedProperty;
        private static SerializedObject serializedObject;     
        public string[] panelLayerName = new string[]
        {
            "Common",          
            "Tip",
            "Top"
        };
        
        [MenuItem(itemName: "YukiFrameWork/UIKit")]
        public static void UIToolKitPanel()
        {
            instance = GetWindow<UIKit>();
            instance.titleContent = new GUIContent("UI管理套件");
            Rect rect = new Rect(0, 0, 600, 400);
            instance.position = rect;
           

        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            serializedProperty = serializedObject.FindProperty("panelLayerName");
        }

        private void OnGUI()
        {         
            InitUIFrameWork();
            InputPath();
            UpdateScriptsAndPath();

            isAutoLayerChange = EditorGUILayout.Toggle("是否自定义层级", isAutoLayerChange);
            if (!isAutoLayerChange)
            {
                panelLayerName = new string[]
                {
                   "Common",                  
                   "Tip",
                   "Top"
                };
            }
            else
            {
                Update_PanelType();
            }

        }

        private void InputPath()
        {
            GUILayout.Space(10);
            GUILayout.Label("请输入面板预制体存放的路径");

            panelPath = EditorGUILayout.TextField("默认路径Assets/：", panelPath);

            GUILayout.Space(10);
            loadType = (Attribution)EditorGUILayout.EnumPopup("面板在运行时的加载方式", loadType);

            switch (loadType)
            {
                case Attribution.Resources:
                    assetBundleName = string.Empty;
                    GUILayout.Label("使用Resources加载面板时则面板预制体存放路径需在Resources文件夹内");
                    break;
                case Attribution.AssetBundle:
                    assetBundleName = EditorGUILayout.TextField("请输入包名", assetBundleName);
                    break;         
            }

            GUILayout.Space(10);
            GUILayout.Label("请输入生成脚本存放的路径(将脚本放置于设置默认路径之下)");
            scriptPath = EditorGUILayout.TextField("默认路径Assets/", scriptPath);

        }

        private void InitUIFrameWork()
        {
            GUILayout.Label("注意:初始化是为将UI框架的层级重置为默认状态，提供Common,Tip,Top层级！");
            if (GUILayout.Button("初始化UI框架"))
            {

                panelLayerName = new string[]
                {
                   "Common",
                   "Tip",
                   "Top"
                };
                Debug.Log("初始化成功！");
                string typeName = "";
                foreach (var item in panelLayerName)
                {
                    typeName += "    " + item + ",\n";
                }
                string scriptContent = $"using UnityEngine;\n\n\nnamespace YukiFrameWork.UI\n" +
                "{\n    public enum UIPanelType" +
                "\n    {" +
                "\n" +
                              typeName +
                "\n    }\n" +
                "}";
                //MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/YukiFrameWork/UI" + "/" + "UIPanelType" + ".cs");
                File.WriteAllText(@"Assets\YukiFrameWork\UI" + @"\" + "UIPanelType" + ".cs", scriptContent, Encoding.UTF8);
                AssetDatabase.Refresh();
            }
        }

        #region 更新脚本与面板挂钩
        private void UpdateScriptsAndPath()
        {
            if (GUILayout.Button("更新脚本到面板"))
            {               
                List<GameObject> panels = new List<GameObject>();
                var pathGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { @"Assets\" + (panelPath == string.Empty ? panelPath : panelPath + @"\" ) }); ;
                foreach (var item in pathGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(item);
                    panels.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                }              
                if (panels.Count <= 0)
                {
                    Debug.LogError("面板为空，无法更新！");
                    return;
                }

                string typeName = "";
                foreach (var item in panels)
                {
                    typeName += "        " + item.name + ",\n";
                }               
                AssetDatabase.Refresh();
                //Debug.Log(typeName);
                foreach (var item in panels)
                {                  
                    if (item.GetComponent<BasePanel>() == null)
                    {
                        AddScriptToPanel(fixationScriptPath + scriptPath, item);
                    }
                }
                CreatePathOfJson();
                AssetDatabase.Refresh();
            }
        }
        #endregion

        private void Update_PanelType()
        {         
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(serializedProperty,true);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
            if (GUILayout.Button("载入面板层级"))
            {
                string finishName = string.Empty;
                foreach (var typeName in panelLayerName)
                    finishName += "    " + typeName + ",\n";
                CreateTypeScript(finishName);
                AssetDatabase.Refresh();
            }
        }
        /// <summary>
        /// 生成面板路径储存位置
        /// </summary>
        private void CreatePathOfJson()
        {
            string direPath = Application.streamingAssetsPath + @"\"+"UIPanel";
            if (!File.Exists(direPath))
            {
                Directory.CreateDirectory(direPath);
                AssetDatabase.Refresh();
            }
            string newPath = string.Empty;
            if (panelPath.Contains("Resources/"))
            {
                newPath = panelPath.Replace("Resources/", string.Empty);
            }
            else if (panelPath.Contains("Resources"))
            {
                newPath = panelPath.Replace("Resources", "");
            }
            
            string overPath = direPath + "/UIPath.Json";
            UIPath path = new UIPath(newPath, assetBundleName);
            path.type = loadType;
            string pathJson = JsonMapper.ToJson(path);

            File.WriteAllText(overPath,pathJson,Encoding.UTF8);
        }

        private static void CreateTypeScript(string name)
        {
            string scriptContent = $"using UnityEngine;\n\n\nnamespace YukiFrameWork.UI\n" +
                "{\n    public enum UIPanelType" +
                "\n    {" +
                "\n"   +
                          name +
                "\n    }\n" +
                "}";
            //MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/YukiFrameWork/UI" + "/" + "UIPanelType" + ".cs");
            File.WriteAllText(@"Assets\YukiFrameWork\UI" + @"\" + "UIPanelType" + ".cs", scriptContent, Encoding.UTF8);
        }          

        private static void AddScriptToPanel(string overPath,GameObject panel)
        {
            string path = overPath + @"\" + panel.name + ".cs";
            if (!File.Exists(path))
            {
                Debug.LogError("更新失败，失败原因：路径内不存在脚本文件！");
                return;
            }
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            Debug.Log("更新成功！");           
            panel.AddComponent(script.GetClass());           
        }
    }
}
#endif