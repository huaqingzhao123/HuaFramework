using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.ResourcesRef
{
    public class ResourceAssetData : ResData
    {
        public ResourceAssetData(string assetPath)
        {
            AssetPath = assetPath;
            //名字为资源路径
            Name = assetPath.Substring(ResourceLoader.ResourcesAssetsPrefix.Length);
            AssetState = AssetState.Waitting;
        }
        public override bool LoadSync()
        {
            AssetState = AssetState.Loading;
            Asset = Resources.Load(Name);
            AssetState = AssetState.Loaded;
            return Asset;
        }
        public override void LoadAsync()
        {
            var requeset = Resources.LoadAsync(Name);
            AssetState = AssetState.Loading;
            requeset.completed += (asset) =>
            {
                Asset = requeset.asset;
                AssetState = AssetState.Loaded;
            };
        }
        protected override void OnReleaseResources()
        {
            Managers.ResManager.Instance.AssetDatas.Remove(this);
            Resources.UnloadAsset(Asset);
            Asset = null;
        }
    }

}
