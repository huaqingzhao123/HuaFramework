using System;
using System.Collections;
using System.Collections.Generic;
using Nireus;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public abstract class AssetsLoader:UIBehaviour
{
    public List<string> _loaded_asset = new List<string>();
    public Dictionary<string,UnityEngine.Object> _asset_dic = new Dictionary<string, Object>();
    public virtual void CacheLoad(string path)
    {
        _loaded_asset.Add(path);
    }

    public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
    {
        UnityEngine.Object temp = AssetManager.Instance.loadSync<T>(assetPath);
        if (temp == null)
        {
            GameDebug.LogError($"{assetPath} is null");
            return null;
        }
        if (!_asset_dic.ContainsKey(assetPath))
        {
            _asset_dic.Add(assetPath,temp);
        }
        _loaded_asset.Add(assetPath);
        return temp as T;
    }

    protected override void OnDestroy()
    {
        UnLoadAll();
    }
    public void UnLoadAll()
    {
        ClearAsset();
    }

    private void ClearAsset()
    {
        for (int i = 0; i < _loaded_asset.Count; i++)
        {
            if (AssetManager.Instance != null)
            {
                AssetManager.Instance.UnloadAsset(_loaded_asset[i]);
            }
        }
        _loaded_asset.Clear();

        _asset_dic.Clear();
    }
}
