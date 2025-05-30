///=====================================================
/// - FileName:      AnimationClipConvertWindow.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/5/29 15:22:05
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System.IO;
using System.Linq;
namespace YukiFrameWork
{
    public class AnimationClipParamListTree : TreeView
    {
        private FrameworkConfigInfo configInfo;
        #region 重写方法
        protected override TreeViewItem BuildRoot()
        {

            TreeViewItem root = new TreeViewItem(-1, -1);            
            if (configInfo != null && !string.IsNullOrEmpty(configInfo.AnimationConvertInfo.folderPath))
            {
                index = 1;
                TreeViewItem item = new TreeViewItem(0, 0, configInfo.AnimationConvertInfo.folderPath);
                root.AddChild(item);
                AddChildren(configInfo.AnimationConvertInfo.folderPath,item);
                SetupDepthsFromParentsAndChildren(root);

            }

            return root;
        }

        private int index;

        private void AddChildren(string folderPath,TreeViewItem item)
        {
            string[] folders = AssetDatabase.GetSubFolders(folderPath);            
            for (int i = 0; i < folders.Length; i++)
            {              
                string folder = folders[i];
                TreeViewItem subItem = new TreeViewItem(index++, 0,folder);
                item.AddChild(subItem);
                AddChildren(folder, subItem);
            }
        }
        

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        
        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);

            var item = FindItem(id, rootItem);
            if (item.children == null)
            {
                string path = item.displayName;

                string targetPath = path + $"/{path.Split('/').LastOrDefault()}.anim";
                configInfo.AnimationConvertInfo.showViewPath = targetPath;
            }
            else
            {
                configInfo.AnimationConvertInfo.showViewPath = item.displayName;
            }
           

            EditorUtility.SetDirty(configInfo);
        }
      

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            if (args.item.parent.id == -1) return;
            if (args.label.Equals(configInfo.AnimationConvertInfo.showViewPath)
                || configInfo.AnimationConvertInfo.showViewPath.Contains(args.label))
            {
                GUI.Label(args.rowRect, "√");
            }
        }

        #endregion

        #region 方法
        public AnimationClipParamListTree(TreeViewState state,FrameworkConfigInfo configInfo) : base(state)
        {          
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.configInfo = configInfo;
          
        }


        #endregion



    }
    public class AnimationClipConvertSearenchWindow : PopupWindowContent
    {

        #region 字段
        private FrameworkConfigInfo configInfo;
        private float width;
        private float height;
        // 搜索框
        private SearchField searchField;
        private Rect searchRect;
        const float searchHeight = 25f;

        // 标签 
        private Rect labelRect;
        const float labelHeight = 30f;

        // 参数列表
        private AnimationClipParamListTree paramTree;
        private TreeViewState paramState;
        private Rect paramRect;

        #endregion


        public AnimationClipConvertSearenchWindow(float width,float height,FrameworkConfigInfo configInfo)
        {
            this.width = width;
            this.height = height;
            this.configInfo = configInfo;
        }


        public override Vector2 GetWindowSize()
        {
            return new Vector2(this.width, this.height);
        }

        public override void OnGUI(Rect rect)
        {
            if (paramTree == null)
            {
                if (paramState == null)
                {
                    paramState = new TreeViewState();
                }

                paramTree = new AnimationClipParamListTree(paramState,configInfo);
                paramTree.Reload();
            }

            // 搜索框
            if (searchField == null)
            {
                searchField = new SearchField();
            }
            searchRect.Set(rect.x + 5, rect.y + 5, rect.width - 10, searchHeight);
            paramTree.searchString = searchField.OnGUI(searchRect, paramTree.searchString);

            // 标签
            labelRect.Set(rect.x, rect.y + searchHeight, rect.width, labelHeight);
            string path = configInfo.AnimationConvertInfo.showViewPath;
            EditorGUI.LabelField(labelRect, string.IsNullOrEmpty(path) ? "全部显示" : path, GUI.skin.GetStyle("AC BoldHeader"));

            // 参数列表 


            paramRect.Set(rect.x, rect.y + searchHeight + labelHeight - 5, rect.width, rect.height - searchHeight - labelHeight);
            paramTree.OnGUI(paramRect);

        }

    }

    public struct SingleTexture2DConvertSetting : IConvertAnimationClipSetting
    {
        private AnimationConvertInfo.Info.FrameInfo[] frameInfos;
        public SingleTexture2DConvertSetting(AnimationConvertInfo.Info info,AnimationConvertInfo.Info.FrameInfo[] frameInfos)
        {
            this.frameInfos = frameInfos;
            Loop = info.loop;
            CycleOffset = info.cycleOffset;
        }
        public bool Loop { get; }

        public float CycleOffset { get; }

        public bool Condition(Sprite sprite)
        {
            return true;
        }

        public float GetFrameRate(Sprite sprite, int index, int length)
        {
            return frameInfos[index].frame;
        }
    }
    public class AnimationClipConvertWindow : MEditorWindow
    {
        private FrameworkConfigInfo configInfo;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            configInfo = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
           
        }

        private GUIStyle titleStyle;
        private GUIStyle subTitleStyle;
        private GUIStyle desStyle;
        private Vector2 scrollView;
        protected override void OnGUI()
        {
            if (titleStyle == null)
            {
                ReLoad();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sprite-AnimationClip转换工具",titleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Single");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(15);
            EditorGUI.BeginChangeCheck();
         
            var rect = EditorGUILayout.BeginHorizontal();
            configInfo.AnimationConvertInfo.folderPath = EditorGUILayout.TextField("文件夹路径:", configInfo.AnimationConvertInfo.folderPath);
            CodeManager.DragObject(rect,out string path);
            if (!string.IsNullOrEmpty(path))
                configInfo.AnimationConvertInfo.folderPath = path;
            if (GUILayout.Button("...",GUILayout.Width(40)))
            {
                configInfo.AnimationConvertInfo.folderPath = CodeManager.SelectFolder(configInfo.AnimationConvertInfo.folderPath);
            }
            if (GUILayout.Button("清空", GUILayout.Width(40)))
            {
                configInfo.AnimationConvertInfo.info_Values.Clear();
                configInfo.AnimationConvertInfo.folderPath = string.Empty;
                configInfo.AnimationConvertInfo.showViewPath = string.Empty;
                EditorUtility.SetDirty(configInfo);
            }
            EditorGUILayout.EndHorizontal();
            var whe =  EditorGUILayout.BeginVertical();
            
            EditorGUILayout.HelpBox("Clip名称由存放Sprite的文件夹名称约定,Single会自动过滤SpriteMode为Multiple的图片。",MessageType.Info);

            if (GUILayout.Button("文档官网", GUILayout.Height(50), GUILayout.Width(100)))
            {
                Application.OpenURL(@"https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Extension/17.AnimationClip转换.md");
            }
            if (configInfo.AnimationConvertInfo.info_Values.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("搜索路径");
                if (GUILayout.Button(configInfo.AnimationConvertInfo.showViewPath))
                {
                    whe.x += whe.width / 2;
                    PopupWindow.Show(whe, new AnimationClipConvertSearenchWindow(whe.width / 2, 200, configInfo));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            scrollView =  EditorGUILayout.BeginScrollView(scrollView,GUI.skin.box);
            foreach (var item in configInfo.AnimationConvertInfo.info_Values)
            {                
                if ((item.Key != configInfo.AnimationConvertInfo.showViewPath 
                    && !item.Key.Contains(configInfo.AnimationConvertInfo.showViewPath)) 
                    || string.IsNullOrEmpty(configInfo.AnimationConvertInfo.showViewPath)) continue;
                EditorGUILayout.BeginVertical("OL Box");
               
                EditorGUILayout.LabelField("预制路径:"+item.Key.Replace(@"\",@"/"), subTitleStyle);
                EditorGUILayout.Space();
                var info = item.Value;
             
                foreach (var frameInfo in info.frameInfos)
                {                 
                    if (string.IsNullOrEmpty(frameInfo.guid)) continue;                 
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUI.color = Color.yellow;
                        GUILayout.Label("文件路径:", GUILayout.Width(75));
                        if (GUILayout.Button(AssetDatabase.GUIDToAssetPath(frameInfo.guid), GUI.skin.label, GUILayout.ExpandWidth(false)))
                        {
                            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(frameInfo.guid));
                            Selection.activeObject = t;
                            if(t)
                                EditorGUIUtility.PingObject(t);
                        }
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("时间帧", GUILayout.Width(40));
                        frameInfo.frame = EditorGUILayout.FloatField(frameInfo.frame, GUILayout.Width(25));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(5);
                    }
                }

                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("基本设置");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Loop", GUILayout.Width(100));
                info.loop = EditorGUILayout.Toggle(info.loop);
                EditorGUILayout.EndHorizontal();
                if (info.loop)
                {                  
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("CycleOffset", GUILayout.Width(100));
                    info.cycleOffset = EditorGUILayout.FloatField(info.cycleOffset);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();           
            if (GUILayout.Button("开始转换",GUILayout.Height(25)))
            {
                string folderPath = configInfo.AnimationConvertInfo.folderPath;

                if (folderPath.IsNullOrEmpty())
                {
                    Debug.LogError("丢失路径");
                    return;
                }
                if (!Directory.Exists(folderPath))
                {
                    Debug.LogWarning("路径非文件夹路径,无法进行转换");
                    return;
                }               
                convertProgress = GetCount(folderPath);
                int value = 0;
                void AllConvert(string folderPath)
                {
                    value++;
                    foreach (var item in Directory.GetDirectories(folderPath, "*")
                .Where(x => !x.EndsWith(".meta")))
                    {                       
                        if (Directory.Exists(item))
                        {
                            AllConvert(item);
                            continue;
                        }                       
                    }

                    Convert(folderPath);
                    EditorUtility.DisplayProgressBar("读取文件夹目录下所有的精灵", $"正在读取并生成预制AnimationClip数据 ---进度:{value}/{convertProgress}", (float)value / convertProgress);
                }               
                AllConvert(folderPath);
                EditorUtility.ClearProgressBar();
                
                //selects = configInfo.AnimationConvertInfo.info_Values.Keys.ToArray();

            }          
            EditorGUILayout.Space();
            if ((configInfo.AnimationConvertInfo.info_Values.Count != 0) && GUILayout.Button("构建全部AnimationClip",GUILayout.Height(25)))
            {
                float progress = 0;

                foreach (var item in configInfo.AnimationConvertInfo.info_Values)
                {
                    progress++;

                    string importPath = item.Key;
                    Sprite[] sprites = item.Value.frameInfos.Select(x => YukiAssetDataBase.GUIDToInstance<Sprite>(x.guid)).ToArray();                    
                    YukiAnimationClipConvertUtility.CreateAnimationClipAndCreateAsset(new SingleTexture2DConvertSetting(item.Value,item.Value.frameInfos),sprites,importPath);
                    EditorUtility.DisplayProgressBar("构建AnimationClip",$"正在根据所有的预制信息生成AnimationClip ---进度:{progress}/{configInfo.AnimationConvertInfo.info_Values.Count}",progress / configInfo.AnimationConvertInfo.info_Values.Count);
                }

                EditorUtility.ClearProgressBar();
            }
          
                        
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(configInfo);
            }
        }
        int GetCount(string folderPath)
        {
            var f = AssetDatabase.GetSubFolders(folderPath);
            // EditorUtility.DisplayProgressBar("读取数据", "正在读取",)
            int count = f.Length;
            foreach (var i in f)
            {
                int subCount = GetCount(i);
                count += subCount;
            }
            return count;
        }
        void ReLoad()
        {
            titleStyle = new GUIStyle();
            titleStyle.fontSize = 14;
            titleStyle.normal.textColor = Color.gray;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperLeft;
            subTitleStyle = new GUIStyle();
            subTitleStyle.fontSize = 16;
            subTitleStyle.fontStyle = FontStyle.Bold;
            subTitleStyle.alignment = TextAnchor.UpperCenter;
            subTitleStyle.normal.textColor = Color.white;
            desStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft
            };
            desStyle.normal.textColor = Color.white;
        }
   

        private int convertProgress;
        private void Convert(string folderPath)
        {
            string folderName = Path.GetFileName(folderPath);
            string targetPath = folderPath.Replace(@"\",@"/") + "/" + folderName + ".anim";
            
            string[] paths = Directory
                .GetFiles(folderPath, "*")
                .Where(x => 
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(x);
                    if (!texture) return false;
                    SpriteImportMode spriteImportMode = texture.GetSpriteImportMode();
                    return !x.EndsWith(".meta") && texture && spriteImportMode != SpriteImportMode.Multiple;
                })
                .ToArray();
            if (paths.Length == 0) return;
            AnimationConvertInfo.Info info = new AnimationConvertInfo.Info();           
            info.loop = false;
            info.frameInfos = new AnimationConvertInfo.Info.FrameInfo[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {               
                var frameInfo = info.frameInfos[i];
                frameInfo = new AnimationConvertInfo.Info.FrameInfo()
                {
                    frame = i,
                    guid = AssetDatabase.AssetPathToGUID(paths[i]),                 
                    
                };
                info.frameInfos[i] = frameInfo;
                
            }
            configInfo.AnimationConvertInfo.info_Values[targetPath] = info;

        }

    }
}
#endif