using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEditor;

#if ASSET_BUNDLE && USE_SUB_PACKAGE
namespace Nireus.Editor
{
    public class ModulePackageConfigEditor
    {
        public readonly static string  OUT_ASSET_BUNDLE_FILE_PATH = "Assets/StreamingAssets/";
        
        [MenuItem("Nireus/Create ModulePackageConfig",false, 301)]
        static void CreateScriptObject()
        {
            if (File.Exists(PackageBundleRes.UI_MODULE_CONFIG_DIR))
            {
                if (EditorUtility.DisplayDialog("分模块加载AssetBundle配置已存在",
                        "确定要重新创建？", "确定", "取消")) {
                }
                else
                {
                    return;
                }
            }
                
            
            ModulePackageConfigAsset createAsset = ScriptableObject.CreateInstance<ModulePackageConfigAsset>();

            AssetDatabase.CreateAsset(createAsset, PackageBundleRes.UI_MODULE_CONFIG_DIR);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Nireus/Move AssetBundleToModule",false, 302)]
        static void MoveAssetBundleToModule()
        {
            if (ModulePackageConfigAsset.IsNotConfigModule())
            {
                Debug.LogError("Module config is empty!!!");
                return;
            }
            ClearModuleAssetBundleDirectory();
            var modulePackageListDictionary_AB = ModulePackageConfigAsset.Instance.ModulePackageListDictionary_AB;
            foreach (var VARIABLE in modulePackageListDictionary_AB)
            {
                var ab_module_fold_path = GetModuleAssetBundlePath(VARIABLE.Key);
                foreach (var depend_ab_name in VARIABLE.Value)
                {
                    var ab_module_file_path = ab_module_fold_path + depend_ab_name;
                    var ab_out_file_path = OUT_ASSET_BUNDLE_FILE_PATH + depend_ab_name;
                    File.Move(ab_out_file_path, ab_module_file_path);
                    File.Move(ab_out_file_path+".meta", ab_module_file_path+".meta");
                    File.Move(ab_out_file_path+".manifest", ab_module_file_path + ".manifest");
                    File.Move(ab_out_file_path+".manifest"+".meta", ab_module_file_path + ".manifest"+".meta");
                }
            }

            PackingBundlesPostProcess.GenerateAssetsVersionInfo();
            GenerateAllModuleAssetsVersionInfo();
            AssetDatabase.Refresh();
            Debug.Log("Move AssetBundle To Module Successfully.");
        }
        
        [MenuItem("Nireus/Move ModuleToAssetBundle",false, 303)]
        static void MoveModuleToAssetBundle()
        {
            if (ModulePackageConfigAsset.IsNotConfigModule())
            {
                Debug.LogError("Module config is empty!!!");
                return;
            }
            var modulePackageListDictionary_AB = ModulePackageConfigAsset.Instance.ModulePackageListDictionary_AB;
            foreach (var VARIABLE in modulePackageListDictionary_AB)
            {
                var ab_module_fold_path = GetModuleAssetBundlePath(VARIABLE.Key);
                var dirInfo = new DirectoryInfo(ab_module_fold_path);
                var files = dirInfo.GetFiles();
                if (files.Length == 0)
                {
                    Debug.LogError($"Module Directory {ab_module_fold_path} is empty,had delete");
                    Directory.Delete(ab_module_fold_path,true);
                    continue;
                }
                foreach (var depend_ab_name in VARIABLE.Value)
                {
                    var ab_module_file_path = ab_module_fold_path + depend_ab_name;
                    var ab_out_file_path = OUT_ASSET_BUNDLE_FILE_PATH + depend_ab_name;
                    File.Move(ab_module_file_path, ab_out_file_path);
                    File.Move(ab_module_file_path+".meta", ab_out_file_path+".meta");
                    File.Move( ab_module_file_path + ".manifest",ab_out_file_path+".manifest");
                    File.Move( ab_module_file_path + ".manifest"+".meta",ab_out_file_path+".manifest"+".meta");
                }
                
                Directory.Delete(ab_module_fold_path,true);
            }
            AssetDatabase.Refresh();
            Debug.Log("Move Module To AssetBundle Successfully.");
        }
        
        [MenuItem("Nireus/Generate AllModuleAssets VersionInfo",false, 304)]
        public static void GenerateAllModuleAssetsVersionInfo()
        {
            if (ModulePackageConfigAsset.IsNotConfigModule())
            {
                Debug.LogError("Module config is empty!!!");
                return;
            }
            var modulePackageListDictionary_AB = ModulePackageConfigAsset.Instance.ModulePackageListDictionary_AB;
            foreach (var VARIABLE in modulePackageListDictionary_AB)
            {
                var ab_module_fold_path = GetModuleAssetBundlePath(VARIABLE.Key);
                GenerateMoudleAssetsVersionInfo(ab_module_fold_path);
            }
            //AssetDatabase.Refresh();
        }
        
        static void ClearModuleAssetBundleDirectory()
        {
            var modulePackageListDictionaryAb = ModulePackageConfigAsset.Instance.ModulePackageListDictionary_AB;
            foreach (var variable in modulePackageListDictionaryAb)
            {
                var modulePath = GetModuleAssetBundlePath(variable.Key);
                Directory.Delete(modulePath);
                string path = new DirectoryInfo(modulePath).FullName;
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        static string GetModuleAssetBundlePath(ModuleNameIndex moduleIndex)
        {
            var moduleName = Utils.GetModulePathName((int)moduleIndex).ToLower();
            var path = Path.Combine(PackageEditor.GetStreamingAssetsPath(), moduleName+ @"/");
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            return path;
        }

        public static void GenerateMoudleAssetsVersionInfo(string modulePath)
        {
#if !ASSET_RESOURCES
            var assetsVersionInfo = new AssetsVersionInfo();
            assetsVersionInfo.version = Application.version;
            assetsVersionInfo.assets = new Dictionary<string, AssetBundleInfo>();

            var md5Provider = new MD5CryptoServiceProvider();
            var dirInfo = new DirectoryInfo(modulePath);
            var files = dirInfo.GetFiles();
            if (files.Length == 0)
            {
                Debug.LogError($"Module Directory {modulePath} is empty,had delete");
                Directory.Delete(modulePath,true);
            }
            else
            {
                foreach (var file in files)
                {
                    var fileName = file.Name;
                    var need = fileName == "AssetsBundle" || fileName.EndsWith(".ab");
                    if (!need) continue;

                    var size = file.Length;
                    var fs = file.OpenRead();
                    var hash = md5Provider.ComputeHash(fs);
                    fs.Close();

                    var sb = new StringBuilder();
                    foreach (var b in hash)
                    {
                        sb.Append(b.ToString("x2"));
                    }

                    var md5 = sb.ToString();

                    var abInfo = new AssetBundleInfo {size = size, md5 = md5};
                    assetsVersionInfo.assets[file.Name] = abInfo;
                }

                var serialized = assetsVersionInfo.ToString();
                var path = modulePath + PathConst.ASSETS_VERSION_INFO_FILENAME;
                File.WriteAllText(path, serialized);

                Debug.Log($"{path} generated.");
            }
#else
            Debug.Log($"Can't generate in Resources mode.");
#endif
        }
    }
}
#endif