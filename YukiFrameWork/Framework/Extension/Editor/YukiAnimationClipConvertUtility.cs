///=====================================================
/// - FileName:      YukiAnimationClipConvertUtility.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/5/30 12:46:05
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 转换AnimationClip所需要的设置接口
    /// </summary>
    public interface IConvertAnimationClipSetting
    {
        /// <summary>
        /// 对于添加的Sprite，判断是否能够添加进AnimationClip作为关键帧，当Condition返回False则忽略
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        bool Condition(Sprite sprite);
        /// <summary>
        /// 获取该Sprite所在的时间帧
        /// </summary>
        /// <param name="sprite">Sprite</param>
        /// <param name="index">所处数组的下标</param>
        /// <param name="length">数组的总长</param>
        /// <returns></returns>
        float GetFrameRate(Sprite sprite, int index,int length);
        /// <summary>
        /// 生成的AnimationClip是否开启Loop
        /// </summary>
        bool Loop { get; }
        /// <summary>
        /// 生成的AnimationClip的cycleOffset
        /// </summary>
        float CycleOffset { get; }
    }

    public class DefaultAnimationClipSetting : IConvertAnimationClipSetting
    {
        public bool Loop => false;

        public float CycleOffset => 0;

        public bool Condition(Sprite sprite)
        {
            return true;
        }

        public float GetFrameRate(Sprite sprite, int index,int length)
        {
            return index;
        }
    }
    public static class YukiAnimationClipConvertUtility 
    {
        //通过获取Texture2D获取到Sprite切割类型

        public static SpriteImportMode GetSpriteImportMode(this Texture2D texture)
        {
            TextureImporter textureImporter = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            if (!textureImporter) throw new NullReferenceException("TextureImporter丢失，请检查传递的Texture是否是资源文件!");
            return textureImporter.spriteImportMode;
        }

        //通过Texture2D获取到所有的Sprite

        public static Sprite[] GetSprites(this Texture2D texture)
        {
            return AssetDatabase
                .LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture))
                .Where(x => x is Sprite)
                .Select(x => x as Sprite)
                .ToArray();
        }

        //通过切割类型为Multiple的Texture2D转换成AnimationClip

        public static AnimationClip MultipleTextureConvertAnimationClip(this Texture2D texture,IConvertAnimationClipSetting convertAnimationClip)
        {
            SpriteImportMode spriteImportMode = texture.GetSpriteImportMode();
            if (spriteImportMode != SpriteImportMode.Multiple)
                throw new InvalidCastException("无法转换AnimationClip,传递的Texture2D图片裁剪类型并非Multiple");
            Sprite[] sprites = texture.GetSprites().Where(x => convertAnimationClip.Condition(x)).ToArray();
            return CreateAnimationClip(convertAnimationClip, sprites);           
        }

        //通过切割类型为Multiple的Texture2D转换成AnimationClip并生成资产文件

        public static void MultipleTextureConvertAnimationClipAndCreateAsset(this Texture2D texture, IConvertAnimationClipSetting convertAnimationClip,string path)
        {
            AnimationClip animationClip = MultipleTextureConvertAnimationClip(texture, convertAnimationClip);
            AssetDatabase.CreateAsset(animationClip, path);
            AssetDatabase.Refresh();
        }
        //通过传递的所有Sprite转换成AnimationClip并生成资产文件
        public static void CreateAnimationClipAndCreateAsset(IConvertAnimationClipSetting convertAnimationClip, Sprite[] sprites, string path)
        {
            AnimationClip animationClip = CreateAnimationClip(convertAnimationClip, sprites);
            AssetDatabase.CreateAsset(animationClip, path);
            AssetDatabase.Refresh();
        }
        //转换AnimationClip
        public static AnimationClip CreateAnimationClip(IConvertAnimationClipSetting convertAnimationClip,Sprite[] sprites)
        {            
            AnimationClip animationClip = new AnimationClip();
            int length = sprites.Length;
            ObjectReferenceKeyframe[] objectReferenceKeyframes = new ObjectReferenceKeyframe[length];
            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };
            animationClip.frameRate = length;
            AnimationClipSettings setting = AnimationUtility.GetAnimationClipSettings(animationClip);
            setting.loopTime = convertAnimationClip.Loop;
            setting.cycleOffset = convertAnimationClip.CycleOffset;
            AnimationUtility.SetAnimationClipSettings(animationClip, setting);
            for (int i = 0; i < length; i++)
            {
                 
                ObjectReferenceKeyframe objectReferenceKeyframe = new ObjectReferenceKeyframe()
                {
                    time = convertAnimationClip.GetFrameRate(sprites[i],i,length) / length,
                    value = sprites[i],
                };
                objectReferenceKeyframes[i] = objectReferenceKeyframe;

            }           
            AnimationUtility.SetObjectReferenceCurve(animationClip, spriteBinding, objectReferenceKeyframes);
            return animationClip;
        }

    }
}
#endif