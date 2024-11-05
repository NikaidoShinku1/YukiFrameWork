using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace XFABManager{




    public class AssetBundleBuildPanel
    {

        private Vector3 m_ScrollPosition;

        private BuildTarget buildTarget;
        //private ValidBuildTarget lastBuildTarget;
        GUIContent targetContent;

        private EditorWindow window;

        //private string output_path;

        //private List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();

        private XFABProject project;

        // 是否清空文件夹
        private bool isClearFolders;
        private GUIContent clearFolderContent;

        // 是否复制到 SteaamingAssets
        private bool isCopyToStreamingAssets;
        private GUIContent copyToStreamingAssetsContent;

        // 是否把AssetBundle 压缩为zip Compressed into a zip
        private bool isCompressedIntoZip;
        private GUIContent compressedIntoZipContent;

        // 是否把AssetBundle 压缩为zip Compressed into a zip
        private bool isAutoPackAtlas;
        private GUIContent isAutoPackAtlasContent;


        private GUIContent atlas_compression_content;
        private AtlasCompressionType atlasCompression; // 图集压缩类型


        private GUIContent crunch_compression_content;
        private bool useCrunchCompression; // 是否使用 紧凑压缩

        private GUIContent compressorQualityContent;
        private int compressorQuality; // 压缩质量

        private string update_message;

        // 更新信息
        private GUIContent updateMessageContent;

        private bool revealInFinderOnBuildFinsh;
        private GUIContent revealInFinderContent;

        private string update_date;

        // 更新信息
        //private GUIContent updateDateContent;

        // 高级设置
        private bool buildOptionSetting;
        private bool[] buildAssetBundleOption;


        public AssetBundleBuildPanel(XFABProject project, EditorWindow window) {

            this.window = window;

            targetContent = new GUIContent("Build Target", "请选择要打包的目标平台!");
            buildTarget = EditorUserBuildSettings.activeBuildTarget;

            //output_path = string.Format("{0}/{1}/{2}/{3}", project.out_path(b), project.name, project.version, buildTarget);

            clearFolderContent = new GUIContent("ClearFolders", string.Format("是否在打包前 删除 {0} 这个文件夹下所有的文件!", string.Format("{0}/{1}/{2}/{3}", project.assetbundle_out_path((BuildTarget)buildTarget), project.name, project.version, buildTarget)));

            copyToStreamingAssetsContent = new GUIContent("复制到StreamingAssets", string.Format("是否将打包完成后的 AssetBundle 文件 复制到StreamingAssets 文件夹!"));

            compressedIntoZipContent = new GUIContent("压缩资源", string.Format("是否将打包完成后的 AssetBundle 文件 压缩为.zip!"));

            isAutoPackAtlasContent = new GUIContent("打包图集", string.Format("是否自动打包图集 默认:true"));

            atlas_compression_content = new GUIContent("Compression","图集压缩类型");

            crunch_compression_content = new GUIContent("CrunchCompression","紧凑压缩");

            compressorQualityContent = new GUIContent("CompressorQuality", "压缩质量");

            updateMessageContent = new GUIContent("更新信息", "在这里可以填写本次更新了哪些内容!");

            revealInFinderContent = new GUIContent("RevealInFinderOnBuildFinsh","打包完成后是否打开AssetBundle所在的文件夹?");

            //updateDateContent = new GUIContent("更新日期", "更新日期!");

            buildAssetBundleOption = new bool[project.buildAssetBundleOptions.Count];
            
            this.project = project;
        }

        public void OnGUI()
        {

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            EditorGUILayout.Space();
            // 构建的目标平台
            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup(targetContent, buildTarget);
            EditorGUILayout.Space();

            // 输出路径
            EditorGUILayout.LabelField("Output Path", project.assetbundle_out_path((BuildTarget)buildTarget));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Show In Explorer", GUILayout.MaxWidth(125f)))
            {
                if (Directory.Exists(project.assetbundle_out_path((BuildTarget)buildTarget)))
                {
                    EditorUtility.RevealInFinder(project.assetbundle_out_path((BuildTarget)buildTarget));
                }
                else {
                    EditorUtility.DisplayDialog("文件夹不存在!", string.Format("文件夹{0}不存在!", project.assetbundle_out_path((BuildTarget)buildTarget)), "ok");
                }

            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            DrawToggle(ref project.isClearFolders,ref isClearFolders, clearFolderContent);
            DrawToggle(ref project.isCopyToStreamingAssets, ref isCopyToStreamingAssets, copyToStreamingAssetsContent);
            DrawToggle(ref project.isCompressedIntoZip, ref isCompressedIntoZip, compressedIntoZipContent);
            DrawToggle(ref project.isAutoPackAtlas, ref isAutoPackAtlas, isAutoPackAtlasContent);
           


            if (project.isAutoPackAtlas)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(100);
                project.atlas_compression_type = (AtlasCompressionType)EditorGUILayout.EnumPopup(atlas_compression_content, project.atlas_compression_type,GUILayout.Width(500));
                GUILayout.EndHorizontal();
                if (atlasCompression != project.atlas_compression_type)
                {
                    atlasCompression = project.atlas_compression_type;
                    project.Save();
                }

                if (project.atlas_compression_type != AtlasCompressionType.None)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(100);
                    DrawToggle(ref project.use_crunch_compression, ref useCrunchCompression, crunch_compression_content,GUILayout.Width(500));
                    GUILayout.EndHorizontal();

                    if (project.use_crunch_compression) {

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(100);
                        project.compressor_quality = (int)EditorGUILayout.Slider(compressorQualityContent, project.compressor_quality, 0, 100,GUILayout.Width(500));
                        GUILayout.EndHorizontal();
                        if (compressorQuality != project.compressor_quality)
                        {
                            compressorQuality = project.compressor_quality;
                            project.Save();
                        }
                    }

                }
                //GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();

            GUILayout.Label(updateMessageContent, GUILayout.Width(155));
            project.update_message = EditorGUILayout.TextArea(this.project.update_message, GUILayout.Width(600), GUILayout.Height(200));

            if (project.update_message != null && !project.update_message.Equals(update_message))
            {
                update_message = project.update_message;
                project.Save();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            DrawToggle(ref project.revealInFinderOnBuildFinsh, ref revealInFinderOnBuildFinsh, revealInFinderContent);

            GUILayout.BeginHorizontal();

            //GUILayout.Label(updateDateContent, GUILayout.Width(155));
            //project.update_date = EditorGUILayout.TextField(this.project.update_date, GUILayout.Width(600));

            if (project.update_date != null && !project.update_date.Equals(update_date))
            {
                update_date = project.update_date;
                project.Save();
            }

            GUILayout.EndHorizontal();

            // 高级设置
            EditorGUILayout.Space();
            buildOptionSetting = EditorGUILayout.Foldout(buildOptionSetting, "BuildAssetBundleOptions");
            if (buildOptionSetting)
            {
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;

                for (int i = 0; i < project.buildAssetBundleOptions.Count; i++)
                {
                    BuildOptionToggleData tog = project.buildAssetBundleOptions[i];
                    tog.isOn = EditorGUILayout.ToggleLeft(tog.content, tog.isOn);

                    if ( tog.isOn != buildAssetBundleOption[i] ) {
                        buildAssetBundleOption[i] = tog.isOn;
                        project.Save();
                    }
                }

                EditorGUILayout.Space();

                EditorGUI.indentLevel = indent;
            }

            if (GUILayout.Button("Build"))
            {
                EditorApplication.delayCall += Build;
            }
            if (GUILayout.Button("打包图集"))
            {
                ProjectBuild.PackAtlas(project);
            }
            if (GUILayout.Button(copyToStreamingAssetsContent))
            {
                //CopyToStreamingAssets();
                if (ProjectBuild.CopyToStreamingAssets(project, (BuildTarget)buildTarget))
                {
                    this.window.ShowNotification(new GUIContent("复制成功!"));
                }
                else {
                    this.window.ShowNotification(new GUIContent("复制失败!详情请看控制台!"));
                }
            }
            if (GUILayout.Button(compressedIntoZipContent))
            {
                //CompressedIntoZip();
                if (ProjectBuild.CompressedIntoZip(project, (BuildTarget)buildTarget))
                {
                    this.window.ShowNotification(new GUIContent("压缩成功!"));
                }
                else {
                    this.window.ShowNotification(new GUIContent("压缩失败!详情请看控制台!"));
                }
            }

            EditorGUILayout.EndScrollView(); 
        }

        private void DrawToggle(ref bool toggleValue,ref bool lastValue,GUIContent content,params GUILayoutOption[] options) {

            toggleValue = GUILayout.Toggle(toggleValue, content, options);
            if (toggleValue != lastValue)
            {
                lastValue = toggleValue;
                project.Save();
            }

        }

        // 构建 AssetBundle
        public void Build() {
            ProjectBuild.Build(project, (BuildTarget)buildTarget,project.revealInFinderOnBuildFinsh);
        }
}


    public enum ValidBuildTarget
    {
        StandaloneOSX = 2,
        StandaloneWindows = 5,
        iOS = 9,
        Android = 13,
        StandaloneWindows64 = 19,
        WebGL = 20,
        WSAPlayer = 21,
        StandaloneLinux64 = 24,
        PS4 = 31,
        XboxOne = 33,
        tvOS = 37,
        Switch = 38,
        Lumin = 39,
        Stadia = 40
    }

    public enum CompressOptions
    {
        Uncompressed = 0,
        StandardCompression,
        ChunkBasedCompression,
    }

    

}
#endif