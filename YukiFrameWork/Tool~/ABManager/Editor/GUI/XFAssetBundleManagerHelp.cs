using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.XFABManager
{

    public class PackageInfo {

        public string version;
    }

    public class XFAssetBundleManagerHelp : EditorWindow
    {
        Rect textureRect = new Rect(0, 10, 291 * 0.7F, 96 * 0.7F);
        private GUIStyle style;

        private string version;

        private void Awake()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportSettingWindow.importPath);
            var data = JsonUtility.FromJson<ImportSettingWindow.Data>(asset.text);
            string path = $"{data.path}/ABManager/package.json";
            TextAsset p = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            PackageInfo info = JsonConvert.DeserializeObject<PackageInfo>(p.text);

            version = string.Format("Version {0}", info.version);
        }

        private void ConfigStyle()
        {
            style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.normal.textColor = new Color(0.03f, 0.4f, 0.9f, 1);
            style.onHover.textColor = Color.white;
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Italic;
            //style.onFocused.textColor = Color.red;
        }

        // 每秒10帧更新
        void OnInspectorUpdate()
        {
            //开启窗口的重绘，不然窗口信息不会刷新
            Repaint();
        }

        private void OnGUI()
        {

            GUILayout.BeginHorizontal("WarningOverlay");
            GUILayout.Space(130);
            GUILayout.Label("*ABManager");
            GUILayout.Label(version);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("ABManager提供了AssetBundle的可视化管理功能，我们通过该插件可以很方便的对项目中");
            GUILayout.Label("的AssetBundle进行打包，添加文件，删除文件等等!!");
            GUILayout.Label("除此之外，此插件还提供了AssetBundle的加载，卸载，更新，下载，压缩，释放等功能，");
            GUILayout.Label("通过该插件可以很方便快速的完成AssetBundle相关的功能开发，提升开发效率!");
            GUILayout.Space(20);
            if (style == null)
            {
                ConfigStyle();
            }
            GUILayout.Label("该资源管理插件原作者：弦小风\n插件交流群：1058692738,插件的更多信息可通过下方教程链接获取!");
            GUILayout.Space(20);
            DrawLink("使用教程:", "https://www.bilibili.com/video/BV1Di4y1Y7tb");
            DrawLink("更多教程:", "https://space.bilibili.com/258939476");
            DrawLink("插件源码:", "https://gitee.com/xianfengkeji/xfabmanager");
            GUILayout.EndVertical();

        }

        private void DrawLink(string title, string url)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, GUILayout.Width(60));

            if (GUILayout.Button(url, style))
            {
                Application.OpenURL(url);
            }

            GUILayout.EndHorizontal();
        }

    }

}

