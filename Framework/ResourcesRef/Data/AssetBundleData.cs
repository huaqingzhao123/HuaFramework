using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace HuaFramework.ResourcesRef
{
    public class AssetBundleData 
    {
        public string Name;
        /// <summary>
        /// AssetBundle包内的所有资源
        /// </summary>
        public List<AssetBundleElementData> AssetBundleElementDatas = new List<AssetBundleElementData>();
        /// <summary>
        /// 依赖的AssetBundle
        /// </summary>
        public string[] DependenciesAssetBundleNames;
    }

}
