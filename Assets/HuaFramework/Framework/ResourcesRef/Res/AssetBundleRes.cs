using HuaFramework.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HuaFramework.ResourcesRef
{
    public class AssetBundleRes : ResData
    {
        public AssetBundleRes(string assetPath)
        {
            //路径为名字
            AssetPath = assetPath;
            //名字也为名字
            Name = assetPath;
            AssetState = AssetState.Waitting;
        }
        public AssetBundle AssetBundle
        {

            get { return Asset as AssetBundle; }
            set { Asset = value; }
        }
        /// <summary>
        /// 存储该ab包的直接依赖
        /// </summary>
        private ResourceLoader _resourceLoader = new ResourceLoader();

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <returns>返回资源是否加载成功</returns>
        public override bool LoadSync()
        {
            AssetState = AssetState.Loading;
            //加载AssetBundle文件，只需要传入名字即可，由工具类组合路径
            var dependencyInfos = AssetBundleManifestData.Instace.GetDirectDependencyName(Name);
            foreach (var item in dependencyInfos)
            {
                //加载依赖，所有依赖只负责加载直接引用的依赖，本质为递归操作
                _resourceLoader.LoadAssetsSync<AssetBundle>(item);
            }
            //不是SimulationMode状态再真正加载AssetBundle资源
            if (!ResManager.IsSimulationModeLogic)
            {
                AssetBundle = AssetBundle.LoadFromFile(HotResUtil.GetOneAssetBundlePath(Name));
            }
            AssetState = AssetState.Loaded;
            return AssetBundle;
        }
        public override void LoadAsync()
        {
            AssetState = AssetState.Loading;
            AsyncLoadDependency(() =>
            {
                //是SimulationMode不加载AssetBundle
                if (ResManager.IsSimulationModeLogic)
                {
                    AssetState = AssetState.Loaded;
                }
                else
                {
                    var requeset = AssetBundle.LoadFromFileAsync(HotResUtil.GetOneAssetBundlePath(Name));
                    requeset.completed += (asset) =>
                    {
                        AssetBundle = requeset.assetBundle;
                        AssetState = AssetState.Loaded;
                    };
                }
            });
        }

        /// <summary>
        /// 异步加载依赖
        /// </summary>
        private void AsyncLoadDependency(Action onLoad)
        {
            int count = 0;
            //异步加载依赖
            var dependencyInfos = AssetBundleManifestData.Instace.GetDirectDependencyName(Name);
            if (dependencyInfos.Length == 0)
            {
                if(onLoad!=null)
                    onLoad.Invoke();
            }
            foreach (var item in dependencyInfos)
            {
                //加载依赖，所有依赖只负责加载直接引用的依赖，本质为递归操作
                _resourceLoader.LoadAssetsAsync<AssetBundle>(item, (assetBundle) =>
                {
                    count++;
                    //所有依赖加载完毕,调用回调
                    if (count == dependencyInfos.Length)
                    {
                        if (onLoad != null)
                            onLoad.Invoke();
                    }
                });
            }
        }
        protected override void OnReleaseResources()
        {
            if (AssetBundle != null)
            {
                Managers.ResManager.Instance.AssetDatas.Remove(this);
                _resourceLoader.ReleaseAllAssets();
                AssetBundle.Unload(true);
                AssetBundle = null;
                _resourceLoader = null;
            }
        }
    }
}
