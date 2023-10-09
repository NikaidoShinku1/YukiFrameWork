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
            if (GUILayout.Button("生成数据池"))
            {
                if (poolSaveData != null)
                {
                    Debug.LogWarning("数据池已存在!");
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

            if (GUILayout.Button("初始化数据池"))
            {
                if (poolSaveData == null)
                {
                    Debug.LogError("数据池为空，无法初始化");
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
            GUILayout.Label("为物品添加类型初始化,填写物品类型名以及对应的名字", EditorStyles.boldLabel);


            EditorGUILayout.PropertyField(dataProperty, true);
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("生成"))
            {
                string path = "Assets/YukiFrameWork/Pools/ObjectType.cs";
                if (!File.Exists(path))
                {
                    Debug.LogError($"类型类丢失，请放回指定位置，位置为：{path}");
                    return;
                }
                string name = "";
                foreach (var item in datas)
                {           
                    if(item.type.Length == 0)
                    {
                        Debug.LogWarning($"类型未填下无法添加该obj，obj为{item.prefab}");
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

            if (GUILayout.Button("更新数据池"))
            {
                var data = datas.Find(x => x.type.Length == 0 || x.prefab == null);
                if (data != null)
                {
                    Debug.LogError("无法更新数据池，原因为有物品没有指定类型或者没有放置prefab");
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
                Debug.Log("更新成功！");
                datas.Clear();              
                serializedObject.Update();
                AssetDatabase.Refresh();
            }          
        }
    }
}
#endif
