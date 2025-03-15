using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace XFABManager
{

    [CustomEditor(typeof(XFABProject))]
    public class XFABProjectInspector : Editor
    {

        private XFABProject project;

        private void OnEnable()
        {
            project = target as XFABProject;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(30);

            if (GUILayout.Button("导出资源包")) 
            {  
                string name = project.name;

                if (!string.IsNullOrEmpty(project.displayName))
                    name = string.Format("{0}({1})",name,project.displayName);

                List<string> all_asset_path = new List<string>();

                List<XFABAssetBundle> bundles = project.GetAllAssetBundles();

                foreach (XFABAssetBundle bundle in bundles) {
                    all_asset_path.AddRange( bundle.GetAllAssetPaths());
                }
                  
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Assets"), false, () => 
                {   

                    string path = EditorUtility.OpenFolderPanel("请选择存放路径!", GetProjectDir(), string.Empty);

                    if (string.IsNullOrEmpty(path)) return;

                    path = string.Format("{0}/{1}.unitypackage",path,name);

                    AssetDatabase.ExportPackage(all_asset_path.ToArray(), path, ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
                     
                    EditorUtility.RevealInFinder(path);
                });
                 
                menu.AddItem(new GUIContent("AssetsWithProjectSettings"), false, () => {

                    string path = EditorUtility.OpenFolderPanel("请选择存放路径!", GetProjectDir(), string.Empty);

                    if (string.IsNullOrEmpty(path)) return;

                    path = string.Format("{0}/{1}.unitypackage", path, name);

                    AssetDatabase.ExportPackage(all_asset_path.ToArray(), path, ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets);
                    
                    EditorUtility.RevealInFinder(path);
                });

                menu.ShowAsContext(); 
            }
        }


        private string GetProjectDir() 
        {
            string dir = Application.dataPath;

            if(dir.EndsWith("/"))
                dir =dir.Substring(0, dir.Length - 7);
            else
                dir = dir.Substring(0, dir.Length - 6);

            return dir;
        }


    }

}

#endif