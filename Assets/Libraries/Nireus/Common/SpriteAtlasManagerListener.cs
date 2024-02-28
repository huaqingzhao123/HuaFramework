using System;
using System.Collections;
using System.Collections.Generic;
using Nireus;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasManagerListener : MonoBehaviour
{

    private void OnEnable()
    {
        SpriteAtlasManager.atlasRequested += OnAtlasRequested;
    }

    private void OnDisable()
    {
        SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
    }
    
    private void OnAtlasRequested(string atlasName, Action<SpriteAtlas> action)
    {
        var path = GetPathWithAtlasName(atlasName);
        var atlas = AssetManager.Instance.loadSync<SpriteAtlas>(path);
        
        action?.Invoke(atlas);
    }

    private string GetPathWithAtlasName(string atlasName)
    {
        string[] tagArray = atlasName.Split('_');
        if (tagArray.Length < 2 || tagArray[0] != "atlas")
        {
            throw new Exception("[SpriteAtlasManagerListener] Illegal SpriteAtlas Name.");
        }

        var path = PathConst.BUNDLE_RES_TEXTURES + "UI/";
        for (int i = 1; i < tagArray.Length; i++)
        {
            path += $"{tagArray[i]}/";
        }

        path += $"{atlasName}.spriteatlas";

        return path;
    }

}
