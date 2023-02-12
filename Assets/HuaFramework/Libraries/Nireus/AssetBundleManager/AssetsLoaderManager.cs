using System.Collections;
using System.Collections.Generic;
using Nireus;
using UnityEngine;
using UnityEngine.U2D;

public class AssetsLoaderManager : Singleton<AssetsLoaderManager>
{
    public Dictionary<int,List<string>> _loaded_asset = new Dictionary<int, List<string>>();
    public Dictionary<int,Dictionary<string,UnityEngine.Object>> _asset_dic = new Dictionary<int, Dictionary<string, Object>>();
    
    public virtual void CacheLoad(int hashCode,string path)
    {
        if (!_loaded_asset.ContainsKey(hashCode))
        {
            _loaded_asset.Add(hashCode,new List<string>());
            _loaded_asset[hashCode].Add(path);
        }
        else
        {
            _loaded_asset[hashCode].Add(path);
        }
    }
    public T LoadAsset<T>(int hashCode,string assetPath) where T : UnityEngine.Object
    {
        UnityEngine.Object temp = AssetManager.Instance.loadSync<T>(assetPath);
        if (temp == null)
        {
            GameDebug.LogError($"{assetPath} is null");
            return null;
        }
        if (!_asset_dic.ContainsKey(hashCode))
        {
            _asset_dic.Add(hashCode,new Dictionary<string, Object>());
            _asset_dic[hashCode].Add(assetPath,temp);
        }
        else
        {
            if (!_asset_dic[hashCode].ContainsKey(assetPath))
            {
                _asset_dic[hashCode].Add(assetPath,temp);
            }
        }

        CacheLoad(hashCode,assetPath);
        return temp as T;
    }
    public void UnLoadAll(int hashCode)
    {
        ClearAsset(hashCode);
    }

    private void ClearAsset(int hashCode)
    {
        if (_loaded_asset.ContainsKey(hashCode))
        {
            for (int i = 0; i < _loaded_asset[hashCode].Count; i++)
            {
                AssetManager.Instance.UnloadAsset(_loaded_asset[hashCode][i]);
            }
            _loaded_asset[hashCode].Clear();
            _loaded_asset.Remove(hashCode);
        }

        if (_asset_dic.ContainsKey(hashCode))
        {
            _asset_dic.Remove(hashCode);
        }
    }

    public Sprite LoadAtlasAsset(int hashCode,string atlasName,string spriteName)
    {
        SpriteAtlas _sprite_atlas= LoadAsset<SpriteAtlas>(hashCode,atlasName);
        return _sprite_atlas.GetSprite(spriteName);
    }
}
