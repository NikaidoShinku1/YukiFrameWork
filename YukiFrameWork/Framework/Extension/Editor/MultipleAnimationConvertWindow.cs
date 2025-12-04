///=====================================================
/// - FileName:      MultipleAnimationConvertWindow.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/5/30 14:27:57
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections.Generic;
using System.IO;



#if UNITY_EDITOR
using YukiFrameWork.Extension;
using Sirenix.OdinInspector.Editor;
using UnityEditor.IMGUI.Controls;


using UnityEditor;
namespace YukiFrameWork
{
    public class MultipleAnimationClipParamListTree : TreeView
    {
        private FrameworkConfigInfo configInfo;
        private Texture2D texture;

        #region 重写方法
        protected override TreeViewItem BuildRoot()
        {

            TreeViewItem root = new TreeViewItem(-1, -1);

            if (texture)
            {
                AddChildren(root);
            }

            return root;
        }

        private int index;

        private void AddChildren(TreeViewItem item)
        {

            Sprite[] sprites = texture.GetSprites();

            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];

                if (!sprite) continue;
             
                string displayName = sprite.name;
                TreeViewItem subItem = new TreeViewItem(index++, 0, displayName);
                item.AddChild(subItem);

            }

            configInfo.multipleAnimationConvertInfo.sprites = sprites.ToList();
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
                Sprite sprite = configInfo.multipleAnimationConvertInfo.sprites[id];

                if (!CheckSprite(sprite))
                {
                    var info = configInfo.multipleAnimationConvertInfo.infos.FirstOrDefault(x => x.IsSceleted);
                    if (info == null)
                        return;
                    info.frameInfos.Add(new MultipleAnimationConvertInfo.Info.FrameInfo() { frame = info.frameInfos.Count, sprite = sprite });
                }
                
            }
           

            EditorUtility.SetDirty(configInfo);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return base.CanMultiSelect(item);
        }

        private bool CheckSprite(Sprite sprite)
        {
            var info = configInfo.multipleAnimationConvertInfo.infos.FirstOrDefault(x => x.IsSceleted);
            if (info == null) return false;
            foreach (var item in info.frameInfos)
            {
                if(item.sprite==sprite)
                    return true;
            }
            return false;
        }


        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            var info = configInfo.multipleAnimationConvertInfo.infos.FirstOrDefault(x => x.IsSceleted);
            if (info == null) return;
            if (info.frameInfos.Count == 0) return;
            foreach (var item in info.frameInfos)
            {
                if(args.label == item.sprite.name)
                    GUI.Label(args.rowRect, "√");
            }
        }

        #endregion

        #region 方法
        public MultipleAnimationClipParamListTree(TreeViewState state, FrameworkConfigInfo configInfo,Texture2D texture) : base(state)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.configInfo = configInfo;
            this.texture = texture;

        }


        #endregion



    }
    public class MultipleAnimationClipConvertSearenchWindow : PopupWindowContent
    {

        #region 字段
        private FrameworkConfigInfo configInfo;
        private Texture2D texture;
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
        private MultipleAnimationClipParamListTree paramTree;
        private TreeViewState paramState;
        private Rect paramRect;

        #endregion


        public MultipleAnimationClipConvertSearenchWindow(float width, float height, FrameworkConfigInfo configInfo,Texture2D texture)
        {
            this.width = width;
            this.height = height;
            this.configInfo = configInfo;
            this.texture = texture;
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

                paramTree = new MultipleAnimationClipParamListTree(paramState, configInfo,texture);
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
            string path = texture.name;
            EditorGUI.LabelField(labelRect, string.IsNullOrEmpty(path) ? "全部显示" : path, GUI.skin.GetStyle("AC BoldHeader"));

            // 参数列表 


            paramRect.Set(rect.x, rect.y + searchHeight + labelHeight - 5, rect.width, rect.height - searchHeight - labelHeight);
            paramTree.OnGUI(paramRect);

        }

    }
    public class MultipleAnimationConvertWindow : MEditorWindow
    {
        private FrameworkConfigInfo configInfo;

        private GUIStyle titleStyle;
        private GUIStyle subTitleStyle;
        private GUIStyle desStyle;
        private Vector2 scrollView;
        private Texture2D texture
        {
            get
            {
                return configInfo.multipleAnimationConvertInfo.texture;
            }
            set
            {
                configInfo.multipleAnimationConvertInfo.texture = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            configInfo = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));

        }
        protected override void OnGUI()
        {
            base.OnGUI();

            if (titleStyle == null)
            {
                ReLoad();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Multiple Sprite-AnimationClip转换工具", titleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Single");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(15);
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Texture的Mode必须是Multiple类型，否则不起作用\n注意:切割的图片每一个名字也必须唯一", MessageType.Warning);
            EditorGUILayout.LabelField("设定图片",GUILayout.Width(60));
            texture  = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), true);
            if (GUILayout.Button("清空", GUILayout.Width(40)))
            {
                texture = null;
                configInfo.multipleAnimationConvertInfo.infos.Clear();
                EditorUtility.SetDirty(configInfo);
            }
            EditorGUILayout.EndHorizontal();
          
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("文档官网", GUILayout.Height(50), GUILayout.Width(100)))
            {
                Application.OpenURL(@"https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Extension/17.AnimationClip转换.md");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("创建一个新的分组", GUILayout.Width(120)))
            {
                var item = new MultipleAnimationConvertInfo.Info();
                if (configInfo.multipleAnimationConvertInfo.infos.Count == 0)
                    item.IsSceleted = true;
                configInfo.multipleAnimationConvertInfo.infos.Add(item);               
            }
            EditorGUILayout.EndHorizontal();          
            if (texture)
            {
                if (texture.GetSpriteImportMode() != SpriteImportMode.Multiple)
                {
                   // texture = null;
                    EditorGUILayout.HelpBox("精灵并非Multiple类型!", MessageType.Error);

                }
                else if(configInfo.multipleAnimationConvertInfo.infos.Count > 0)
                {
                    var whe = EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("添加精灵"))
                    {
                        whe.x += whe.width / 2;
                        PopupWindow.Show(whe, new MultipleAnimationClipConvertSearenchWindow(whe.width / 2, 200, configInfo, texture));
                    }
                    if (GUILayout.Button("添加精灵(该图集全部)"))
                    {
                        var info = configInfo.multipleAnimationConvertInfo.infos.FirstOrDefault(x => x.IsSceleted);

                        if (info == null)
                        {
                            Debug.LogError("当前没有选中图集");
                        }
                        else
                        {
                            var sprites = texture.GetSprites();
                            foreach (var sprite in sprites)
                            {                               
                                if (!CheckSprite(sprite))
                                {                                  
                                    info.frameInfos.Add(new MultipleAnimationConvertInfo.Info.FrameInfo() { frame = info.frameInfos.Count, sprite = sprite });
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
            }
            scrollView = EditorGUILayout.BeginScrollView(scrollView, GUI.skin.box);
            MultipleAnimationConvertInfo.Info deleteInfo = null;
            foreach (var info in configInfo.multipleAnimationConvertInfo.infos)
            {
                EditorGUILayout.BeginVertical("OL Box");

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("选中这个分组", GUILayout.Width(90));
                bool value = info.IsSceleted;
                info.IsSceleted = EditorGUILayout.Toggle(info.IsSceleted);

                if (!value && info.IsSceleted)
                {
                    foreach (var normalInfo in configInfo.multipleAnimationConvertInfo.infos)
                    {
                        if (normalInfo == info) continue;
                        normalInfo.IsSceleted = false;
                    }
                }

                GUILayout.FlexibleSpace();
                GUI.color = Color.red;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (deleteInfo == null)
                        deleteInfo = info;
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                foreach (var frameInfo in info.frameInfos)
                {
                    EditorGUILayout.Space(15);
                    if (!frameInfo.sprite) continue;
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUI.color = Color.yellow;
                        GUILayout.Label("精灵名称", GUILayout.Width(75));
                        GUILayout.Label(frameInfo.sprite.name);

                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("时间帧", GUILayout.Width(40));
                        frameInfo.frame = EditorGUILayout.FloatField(frameInfo.frame, GUILayout.Width(25));
                        EditorGUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("预览图", GUILayout.Width(50));
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(50));
                        EditorGUILayout.ObjectField(frameInfo.sprite,typeof(Sprite),true,GUILayout.Height(50));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(5);
                    }
                }

                if (info.frameInfos.Count > 0)
                {
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
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("设定动画名称",GUILayout.Width(120));
                    info.animName = EditorGUILayout.TextField(info.animName,GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();

            }
            
            EditorGUILayout.EndScrollView();
            if ((configInfo.multipleAnimationConvertInfo.infos.Count != 0) && texture && GUILayout.Button("构建全部AnimationClip", GUILayout.Height(25)))
            {
                float progress = 0;

                foreach (var item in configInfo.multipleAnimationConvertInfo.infos)
                {
                    if (item.animName.IsNullOrEmpty())
                    {
                        Debug.LogWarning("已自动过滤没有填写动画名称的分组");
                        continue;
                    }
                    progress++;

                    string importPath = AssetDatabase.GetAssetPath(texture);
                    string directoryPath = Path.GetDirectoryName(importPath);
                    Debug.Log(directoryPath);
                    
                    string targetPath = directoryPath + "/" + item.animName  + ".anim";
                    if (File.Exists(targetPath))
                    {
                        Debug.LogWarning("已存在同名动画，自动过滤 AnimName:" + item.animName);
                        continue;
                    }
                    Debug.Log(targetPath);
                   Sprite[] sprites = item.frameInfos.Select(x => x.sprite).ToArray();
                    foreach (var i in  sprites)
                    {
                        Debug.Log(i);
                    }
                   YukiAnimationClipConvertUtility.CreateAnimationClipAndCreateAsset(new MultipleTexture2DConvertSetting(item),sprites,targetPath);
                   EditorUtility.DisplayProgressBar("构建AnimationClip", $"正在根据所有的预制信息生成AnimationClip ---进度:{progress}/{configInfo.AnimationConvertInfo.info_Values.Count}", progress / configInfo.AnimationConvertInfo.info_Values.Count);
                }

                EditorUtility.ClearProgressBar();
            }
            if (deleteInfo != null)
            {
                configInfo.multipleAnimationConvertInfo.infos.Remove(deleteInfo);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(configInfo);
            }

        }

        private bool CheckSprite(Sprite sprite)
        {
            var info = configInfo.multipleAnimationConvertInfo.infos.FirstOrDefault(x => x.IsSceleted);
            if (info == null) return false;
            foreach (var item in info.frameInfos)
            {
                if (item.sprite == sprite)
                    return true;
            }
            return false;
        }

        private void ReLoad()
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
            if(!configInfo)
            configInfo = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
        }
        
        void OpenUrl()
        {
            Application.OpenURL(@"https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Extension/17.AnimationClip转换.md");
        }
      
    }

    public struct MultipleTexture2DConvertSetting : IConvertAnimationClipSetting
    {
        private List<MultipleAnimationConvertInfo.Info.FrameInfo> frameInfos;
        public MultipleTexture2DConvertSetting(MultipleAnimationConvertInfo.Info info)
        {
            this.frameInfos = info.frameInfos;
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
}
#endif