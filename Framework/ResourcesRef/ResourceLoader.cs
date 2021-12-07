using HuaFramework.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.ResourcesManager
{
    /// <summary>
    /// 资源管理类细分，存储(引用)通过此类直接加载或者从全局缓存拿过来的资源
    /// </summary>
    public class ResourceLoader
    {
        /// <summary>
        /// 在本地引用记录中缓存加载过的资源,一个资源出现一次引用就加一
        /// </summary>
        private List<ResData> _resourceLoaded = new List<ResData>();
        public static string ResourcesAssetsPrefix = "Resources://";

        #region API

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public T LoadAssetsSync<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetSyncLoadHandle<T>(assetPath);
        }

        /// <summary>
        /// 同步加载AB包里的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public T LoadAssetsSync<T>(string assetBundleName, string assetPath) where T : UnityEngine.Object
        {

            return AssetSyncLoadHandle<T>(assetPath, assetBundleName);
        }



        public void LoadAssetsAsync<T>(string assetBundleName,string assetPath, Action<T> onLoad) where T : UnityEngine.Object
        {
            AssetAsyncLoadHandle<T>(assetPath,onLoad,assetBundleName);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void LoadAssetsAsync<T>(string assetPath, Action<T> onLoad) where T : UnityEngine.Object
        {
            AssetAsyncLoadHandle<T>(assetPath, onLoad);
        }
     

        public void ReleaseAssets<T>(T asset) where T : UnityEngine.Object
        {
            var data = _resourceLoaded.Find((item) => item.Name == asset.name);
            data.ReleaseAssets();
        }

        public void ReleaseAllAssets()
        {
            _resourceLoaded.ForEach((item) =>
            {
                //卸载资源
                item.ReleaseAssets();
            });
            _resourceLoaded.Clear();
        }

        #endregion


        #region private

        /// <summary>
        /// 创建资源数据类
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private ResData CreateAssetDataBase(string assetPath, string assetBundleName = null)
        {
            var asset = AssetDataFactory.CreatAssetData(assetBundleName, assetPath);
            AddCustomRecord(asset);
            ResManager.Instance.AssetDatas.Add(asset);
            return asset;
        }

        /// <summary>
        /// 第一次加载资源或者从全局资源中引用，在本地引用记录中缓存,并且增加引用计数
        /// </summary>
        private void AddCustomRecord(ResData asset)
        {
            _resourceLoaded.Add(asset);
            asset.RetainAssets();
        }

        /// <summary>
        /// 从缓存池中查找该物体
        /// </summary>
        /// <param name="assetPath">资源路径,路由机制，此处用assetPath而不能是Name</param>
        /// <returns></returns>
        private ResData FindAssetCustom(string assetPath)
        {
            //先从局部缓存寻找
            var asset = _resourceLoaded.Find((item) => item.AssetPath == assetPath);
            if (asset == null)
            {
                //再从全局缓存池找
                asset = Managers.ResManager.Instance.AssetDatas.Find((item) => item.AssetPath == assetPath);
                if (asset != null)
                {
                    AddCustomRecord(asset);
                }
            }
            return asset;
        }

        /// <summary>
        /// 资源加载处理
        /// </summary>
        private T AssetSyncLoadHandle<T>(string assetPath, string assetBundleName = null) where T : UnityEngine.Object
        {
            //先从缓存池找
            var asset = FindAssetCustom(assetPath);
            if (asset != null)
            {
                if (asset.AssetState != AssetState.Loaded)
                {
                    throw new Exception(string.Format("在异步加载资源:{0}时尝试同步加载此资源", assetPath));
                }
                else
                {
                    return asset.Asset as T;
                }
            }
            //缓存中没有此资源
            //资源加载部分
            asset = CreateAssetDataBase(assetPath, assetBundleName);
            asset.LoadSync();
            return asset.Asset as T;
        }

        private void AssetAsyncLoadHandle<T>(string assetPath, Action<T> onLoad, string assetBundleName = null) where T : UnityEngine.Object
        {
            //先从缓存池找
            var asset = FindAssetCustom(assetPath);
            Action<ResData> OnLoadCallBack = null;
            OnLoadCallBack = data =>
            {
                onLoad?.Invoke(data.Asset as T);
                asset.UnRegisterOnLoadedEvent(OnLoadCallBack);
            };
            //池子没有就加载
            if (asset == null)
            {
                //资源加载部分
                asset = CreateAssetDataBase(assetPath, assetBundleName);
                asset.RegisterOnLoadedEvent(OnLoadCallBack);
                asset.LoadAsync();
            }
            else
            {
                if (asset.AssetState == AssetState.Loaded)
                    onLoad?.Invoke(asset.Asset as T);
                //资源正在加载,注册回调进去，即等待资源加载完毕操作
                else
                {
                    asset.RegisterOnLoadedEvent(OnLoadCallBack);
                }
            }
        }

        #endregion
    }

}
