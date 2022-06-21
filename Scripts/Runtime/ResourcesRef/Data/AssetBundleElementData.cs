using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.ResourcesRef
{
    /// <summary>
    /// 编辑器模拟模式时不走AB包，使用
    /// </summary>
    public class AssetBundleElementData 
    {
        /// <summary>
        /// 所属AssetBundle的名字
        /// </summary>
        public string ParentAssetBundleName;
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name;
    }

}
