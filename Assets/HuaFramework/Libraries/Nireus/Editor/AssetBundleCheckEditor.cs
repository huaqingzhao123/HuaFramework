using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Nireus;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class AssetBundleCheckEditor : OdinEditorWindow
{
    
    public static AssetBundleCheckEditor OpenWindow()
    {
        AssetBundleCheckEditor window = GetWindow<AssetBundleCheckEditor>();
        window.Show();
        //data = obj;
        return window;
    }
    //当前内存里的Bundle
    public Dictionary<string, AssetBundleObject> _asset_bundle_map;
    //缓存Bundle的字典(用于比较)
    public Dictionary<string, AssetBundleObject> _cache_asset_bundle_map = new Dictionary<string, AssetBundleObject>();
    //对比Bundle字典
    public Dictionary<string, AssetBundleObject> _compare_asset_bundle_map = new Dictionary<string, AssetBundleObject>();
    //检查错误的Bundle(依赖为0的)
    public Dictionary<string, AssetBundleObject> _error_asset_bundle_map = new Dictionary<string, AssetBundleObject>();
    //原来存在  现在被卸载的Bundle
    //即_cache_asset_bundle_map中存在的  _asset_bundle_map中不存在的
    public Dictionary<string, AssetBundleObject> _older_asset_bundle_map = new Dictionary<string, AssetBundleObject>();
    [Button(ButtonSizes.Large)]
    public void Refresh()
    {
        if (Application.isPlaying)
        {
            _asset_bundle_map = AssetManager.Instance.GetAssetBundleObjectDic();    
        }
        
    }

    [Button(ButtonSizes.Large)]
    public void Cache()
    {
        if (Application.isPlaying)
        {
            _cache_asset_bundle_map.Clear();
            foreach (var data in _asset_bundle_map)
            {
                AssetBundleObject obj = new AssetBundleObject();
                obj.ab_name = data.Value.ab_name;
                obj.ref_count = data.Value.ref_count;
                _cache_asset_bundle_map.Add(data.Key,obj);   
            }
            //_cache_asset_bundle_map = new Dictionary<string, AssetBundleObject>(_asset_bundle_map);    
        }
    }
    [Button(ButtonSizes.Large)]
    public void Compare()
    {
        _compare_asset_bundle_map.Clear();
        foreach (var data in _asset_bundle_map)
        {
            if (_cache_asset_bundle_map.ContainsKey(data.Key))
            {
                AssetBundleObject obj = _cache_asset_bundle_map[data.Key];
                if (obj.ref_count != data.Value.ref_count)
                {
                    _compare_asset_bundle_map.Add(data.Key,data.Value);
                }
            }
            else
            {
                _compare_asset_bundle_map.Add(data.Key,data.Value);
            }
        }
    }
    
    [Button(ButtonSizes.Large)]
    public void CompareNew()
    {
        _compare_asset_bundle_map.Clear();
        foreach (var data in _asset_bundle_map)
        {
            if (_cache_asset_bundle_map.ContainsKey(data.Key))
            {
               
            }
            else
            {
                _compare_asset_bundle_map.Add(data.Key,data.Value);
            }
        }
    }

    [Button(ButtonSizes.Large)]
    public void Check()
    {
        _error_asset_bundle_map.Clear();
        foreach (var data in _asset_bundle_map)
        {
            if (data.Value.ref_count == 0)
            {
                AssetBundleObject obj = new AssetBundleObject();
                obj.ab_name = data.Value.ab_name;
                obj.ref_count = data.Value.ref_count;
                _error_asset_bundle_map.Add(data.Key,obj);
            }
        }
    }

    [Button(ButtonSizes.Large)]
    public void CheckOlder()
    {
        _older_asset_bundle_map.Clear();
        foreach (var data in _cache_asset_bundle_map)
        {
            if (_asset_bundle_map.ContainsKey(data.Key))
            {
                AssetBundleObject obj = _cache_asset_bundle_map[data.Key];
                if (obj.ref_count != data.Value.ref_count)
                {
                    _older_asset_bundle_map.Add(data.Key,data.Value);
                }
            }
            else
            {
                _older_asset_bundle_map.Add(data.Key,data.Value);
            }
        }
    }
    
}

public class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICloneable
{
    public object Clone()
    {
        BinaryFormatter Formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));
        MemoryStream stream = new MemoryStream();
        Formatter.Serialize(stream, this);
        stream.Position = 0;
        object clonedObj = Formatter.Deserialize(stream);
        stream.Close();
        return clonedObj;
    }


    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }

    public int Count { get; }
    public bool IsReadOnly { get; }
    public void Add(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(TKey key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(TKey key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        throw new NotImplementedException();
    }

    public TValue this[TKey key]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public ICollection<TKey> Keys { get; }
    public ICollection<TValue> Values { get; }
}
