using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
//按文件配置数据
[CreateAssetMenu(menuName = "AssetChecker/TextureModifyByFileAssets", order =100)]
public class TextureModifyByFileAssets : SerializedScriptableObject
{
    public Dictionary<string,TextureModifyByFileData> DataDic = new Dictionary<string, TextureModifyByFileData>();
    
    public TextureModifyByFileData GetData(string key)
    {
        if (DataDic.ContainsKey(key))
        {
            return DataDic[key];
        }

        return null;
    }

    public void AddData(TextureModifyByFileData data)
    {
        if (DataDic.ContainsKey(data.Path))
        {
            Debug.LogError(data.Path + " is Exist");
        }
        else
        {
            DataDic.Add(data.Path,data);   
        }
    }

    public void RemoveData(string Path)
    {
        DataDic.Remove(Path);
    }
}
//缓存多有Texture数据
[CreateAssetMenu(menuName = "AssetChecker/TextureModifyDataAssets", order = 101)]
public class TextureModifyDataAssets : SerializedScriptableObject
{
    public List<TextureData> DataList = new List<TextureData>();   
}
//第一次导入时  按配置  设置参数
[CreateAssetMenu(menuName = "AssetChecker/TextureFirstImportDataAssets", order = 102)]
public class TextureFirstImportDataAssets : SerializedScriptableObject
{
    public List<string> DataList = new List<string>();

}