#if USE_SUB_PACKAGE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nireus
{
    public class AssetModuleLoadManager : Singleton<AssetModuleLoadManager> {

#if UNITY_ANDROID
        public const string CONFIG_ASSET_PATH = PathConst.BUNDLE_RES + "config/module/package_config.asset";
#else
        public const string CONFIG_ASSET_PATH = PathConst.BUNDLE_RES + "config/module/package_config_ios.asset";
#endif
        private  ModulePackageConfigAsset _config;
        private Dictionary<string, AssetResManifest> _local_manifest_dic = new Dictionary<string, AssetResManifest>();
        private HashSet<string> _module_name_in_config = new HashSet<string>();
        public void Init()
        {

#if UNITY_ANDROID || UNITY_IOS
            //先加载配置信息
            //_config = AssetManager.Instance.GetAsset<ModulePackageConfigAsset>(CONFIG_ASSET_PATH);
            _config = AssetManager.Instance.loadSync<ModulePackageConfigAsset>(CONFIG_ASSET_PATH);
            if (_config)
            {
                var modulePackageListDictionary_AB = _config.ModulePackageListDictionary_AB;
                foreach (var module in modulePackageListDictionary_AB)
                {
                    LoadLocalManifest(Utils.GetModulePathName((int)module.Key));
                }
                
                GameDebug.Log("AssetModuleLoadManager init success");
            }
            else
            {
                GameDebug.LogError(CONFIG_ASSET_PATH+"is not exist!!!");
            }
#endif
        }
        
        //用来判断模块名是否在配置表中
        public void InitMuduleName()
        {

#if UNITY_ANDROID || UNITY_IOS
            //先加载配置信息
            _module_name_in_config.Clear();
            _config = AssetManager.Instance.loadSync<ModulePackageConfigAsset>(CONFIG_ASSET_PATH);
            //_config = AssetManager.Instance.GetAsset<ModulePackageConfigAsset>(CONFIG_ASSET_PATH);
            if (_config)
            {
                var modulePackageListDictionary_AB = _config.ModulePackageListDictionary_AB;
                foreach (var module in modulePackageListDictionary_AB)
                {
                    var name = Utils.GetModulePathName((int) module.Key);
                    _module_name_in_config.Add(name);
                }
                
                GameDebug.Log("AssetModuleLoadManager InitMuduleName success");
            }
            else
            {
                GameDebug.LogError(CONFIG_ASSET_PATH+"is not exist!!!");
            }
#endif
        }

        public bool IsModuleInConfig(string modulePackageName)
        {
            if (_module_name_in_config.Contains(modulePackageName))
            {
                return true;
            }
            
            return false;
        }
        public Boolean IsModuleAB(string modulePackageName)
        {
            if (_local_manifest_dic.ContainsKey(modulePackageName))
            {
                return true;
            }

            return false;
        }

        public void LoadLocalManifest(string modulePackageName)
        {
            var filename = modulePackageName + @"/" + PathConst.ASSETS_VERSION_INFO_FILENAME;
            string file_data = FileUtil.getInstance().LoadStreamOrPatchTextAsset(filename);
#if UNITY_EDITOR
            GameDebug.Log($"[LoadLocalManifest] filename:{filename} \nfiledata:{file_data}");
#endif
            if (string.IsNullOrEmpty(file_data) == false)
            {
                if (_local_manifest_dic.ContainsKey(modulePackageName))
                {
                    _local_manifest_dic[modulePackageName] = AssetResManifest.parseFile(file_data);
                }
                else
                {
                    _local_manifest_dic.Add(modulePackageName,AssetResManifest.parseFile(file_data));
                }
            }
            else
            {
                if (_local_manifest_dic.ContainsKey(modulePackageName))
                {
                    _local_manifest_dic[modulePackageName] = null;
                }
                else
                {
                    _local_manifest_dic.Add(modulePackageName, null);
                }
            }
        }
        public AssetResManifest GetLocalManifest( ModuleNameIndex moduleIndex)
        {
            var moduleName = Utils.GetModulePathName((int)moduleIndex);
            _local_manifest_dic.TryGetValue(moduleName, out var manifest);
            return manifest;
        }
        
        public Boolean IsModuleInConfig( ModuleNameIndex moduleIndex)
        {
            if (null != _config && _config.ModulePackageListDictionary.ContainsKey(moduleIndex))
            {
                return true;
            }

            return false;
        }

        public Boolean IsNeedUpdate(ModuleNameIndex moduleIndex)
        {
            if (IsModuleInConfig(moduleIndex) == false)
            {
                GameDebug.LogWarning("skip module update,module is not in config");
                return false;
            }
            var local_manifest = GetLocalManifest(moduleIndex);
#if (UNITY_ANDROID || UNITY_IOS) && ASSET_BUNDLE
            var version = RemoteVersionManager.Instance.GetRemoteVersion();
            if (string.IsNullOrWhiteSpace(version))
            {
                GameDebug.LogWarning("skip module update,remote version is empty");
                return false;
            }
            var server_version = new Version(version);
            if (local_manifest == null)
            {
                GameDebug.LogWarning($"local_manifest of {moduleIndex.ToString()} is null");
                return true;
            }

            if (server_version.isNewerThan(local_manifest.version))
            {
                GameDebug.LogWarning($"server version ({server_version.ToString()}) is Newer than local's ({local_manifest.version}).");
                return true;
            }
#endif
            GameDebug.LogWarning("skip module update,local res is newest");
            return false;
        }
    }
}
#endif