#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nireus
{
    public class AssetsVersionInfo
    {
        public string version;
        public Dictionary<string, AssetBundleInfo> assets;

        public static AssetsVersionInfo CreateFromString(string jsonString)
        {
            return JsonConvert.DeserializeObject<AssetsVersionInfo>(jsonString);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }

    public class AssetBundleInfo
    {
        public long size;
        public string md5;
    }
}
#endif
