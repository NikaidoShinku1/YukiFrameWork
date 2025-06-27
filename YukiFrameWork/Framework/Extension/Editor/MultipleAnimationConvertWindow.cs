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

#if UNITY_EDITOR
using YukiFrameWork.Extension;
using Sirenix.OdinInspector.Editor;


using UnityEditor;
namespace YukiFrameWork
{
    public class MultipleAnimationConvertWindow : OdinEditorWindow
    {
        private FrameworkConfigInfo configInfo;
       
        protected override void OnEnable()
        {
            base.OnEnable();
            configInfo = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
            texture = configInfo.multipleAnimationConvertInfo.texture;
            multipleAnimationConvertInfo = configInfo.multipleAnimationConvertInfo; 
           
        }
        [Title("Sprite-AnimationClip转换工具 Multiple",TitleAlignment = TitleAlignments.Left)]
        //[ValueDropdown(nameof(allMultipleTexture))]
        [InfoBox("Multiple默认情况下仅支持单个Texture的修改")]
        [OnValueChanged(nameof(OnValueChanged)),BoxGroup()]
        public Texture2D texture;
        [ShowIf(nameof(Check))]
        public MultipleAnimationConvertInfo multipleAnimationConvertInfo;
        private bool Check()
        {
            try
            {
                return texture && texture.GetSpriteImportMode() == SpriteImportMode.Multiple;
            }
            catch { return false; }
        }

        private void OnValueChanged(Texture2D texture) 
        {
          
            this.texture = texture;
            multipleAnimationConvertInfo.texture = texture;
            if (this.texture)
            {
                Refresh();

            }
        }
        [Button("刷新并清空数据"),BoxGroup()]
        void Refresh()
        {
            if(texture)
            multipleAnimationConvertInfo.sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this.texture))
                   .Where(x => x is Sprite)
                   .Select(x => x as Sprite).ToList();
            multipleAnimationConvertInfo.multipleInfos = new MultipleAnimationConvertInfo.MultipleInfo[0];
            Save();
        }
        private void Save()
        {
            multipleAnimationConvertInfo.texture = texture;
            configInfo.multipleAnimationConvertInfo = multipleAnimationConvertInfo;
        }

        private void Load()
        {
            texture = multipleAnimationConvertInfo.texture;
            multipleAnimationConvertInfo = configInfo.multipleAnimationConvertInfo;
        }
        protected override void OnAfterDeserialize() 
        {                      
            base.OnAfterDeserialize();
            if (!configInfo)
                configInfo = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
            Save();
        }
        protected override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            if(!configInfo)
                configInfo = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
            Load();
           
        }

        [Button("构建并生成",ButtonHeight = 40),ShowIf(nameof(Check))]
        void Generator()
        {
            if (configInfo.multipleAnimationConvertInfo.multipleInfos == null && configInfo.multipleAnimationConvertInfo.multipleInfos.Length == 0) return;
            if (texture.GetSpriteImportMode() != SpriteImportMode.Multiple) return;
            YukiAssetDataBase.GetAssetPath(texture, out string dir, out _, out _);
            int length = configInfo.multipleAnimationConvertInfo.multipleInfos.Length;
            float progress = 0;
            foreach (var item in configInfo.multipleAnimationConvertInfo.multipleInfos)
            {
                if (string.IsNullOrEmpty(item.clipName)) continue;
                string path = $"{dir}/{item.clipName}.anim";
                
                Sprite[] sprites = item.multipleFrameInfos.Select(x => x.sprite).ToArray();

                YukiAnimationClipConvertUtility.CreateAnimationClipAndCreateAsset(new DefaultMultipleConvertAnimationClipSetting() 
                {
                    multipleInfo = item
                },sprites,path);
                EditorUtility.DisplayProgressBar("构建AnimationClip", $"正在根据所有的预制信息生成AnimationClip ---进度:{progress}/{length}", progress / length);
                progress++;

            }
            EditorUtility.ClearProgressBar();
        }

        [Button("文档官网"),LabelWidth(30),PropertySpace(25)]
        void OpenUrl()
        {
            Application.OpenURL(@"https://gitee.com/NikaidoShinku/YukiFrameWork/blob/master/YukiFrameWork/Framework/Extension/17.AnimationClip转换.md");
        }
        public class DefaultMultipleConvertAnimationClipSetting : IConvertAnimationClipSetting
        {
            public MultipleAnimationConvertInfo.MultipleInfo multipleInfo;
            public bool Loop => multipleInfo.loop;

            public float CycleOffset => multipleInfo.cycleOffset;

            public bool Condition(Sprite sprite)
            {
                return true;
            }

            public float GetFrameRate(Sprite sprite, int index, int length)
            {
                return multipleInfo.multipleFrameInfos[index].frame;
            }
        }
    }
}
#endif