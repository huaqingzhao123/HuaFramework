using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace HuaFramework.ResourcesRef
{
    /// <summary>
    /// 编辑器模拟模式时不走AB包，使用
    /// </summary>
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
