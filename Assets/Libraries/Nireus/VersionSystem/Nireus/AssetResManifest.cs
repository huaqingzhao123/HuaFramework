#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using System;
using System.Collections.Generic;

namespace Nireus
{
    public class DownloadUnit
    {
        public string name;
        public string md5;
        public long size;
    }
    public class AssetResManifest
    {
        public Version version { get; private set; } = null;
        public AssetsVersionInfo assets_version_info { get; private set; } = null;
        public AssetResManifest(string text)
        {
            assets_version_info = AssetsVersionInfo.CreateFromString(text);
            version = new Version(assets_version_info.version);
        }
        public static AssetResManifest parseFile(string text)
        {
            if(string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            else
            {
                AssetResManifest m = null;
                try
                {
                    m = new AssetResManifest(text);
                }
                catch(Exception e)
                {
                    //todo
                }
                return m;
            }
            
        }
        public static AssetResManifest parseFile(byte[] datas)
        {
            return parseFile(System.Text.Encoding.UTF8.GetString(datas));
        }
        public bool versionEquals(ref AssetResManifest b)
        {
            return version.isEqual(b.version);
        }
        public bool versionGreater(ref AssetResManifest b)
        {
            return version.isNewerThan(b.version);
        }
        public bool versionGreaterOrEquals(ref AssetResManifest b)
        {
            return version.isNewerThan(b.version) || version.isEqual(b.version);
        }
        public void getNeedUpdateOrDeleteAssetBundle(ref AssetResManifest b,ref Dictionary<string, DownloadUnit> update_map, ref Dictionary<string, DownloadUnit> delete_map)
        {
            Dictionary<string, AssetBundleInfo> comp_assets_dic = b.assets_version_info.assets;
            string name;
            AssetBundleInfo asset_a;
            AssetBundleInfo asset_b;
            foreach (var pair in assets_version_info.assets)
            {
                name = pair.Key;
                asset_a = pair.Value;
                if(!comp_assets_dic.ContainsKey(name))
                {
                    DownloadUnit unit = new DownloadUnit();
                    unit.name = name;
                    unit.md5 = asset_a.md5;
                    unit.size = asset_a.size;
                    delete_map.Add(name, unit);
                }
                else
                {
                    asset_b = comp_assets_dic[name];
                    if(asset_a.md5 != asset_b.md5)
                    {
                        DownloadUnit unit = new DownloadUnit();
                        unit.name = name;
                        unit.md5 = asset_b.md5;
                        unit.size = asset_b.size;
                        update_map.Add(name, unit);
                    }
                }
            }

            foreach(var pair in comp_assets_dic)
            {
                name = pair.Key;
                asset_b = pair.Value;
                if(!assets_version_info.assets.ContainsKey(name))
                {
                    DownloadUnit unit = new DownloadUnit();
                    unit.name = name;
                    unit.md5 = asset_b.md5;
                    unit.size = asset_b.size;
                    update_map.Add(name, unit);
                }
            }
        }
        
        public void getNeedUpdateAssetBundle(ref Dictionary<string, DownloadUnit> update_map)
        {
            string name;
            AssetBundleInfo asset_a;
            foreach (var pair in assets_version_info.assets)
            {
                name = pair.Key;
                asset_a = pair.Value;
                DownloadUnit unit = new DownloadUnit();
                unit.name = name;
                unit.md5 = asset_a.md5;
                unit.size = asset_a.size;
                update_map.Add(name, unit);
            }
        }
    }
}
#endif
