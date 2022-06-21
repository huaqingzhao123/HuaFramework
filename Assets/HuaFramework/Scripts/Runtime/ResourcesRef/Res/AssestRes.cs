using HuaFramework.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HuaFramework.ResourcesRef
{
    public class AssestRes : ResData
    {
        /// <summary>
        /// 直接传入Ab包的名字即可
        /// </summary>
        private string _mBundleName;
        public AssestRes(string assetPath, string bundleName)
        {
            //资源路径
            AssetPath = assetPath;
            Name = assetPath;
            AssetState = AssetState.Waitting;
            _mBundleName = bundleName;
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
            //先加载所属ab包,其实Simulation Mode状态下没有真正加载AB包
            var bundle = _resourceLoader.LoadAssetsSync<AssetBundle>(_mBundleName);
            if (ResManager.IsSimulationModeLogic)
            {
#if UNITY_EDITOR
                var path = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(_mBundleName, Name);
                Asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path[0]);
#endif
            }
            else
            {
                Asset = bundle.LoadAsset(Name);
            }
            AssetState = AssetState.Loaded;
            return Asset;


        }
        public override void LoadAsync()
        {
            AssetState = AssetState.Loading;

            //不管是不是SimulationMode状态都需要加载AssetBundle资源，只不过SimulationMode状态下是假装加载没有实际去加载文件
            //先加载bundle，加载完拿到该asset
            _resourceLoader.LoadAssetsAsync<AssetBundle>(_mBundleName, (bundle) =>
            {
                if (ResManager.IsSimulationModeLogic)
                {
#if UNITY_EDITOR
                    var path = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(_mBundleName, Name);
                    Asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path[0]);
                    AssetState = AssetState.Loaded;
#endif
                }
                else
                {
                    var assetRequeset = bundle.LoadAssetAsync(Name);
                    assetRequeset.completed += (operation) =>
                    {
                        Asset = assetRequeset.asset;
                        AssetState = AssetState.Loaded;
                    };
                }
            });
        }

        protected override void OnReleaseResources()
        {
            if (Asset != null)
            {
                if (Asset is GameObject)
                {

                }
                else
                {
                    Resources.UnloadAsset(Asset);
                }

                Managers.ResManager.Instance.AssetDatas.Remove(this);
                _resourceLoader.ReleaseAllAssets();
                Asset = null;
                _resourceLoader = null;
            }
        }
    }

}
