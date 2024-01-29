using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasPacking
{
     
    public static void CreateAtlasByAssetPath(string[] assets, string path, TextureImporterPlatformSettings platformSetting = null)
    {
        List<Object> objects = new List<Object>();

        foreach (var asset in assets)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(asset);

            if (sprite != null && sprite.rect.width < 512 && sprite.rect.height < 512)
            {
                // 如果这个图片本身有多子图片 说明它本身就是一个图集 不需要再打进图集中
                if (AssetDatabase.LoadAllAssetsAtPath(asset).Length == 2)
                    objects.Add(sprite); 
            }
        }

        if (objects.Count <= 1) return; // 不需要打包图集

        SpriteAtlas atlas = new SpriteAtlas();
        atlas.SetIncludeInBuild(true);

        SpriteAtlasPackingSettings packSet = new SpriteAtlasPackingSettings();
        packSet.blockOffset = 1;
        packSet.enableRotation = false;
        packSet.enableTightPacking = false;
        packSet.padding = 2;

        atlas.SetPackingSettings(packSet);

        SpriteAtlasTextureSettings textureSet = new SpriteAtlasTextureSettings();

        textureSet.readable = false;
        textureSet.generateMipMaps = false;
        textureSet.sRGB = true;
        textureSet.filterMode = FilterMode.Bilinear;

        atlas.SetTextureSettings(textureSet);

        atlas.Add(objects.ToArray());

        if (platformSetting == null)
            platformSetting = new TextureImporterPlatformSettings()
            {
                maxTextureSize = 2048,
                format = TextureImporterFormat.Automatic,
                crunchedCompression = true,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50,
            };
         
        atlas.SetPlatformSettings(platformSetting);

        AssetDatabase.CreateAsset(atlas, path);

        AssetDatabase.Refresh();


    }

}
