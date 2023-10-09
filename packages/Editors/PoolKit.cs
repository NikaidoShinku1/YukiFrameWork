using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YukiFrameWork.Pools;
using System.IO;

#if UNITY_EDITOR
namespace YukiFrameWork.Editors.Explorer
{
    [System.Serializable]
    public class ObjData
    {
        public string type;
        public GameObject prefab;
    }

    public class PoolKit : EditorWindow
    {
        private static PoolKit instance => GetWindow<PoolKit>();
        
        private static SerializedProperty dataProperty;        
        private static SerializedObject serializedObject;
           

        public List<ObjData> datas = new List<ObjData>();
        public PoolsData poolSaveData;

        private static string overPath;

        [MenuItem(itemName: "YukiFrameWork/PoolsKit")]
        public static void OnPoolsWindow()
        {           

            Rect rect = new Rect(0, 0, 600, 1200);
            instance.position = rect;
            instance.autoRepaintOnSceneChange = true;
        }
       
        public void OnEnable()
        {
            serializedObject = new SerializedObject(this);           
            dataProperty = serializedObject.FindProperty("datas");
            
        }       

        private void OnValidate()
        {
            //saveDataObject.ApplyModifiedProperties();
            if (poolSaveData != null)
            {
                overPath = AssetDatabase.GetAssetPath(poolSaveData);
                Debug.Log(overPath);
                var data = AssetDatabase.LoadAssetAtPath<PoolsData>(overPath);
                Debug.Log(data);
            }
        }     

        private void OnGUI()
        {
            if (GUILayout.Button("�������ݳ�"))
            {
                if (poolSaveData != null)
                {
                    Debug.LogWarning("���ݳ��Ѵ���!");
                    return;
                }
                string path = "Assets/PoolsData";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var data = CreateInstance<PoolsData>();
                AssetDatabase.CreateAsset(data, path + "/PoolsData.asset");
                
                poolSaveData = data;
                
                AssetDatabase.Refresh();
               
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("��ʼ�����ݳ�"))
            {
                if (poolSaveData == null)
                {
                    Debug.LogError("���ݳ�Ϊ�գ��޷���ʼ��");
                    return;
                    //AssetDatabase.GetAssetPath();
                }
                foreach (var item in poolSaveData.dataSaves)
                {
                    DestroyImmediate(item.obj.GetComponent<InitPrefab>());
                }
                poolSaveData.dataSaves.Clear();
                AssetDatabase.Refresh();
            }
            
            BeginWindows();
            Rect rect;
            rect = GUILayout.Window(1, new Rect(200, 150, 100, 400), ChildGUI,"");
           
            EndWindows();         
        }

        private void ChildGUI(int uncurrentID)
        {
            GUILayout.Label("Ϊ��Ʒ������ͳ�ʼ��,��д��Ʒ�������Լ���Ӧ������", EditorStyles.boldLabel);


            EditorGUILayout.PropertyField(dataProperty, true);
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("����"))
            {
                string path = "Assets/YukiFrameWork/Pools/ObjectType.cs";
                if (!File.Exists(path))
                {
                    Debug.LogError($"�����ඪʧ����Ż�ָ��λ�ã�λ��Ϊ��{path}");
                    return;
                }
                string name = "";
                foreach (var item in datas)
                {           
                    if(item.type.Length == 0)
                    {
                        Debug.LogWarning($"����δ�����޷���Ӹ�obj��objΪ{item.prefab}");
                        continue;
                    }
                    name += "        " + item.type + ",\n";
                }
                string content = "using UnityEngine;\nnamespace YukiFrameWork.Pools\n{" +
                    "\n    public enum ObjectType\n    {" +
                    $"    \n{name}" +
                    "\n    }\n}";
                File.WriteAllText(path, content);
                AssetDatabase.Refresh();
            }           

            if (GUILayout.Button("�������ݳ�"))
            {
                var data = datas.Find(x => x.type.Length == 0 || x.prefab == null);
                if (data != null)
                {
                    Debug.LogError("�޷��������ݳأ�ԭ��Ϊ����Ʒû��ָ�����ͻ���û�з���prefab");
                    return;
                }
                foreach (var item in datas)
                {
                    if(item.prefab.GetComponent<InitPrefab>() == null)
                    item.prefab.AddComponent<InitPrefab>();
                    Debug.Log(item.type);                    
                    System.Enum.TryParse(item.type,out ObjectType type);
                    poolSaveData.dataSaves.Add(new DataSave(type, item.prefab));
                }
                Debug.Log("���³ɹ���");
                datas.Clear();              
                serializedObject.Update();
                AssetDatabase.Refresh();
            }          
        }
    }
}
#endif
