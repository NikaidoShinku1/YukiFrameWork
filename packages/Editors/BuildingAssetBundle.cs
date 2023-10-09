using System.Collections;
using System.Collections.Generic;
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
            rect = new Rect(0, 0, 400, 200);
            instance.position = rect;
        }

        private void OnGUI()
        {          
            
            GUILayout.Label("��������·��:");

            assetBundleDirectory = EditorGUILayout.TextField("·����Assets/", assetBundleDirectory);

            if (GUILayout.Button("����AB�����·��"))
            {
                string path = "Assets" + "/" + assetBundleDirectory;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Debug.Log("���ɳɹ�");
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogWarning("�ļ����Ѵ���");
                }
            }

            options = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("��ѡ����ѡ�", options);
            target = (BuildTarget)EditorGUILayout.EnumFlagsField("��ѡ�񹹽���Ŀ��ƽ̨��", target);

            if (GUILayout.Button("���AB��"))
            {
                Build();
            }
            
        }
        private static void Build()
        {
            if (!Directory.Exists(Path.Combine("Assets", assetBundleDirectory)))
            {
                Debug.LogError("�ļ���δ���������ʧ��");
                return;
            }

            if ((int)target == 0)
            {
                Debug.LogError("ƽ̨δѡ���޷����д��������");
                return;
            }

            BuildPipeline.BuildAssetBundles("Assets" + "/" + assetBundleDirectory,
            options,
            target);
        }

    }

}
#endif