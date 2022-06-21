using HuaFramework.Managers;
using HuaFramework.Singleton;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HuaFramework.ResourcesRef
{

    public class AssetBundleManifestData : Singleton<AssetBundleManifestData>
    {

        private AssetBundleManifest _assetBundleManifest;
        private AssetBundleManifestData() { }
        //存储所有的AssetBundle
        public List<AssetBundleData> AllAssetBundles = new List<AssetBundleData>();


        public void Load()
        {
            if (ResManager.IsSimulationModeLogic)
            {
#if UNITY_EDITOR
                var allAssetBundle = UnityEditor.AssetDatabase.GetAllAssetBundleNames();
                foreach (var item in allAssetBundle)
                {
                    //获得此AssetBundle的所有资源的路径
                    var assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(item);
                    var assetBundleData = new AssetBundleData()
                    {
                        Name = item,
                        DependenciesAssetBundleNames = UnityEditor.AssetDatabase.GetAssetBundleDependencies(item, false)
                    };
                    foreach (var assetPath in assetPaths)
                    {
                        var asset = new AssetBundleElementData()
                        {
                            Name = assetPath.Split('/').Last().Split('.').First(),
                            ParentAssetBundleName = item
                        };
                        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                        //将该AssetBundle下的所有资源数据缓存
                        assetBundleData.AssetBundleElementDatas.Add(asset);
                    }
                    //缓存起所有AssetBundle
                    AllAssetBundles.Add(assetBundleData);
                }
                AllAssetBundles.ForEach(assetBundle =>
                {
                    assetBundle.AssetBundleElementDatas.ForEach(asset =>
                    {
                        Debug.LogFormat("assetBundle:{0}包含资源:{1}", assetBundle.Name, asset.Name);
                    });
                    foreach (var item in assetBundle.DependenciesAssetBundleNames)
                    {
                        Debug.LogFormat("assetBundle:{0}依赖AB包:{1}", assetBundle.Name, item);
                    }
                });

#endif
            }
            else
            {
                var streamingAsset = AssetBundle.LoadFromFile(HotResUtil.GetOneAssetBundlePath(HotResUtil.GetPlatformName()));
                _assetBundleManifest = streamingAsset.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
        }


        public string[] GetDirectDependencyName(string assetBundleName)
        {
            if (ResManager.IsSimulationModeLogic)
            {
                return AllAssetBundles
                    .Find((assetBundle) => assetBundle.Name == assetBundleName)
                    .DependenciesAssetBundleNames;
            }
            return _assetBundleManifest.GetDirectDependencies(assetBundleName);
        }
    }

}
