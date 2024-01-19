#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using YukiFrameWork.Extension;

namespace YukiFrameWork.ABManager
{
    public class AssetBundleManagerMenu : MonoBehaviour
    {
        [MenuItem("YukiFrameWork/ABManager/Projects", false, 1)]
        static void AssetBundleManager()
        {

            AssetBundleManagerProjects window = EditorWindow.GetWindow<AssetBundleManagerProjects>("ABManager");
            window.Show();

        }
             
        [MenuItem("YukiFrameWork/ABManager/About", false, 2000)]
        static void Help()
        {          
            Rect rect = new Rect(0, 0, 550, 370);
            AssetBundleManagerHelp window = EditorWindow.GetWindowWithRect<AssetBundleManagerHelp>(rect, true, "About ABManager");
            window.Show();

        }

        //[MenuItem("YukiFrameWork.Res/Test Window", false, 3000)]
        //static void Test()
        //{
        //    AssetBundleProjectMain window = EditorWindow.GetWindow<AssetBundleProjectMain>("Project");
        //    window.Show();
        //}

    }
}
#endif