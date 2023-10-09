using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using YukiFrameWork.UI;

#if UNITY_EDITOR
namespace YukiFrameWork.Editors.UI
{
    public class UIToolKit : EditorWindow
    {
        private static UIToolKit instance;

        private readonly string fixationScriptPath = "Assets/Script/";

        private static string panelPath = "";

        private static string scriptPath = "UIPanel";

        private static string panelName = "";
        
        [MenuItem(itemName: "YukiFrameWork/UIToolKit")]
        public static void UIToolKitPanel()
        {
            instance = GetWindow<UIToolKit>();
            Rect rect = new Rect(0, 0, 300, 400);
            instance.position = rect;
        }

        private void OnGUI()
        {         
            if (GUILayout.Button("初始化UI框架"))
            {               
                AssetDatabase.DeleteAsset(fixationScriptPath + "Root" + "/" + "UIRoot.cs");
                Debug.Log("初始化成功！");
                string scriptContent = "using UnityEngine;\nusing YukiFrameWork.UI;\nnamespace YukiFrameWork.UI\n{\n    public enum UIPanelType\n    {\n\n    }\n}";
                File.WriteAllText("Assets/YukiFrameWork/UI" + "/" + "UIPanelType" + ".cs", scriptContent);
                AssetDatabase.Refresh();
            }

            GUILayout.Label("请输入面板预制体存放的路径(路径不存在则自动创建)");
            panelPath = EditorGUILayout.TextField("默认路径Resources", panelPath);

            GUILayout.Label("请输入生成脚本存放的路径(路径不存在则自动创建)");
            scriptPath = EditorGUILayout.TextField("默认路径Assets/Script/", scriptPath);

            if (GUILayout.Button("生成面板套件"))
            {
                var panels = Resources.LoadAll<GameObject>(panelPath);
                if (panels.Length <= 0)
                {
                    Debug.LogError("当前路径下没有UI面板!");
                    return;
                }

                string savePath = fixationScriptPath + scriptPath;

                for (int i = 0; i < panels.Length; i++)
                {                   
                    CreateScript(savePath, panels[i]);
                }
            }
            #region 更新脚本与面板挂钩
            if (GUILayout.Button("更新脚本到面板"))
            {
                var panels = Resources.LoadAll<GameObject>(panelPath);
                if (panels.Length <= 0)
                {
                    Debug.LogError("面板为空，无法更新！");
                    return;
                }

                string typeName = "";
                foreach (var item in panels)
                {
                    typeName += "    "+item.name + ",\n";
                }
                CreateTypeScript(typeName);
                AssetDatabase.Refresh();
                //Debug.Log(typeName);
                foreach (var item in panels)
                {
                    if (item.GetComponent<BasePanel>() == null)
                    {
                        AddScriptToPanel(fixationScriptPath + scriptPath, item);                      
                    }
                }

                AssetDatabase.Refresh();
            }
            #endregion

            #region 框架启动

            panelName = EditorGUILayout.TextField("请写入主面板的名字",panelName);

            if (GUILayout.Button("添加启动"))
            {
                UnityEngine.EventSystems. EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                Canvas canvas = FindObjectOfType<Canvas>();
                if (eventSystem == null)
                {
                    Debug.LogError("请添加事件系统保证UI的正常运转");
                    return;
                }

                if (canvas == null)
                {
                    Debug.LogError("请添加画布");
                    return;
                }

                var infos = AssetDatabase.FindAssets("UIRoot");

                if (infos.Length != 0)
                {
                    Debug.LogWarning("启动脚本已存在");
                }
                else
                {
                    CreateRootScript(fixationScriptPath + "Root");
                }
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("绑定启动到场景"))
            {
                GameObject root = GameObject.Find("Root");
                if (root == null)
                {
                    root = new GameObject
                    {
                        name = "Root"
                    };
                }             
                string overPath = fixationScriptPath + "Root" + "/" + "UIRoot.cs";
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(overPath);
                Debug.Log(script);

                root.AddComponent(script.GetClass());
            }
            #endregion

        }

        private static void CreateTypeScript(string name)
        {
            string scriptContent = $"using UnityEngine;\nusing YukiFrameWork.UI;\nnamespace YukiFrameWork.UI\n" +
                "{\n    public enum UIPanelType" +
                "\n    {" +
                "\n" +
                name +
                "\n    }\n}";
            //MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/YukiFrameWork/UI" + "/" + "UIPanelType" + ".cs");
            File.WriteAllText("Assets/YukiFrameWork/UI" + "/" + "UIPanelType" + ".cs", scriptContent);
        }

        private static void CreateRootScript(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string overPath = path + "/" + "UIRoot.cs";
            var panels = Resources.LoadAll<GameObject>(panelPath);
            //判断名字是否能被获取到
            bool isName = false;
            foreach (var panel in panels)
            {
                if (panelName == panel.name)
                {
                    isName = true;
                    break;
                }                
            }
            if (!isName)
            {
                Debug.LogError("名字不存在无法加载");
                return;
            }
            
            string scriptContent = $"using UnityEngine;\nusing YukiFrameWork.UI;\npublic class UIRoot : MonoBehaviour" +
                "\n{" +
                "\n    private void Awake()\n" +
                "    {\n" +
                $"        UIManager.Instance.PushPanel(UIPanelType.{panelName});" +
                "\n    }\n}";
            File.WriteAllText(overPath, scriptContent);          
        }

        private static void CreateScript(string path, GameObject panel)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string overPath = path + "/" + panel.name + ".cs";

            string scriptContent = $"using UnityEngine;\nusing YukiFrameWork.UI;\npublic class {panel.name} : BasePanel\n " +
                "{" +
                "\n\n" +
                "}";

            File.WriteAllText(overPath, scriptContent);

            AssetDatabase.Refresh();       
        }

        private static void AddScriptToPanel(string overPath,GameObject panel)
        {
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(overPath + "/" + panel.name + ".cs");
            Debug.Log(script + " Error:" + overPath + panel.name + ".cs");           
            panel.AddComponent(script.GetClass());           
        }
    }
}
#endif