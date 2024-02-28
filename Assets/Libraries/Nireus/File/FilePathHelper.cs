using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;

namespace Nireus
{
    public class FilePathHelper
    {
        private static FilePathHelper _instance = null;

        static public FilePathHelper getInstance()
        {
            if (_instance == null)
            {
                _instance = new FilePathHelper();
            }

            return _instance;
        }

        public static readonly string
            RW_ROOT_DIR = FilePathHelper.getResFolderRW() + "/"; // ANDROID /data/xxx.com/files/

        public static readonly string
            BASE_VERSION_FILE = FilePathHelper.getResFolderRW() + "/" + "base_version"; // 当前copy出来资源版本号;

        public readonly static string PATCH_ROOT_DIR = FilePathHelper.getResFolderRW() + "/patch/"; // 存放更新文件的路径;

        public readonly static string
            PATCH_TEMP_ROOT_DIR = FilePathHelper.getResFolderRW() + "/patch_temp/"; // 存放更新文件的路径;

        public readonly static string PATCH_AB_ROOT_DIR = FilePathHelper.getResFolderRW() + "/ab_patch/"; // 存放更新文件的路径;
        public readonly static string PATCH_GAME_RES_DIR = PATCH_AB_ROOT_DIR + "bundleres/"; // 解压存放最新补丁文件的路径;
        public readonly static string PATCH_GAME_RES_MODULE_DIR = PATCH_AB_ROOT_DIR + "module/"; // 解压存放最新模块补丁文件的路径;
        public static readonly string UPDATE_VERSION_FILE = PATCH_ROOT_DIR + "update_version.txt"; // 当前已更新的版本号;
        public readonly static string EXTRA_RES_DIR = PATCH_ROOT_DIR + "extra_res/";


        List<string> _patch_path_list; //补丁文件表.证明文件需要外部加载.
        Dictionary<string, string> _file_path_map;
        Dictionary<string, string> _asset_bundle_name_map;
        private static List<string> patch_ab_path_dict = new List<string>(); //补丁文件表
        private static List<string> patch_module_ab_path_list = new List<string>(); //模块补丁文件表
        private static Dictionary<string,string> patch_module_ab_path_dict = new Dictionary<string,string>(); //模块补丁文件表(ab_name,module_name)

        FilePathHelper()
        {
            _file_path_map = new Dictionary<string, string>();
            _asset_bundle_name_map = new Dictionary<string, string>();
            _patch_path_list = new List<string>();
        }

        public static string GetManifestAssetBundleFullPath()
        {
#if !UNITY_IOS
            string patch_path = Path.Combine(PATCH_GAME_RES_DIR, "AssetsBundle");
            if (File.Exists(patch_path))
            {
                return patch_path;

            }
#endif
            return Path.Combine(FilePathHelper.getInstance().GetPlatformStreamingPath(false), "AssetsBundle");
        }
        

        public void InitPatchDict()
        {
            patch_ab_path_dict.Clear();
            patch_module_ab_path_list.Clear();
            patch_module_ab_path_dict.Clear();
            InitMainPatchDict();
            InitModulePatchDict();
        }

        public void InitMainPatchDict()
        {
#if !UNITY_IOS
            if (Directory.Exists(FilePathHelper.PATCH_GAME_RES_DIR))
            {
                Version client_origin_version = new Version(Application.version);
                AssetResManifest local_manifest = null;
                string
                    file_data = FileUtil.getInstance().LoadPatchTextAsset(PathConst.ASSETS_VERSION_INFO_FILENAME);
                if (string.IsNullOrEmpty(file_data) == false)
                {
                    local_manifest = AssetResManifest.parseFile(file_data);
                    if (local_manifest != null)
                    {
                        Version patch_version = new Version(local_manifest.assets_version_info.version);
                        if (client_origin_version.isNewerThan(patch_version) ||
                            client_origin_version.isEqual(patch_version))
                        {
                            Directory.Delete(FilePathHelper.PATCH_GAME_RES_DIR, true);
                            return;
                        }
                    }
                }

                DirectoryInfo dir = new DirectoryInfo(FilePathHelper.PATCH_GAME_RES_DIR);
                FileInfo[] files = dir.GetFiles();
                for (int i = 0; i < files.Length; ++i)
                {
                    var file = files[i];
                    string file_name = file.Name;
                    patch_ab_path_dict.Add(files[i].Name);
                }
            }
#endif
        }
        
        public void InitModulePatchDict()
        {
#if USE_SUB_PACKAGE
            AssetModuleLoadManager.Instance.InitMuduleName();
            if (Directory.Exists(PATCH_GAME_RES_MODULE_DIR))
            {
                DirectoryInfo root_dir = new DirectoryInfo(PATCH_GAME_RES_MODULE_DIR);
                DirectoryInfo[] flods = root_dir.GetDirectories();
                foreach (var moduleFlod in flods)
                {
                    var moduleName = moduleFlod.Name;
                    if (AssetModuleLoadManager.Instance.IsModuleInConfig(moduleName))
                    {
                        InitSingleModulePatchDict(moduleName);
                    }
                }
            }
#endif
        }

        public void InitSingleModulePatchDict(string moduleName)
        {
#if USE_SUB_PACKAGE
            string modulePath = Path.Combine(PATCH_GAME_RES_MODULE_DIR, moduleName);
            GameDebug.Log($"[InitSingleModulePatchDict] modulePath:{modulePath}");
            string modulePathOrigin = Path.Combine(FilePathHelper.getInstance().GetPlatformStreamingPath(false), moduleName);
            if (Directory.Exists(modulePath))
            {
                if (Directory.Exists(modulePathOrigin))
                {
                    Version client_origin_version = new Version(Application.version);
                    AssetResManifest local_manifest = null;
                    string
                        file_data = FileUtil.getInstance() 
                            .LoadModulePatchTextAsset(moduleName, PathConst.ASSETS_VERSION_INFO_FILENAME);
                    if (string.IsNullOrEmpty(file_data) == false)
                    {
                        local_manifest = AssetResManifest.parseFile(file_data);
                        if (local_manifest != null)
                        {
                            Version patch_version = new Version(local_manifest.assets_version_info.version);
                            if (client_origin_version.isNewerThan(patch_version) ||
                                client_origin_version.isEqual(patch_version))
                            {
                                //分模块下载的资源 安装新包不做删除
                                // Directory.Delete(modulePath, true);
                                // Debug.LogWarning($"[InitSingleModulePatchDict] Delete Dir: {modulePath}");
                                // return;
                            }
                        }
                    }
                }

                DirectoryInfo dir = new DirectoryInfo(modulePath);
                FileInfo[] files = dir.GetFiles();
                GameDebug.Log($"[InitSingleModulePatchDict] modulePath:{modulePath}\n filesCnt:{files.Length}");
                for (int i = 0; i < files.Length; ++i)
                {
                    var file = files[i];
                    string file_name = file.Name;
                    if (file_name == PathConst.ASSETS_VERSION_INFO_FILENAME)
                    {
                        //热更版本文件不加入，不然键值重复了
                        continue;
                    }
                    patch_module_ab_path_list.Add(file_name);
                    patch_module_ab_path_dict.Add(file_name,moduleName);
                }
            }
#endif
        }

#if UNITY_ANDROID || USE_SUB_PACKAGE
        public static string GetPatchFilePath(string asset_name)
        {
            if (patch_ab_path_dict.Contains(asset_name))
            {
                return Path.Combine(FilePathHelper.PATCH_GAME_RES_DIR, asset_name);
            }
            
            var str_list = asset_name.Split('/');
            if (str_list.Length >= 2)
            {
                var moduleAssetName = str_list[str_list.Length - 1];
                var moduleName = str_list[str_list.Length - 2].ToLower();
                if (patch_module_ab_path_dict.ContainsValue(moduleName))
                {
                    return Path.Combine(FilePathHelper.PATCH_GAME_RES_MODULE_DIR, moduleName, moduleAssetName);
                }

                GameDebug.Log($"[FilePathHelper.GetPatchFilePath] patch_module_ab_path_dict doesn't contains value:{moduleName}");
            }

            return "";
        }
#endif


        public static string GetFilePathAbsolute(string bundleName)
        {
#if UNITY_IOS || UNITY_ANDROID
            if (patch_module_ab_path_list.Contains(bundleName))
            {
                var moduleName = patch_module_ab_path_dict[bundleName].ToLower();
                return Path.Combine(FilePathHelper.PATCH_GAME_RES_MODULE_DIR, moduleName, bundleName);
            }
#endif
#if !UNITY_IOS
            if (patch_ab_path_dict.Contains(bundleName))
            {
                return Path.Combine(FilePathHelper.PATCH_GAME_RES_DIR, bundleName);
            }
#endif
            return Path.Combine(FilePathHelper.getInstance().GetPlatformStreamingPath(false), bundleName);
        }

        public static string AssetBundlePathToName(string assetPath)
        {
            if (assetPath.IndexOf("@") >= 0)
            {
                string ab_name = assetPath.Replace('/', '_').ToLower();
                ab_name = ab_name.Substring(0, ab_name.LastIndexOf(".")) + ".ab";
                return ab_name;
            }
            else
            {
                //一个文件夹下所有资源 打一个AB,不受子文件夹影响
                if (assetPath.IndexOf("$") >= 0)//一个文件一个ab
                {  
                    string ab_name = assetPath.Replace('/', '_').ToLower();
                    ab_name = ab_name.Substring(0, assetPath.LastIndexOf("$")+1) + ".ab";
                    return ab_name;
                }
                else if (assetPath.IndexOf(PathConst.BUNDLE_RES_UI_PREFABS) >= 0)
                {
                    string module_name = GetUIModuleDirName(assetPath);
                    string path = PathConst.BUNDLE_RES_UI_PREFABS.Replace('/', '_').ToLower();
                    return path + module_name + ".ab";
                }
                else
                {
                    string ab_name = Path.GetDirectoryName(assetPath)
                        .Replace(System.IO.Path.DirectorySeparatorChar, '_').ToLower();
                    return ab_name + ".ab";
                }
            }
        }

        public static string GetUIModuleDirName(string full_path)
        {
            string dir_full_root = PathConst.BUNDLE_RES_UI_PREFABS;
            //full_path = full_path.Replace("\\", "/");
            int index = full_path.IndexOf(dir_full_root) + dir_full_root.Length;
            if (index != -1)
            {
                string start = full_path.Substring(index);
                string module = start.Substring(0, start.IndexOf("/"));
                return module.ToLower();
            }

            return "";
        }


        static string _res_floder;

        public static string getResFolderRW()
        {
            if (_res_floder != null) return _res_floder;

#if UNITY_EDITOR
            string path = new DirectoryInfo(Application.dataPath).Parent.FullName;
            if (System.IO.Directory.Exists(path) == false)
                System.IO.Directory.CreateDirectory(path);
            _res_floder = path;
#else
            _res_floder = Application.persistentDataPath;
#endif
            return _res_floder;
        }


        public static string WebPersistentPath
        {
            get
            {
                string path = string.Empty;
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        path = "file://" + Directory.GetParent(Application.dataPath) + "/Persistent/";
                        if (!Directory.Exists(PersistentPath))
                        {
                            Directory.CreateDirectory(path);
                        }

                        break;
                    default:
                        path = "file://" + Application.persistentDataPath + "/";
                        break;
                }

                return path;
            }
        }

        public static string PersistentPath
        {
            get
            {
                string path = string.Empty;
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        path = Directory.GetParent(Application.dataPath) + "/Persistent/";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        break;
                    default:
                        path = Application.persistentDataPath + "/";
                        break;
                }

                return path;
            }
        }

        public string GetPlatformStreamingPath(bool useForWWW)
        {
            string path = string.Empty;
#if UNITY_EDITOR
            path = (useForWWW ? "file://" : "") + Application.dataPath + "/StreamingAssets/";
#elif UNITY_IPHONE
            path = (useForWWW ? "file://" : "") + Application.dataPath + "/Raw/";
#elif UNITY_ANDROID
            path = "jar:file://" + Application.dataPath + "!/assets/" ;
#else
            path = Application.dataPath + "/StreamingAssets/" ;
#endif
            return path;
        }
    }
}
