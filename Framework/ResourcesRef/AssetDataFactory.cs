using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.ResourcesManager
{
    public class AssetDataFactory
    {
        /// <summary>
        /// 创建资源数据类
        /// </summary>
        /// <returns></returns>
        public static ResData CreatAssetData(string assetBundleName, string assetPath)
        {
            ResData asset;
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                asset = new AssestRes(assetPath, assetBundleName);
            }
            else
            {
                if (assetPath.StartsWith(ResourceLoader.ResourcesAssetsPrefix))
                {
                    asset = new ResourceAssetData(assetPath);
                }
                else
                {
                    asset = new AssetBundleRes(assetPath);
                }
            }
            return asset;
        }
    }

}
