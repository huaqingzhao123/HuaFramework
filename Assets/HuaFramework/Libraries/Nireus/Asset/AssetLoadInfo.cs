using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nireus
{
    public enum AssetLoadType
    {
        COMMON = 1,//常用的
        SCENE = 2,//场景
        UI = 3,//UI
    }
    public class AssetLoadInfo
    {
        public string assetPath;
        public object info;
        public IAssetLoaderReceiver receiver;
        public AssetLoadType type;
        public Type assetObjectType;//指定加载类型


        public AssetLoadInfo(string assetPath, object info, IAssetLoaderReceiver receiver, Type assetObjectType = null)
        {
            this.assetPath = assetPath;
            this.info = info;
            this.receiver = receiver;
            this.type = AssetLoadType.COMMON;
            this.assetObjectType = assetObjectType;

        }
        public AssetLoadInfo(string assetPath, object info, IAssetLoaderReceiver receiver, AssetLoadType load_type, Type assetObjectType = null)
        {
            this.assetPath = assetPath;
            this.info = info;
            this.receiver = receiver;
            this.type = load_type;
            this.assetObjectType = assetObjectType;
        }

        public string GetPrefabName()
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return "PrefabUnknown";
            }

            var startIndex = 0;
            var lastIndexOfBar = assetPath.LastIndexOf("/");
            if (lastIndexOfBar >= 0)
            {
                startIndex = lastIndexOfBar + 1;
            }

            var lastIndexOfDot = assetPath.LastIndexOf(".");
            if (lastIndexOfDot < 0)
            {
                lastIndexOfDot = assetPath.Length;
            }

            var name = assetPath.Substring(startIndex, lastIndexOfDot - startIndex);
            return name;
        }
    }
}
