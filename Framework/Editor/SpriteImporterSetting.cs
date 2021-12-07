using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace HuaFramework {

    public class SpriteImporterSetting : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (assetPath.ToLower().Contains("image"))
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
            }
        }
    }

}

