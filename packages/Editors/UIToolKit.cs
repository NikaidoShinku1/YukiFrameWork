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
            if (GUILayout.Button("��ʼ��UI���"))
            {               
                AssetDatabase.DeleteAsset(fixationScriptPath + "Root" + "/" + "UIRoot.cs");
                Debug.Log("��ʼ���ɹ���");
                string scriptContent = "using UnityEngine;\nusing YukiFrameWork.UI;\nnamespace YukiFrameWork.UI\n{\n    public enum UIPanelType\n    {\n\n    }\n}";
                File.WriteAllText("Assets/YukiFrameWork/UI" + "/" + "UIPanelType" + ".cs", scriptContent);
                AssetDatabase.Refresh();
            }

            GUILayout.Label("���������Ԥ�����ŵ�·��(·�����������Զ�����)");
            panelPath = EditorGUILayout.TextField("Ĭ��·��Resources", panelPath);

            GUILayout.Label("���������ɽű���ŵ�·��(·�����������Զ�����)");
            scriptPath = EditorGUILayout.TextField("Ĭ��·��Assets/Script/", scriptPath);

            if (GUILayout.Button("��������׼�"))
            {
                var panels = Resources.LoadAll<GameObject>(panelPath);
                if (panels.Length <= 0)
                {
                    Debug.LogError("��ǰ·����û��UI���!");
                    return;
                }

                string savePath = fixationScriptPath + scriptPath;

                for (int i = 0; i < panels.Length; i++)
                {                   
                    CreateScript(savePath, panels[i]);
                }
            }
            #region ���½ű������ҹ�
            if (GUILayout.Button("���½ű������"))
            {
                var panels = Resources.LoadAll<GameObject>(panelPath);
                if (panels.Length <= 0)
                {
                    Debug.LogError("���Ϊ�գ��޷����£�");
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

            #region �������

            panelName = EditorGUILayout.TextField("��д������������",panelName);

            if (GUILayout.Button("�������"))
            {
                UnityEngine.EventSystems. EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                Canvas canvas = FindObjectOfType<Canvas>();
                if (eventSystem == null)
                {
                    Debug.LogError("������¼�ϵͳ��֤UI��������ת");
                    return;
                }

                if (canvas == null)
                {
                    Debug.LogError("����ӻ���");
                    return;
                }

                var infos = AssetDatabase.FindAssets("UIRoot");

                if (infos.Length != 0)
                {
                    Debug.LogWarning("�����ű��Ѵ���");
                }
                else
                {
                    CreateRootScript(fixationScriptPath + "Root");
                }
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("������������"))
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
            //�ж������Ƿ��ܱ���ȡ��
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
                Debug.LogError("���ֲ������޷�����");
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