using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Nireus.Editor
{
#if !ASSET_RESOURCES
    public class PackageBundleRes
	{
        //const string OUT_PATH = "Assets/BeforeEncryption/";
        const string GAME_RES_PATH = "Assets/Res/BundleRes/";
        const string TEXTURES_PATH = "Assets/Res/Textures/";
        const string UI_RES_COMMON_IMAGES    = TEXTURES_PATH + "UIResources/Common/";
        const string UI_RES_COMMON_ANIMATION = TEXTURES_PATH + "UIResources/Common/Animation/";
        const string SPINE_PATH = TEXTURES_PATH + "SpineResources/";
        //const string UI_RES_BUTTIONS = GAME_RES_PATH + "Textrue/UI/button/";
        
#if UNITY_ANDROID 
        public readonly static string UI_MODULE_CONFIG_DIR = GAME_RES_PATH + "Config/module/package_config.asset";//分模块AssetBundle加载配置文件
#else 
        public readonly static string UI_MODULE_CONFIG_DIR = GAME_RES_PATH + "Config/module/package_config_ios.asset";//分模块AssetBundle加载配置文件
#endif
        public static Dictionary<string,HashSet<string>> bundlesMap = new Dictionary<string, HashSet<string>>();
        public static void packageOneDic(string dir_root)
        {
            _packAllDir(dir_root);
        }
        public static void package()
		{
            _packAllDir(GAME_RES_PATH);
        }
        public static void packageNew()
        {
            _packAllDir_New(GAME_RES_PATH);
        }
        private static void _packAllDir(string dir_root)
        {
            DirectoryInfo directory_info = new DirectoryInfo(dir_root);
            List<FileInfo> files = new List<FileInfo>();
            FileUtil.GetFliesRecursion(files, directory_info);
            int total_cnt = files.Count;
            int succ_cnt = 0;
            foreach (var item in files)
            {
                AddAssetBundleName(dir_root, item);
                succ_cnt++;
            }
            Debug.Log("dir: " + dir_root + " Complete: succ/total: " + succ_cnt + "/" + total_cnt);
        }

        public static void AddAssetBundleName(string dir_root,FileInfo item)
        {
            string assetBundleName = AssetBundleFilePath.GetAssetBundleName(dir_root,item);
            if (assetBundleName.Length!=0)
            {
                string assetFullPath = item.FullName.Replace("\\", "/");
                string assetBundlePath = assetFullPath.Substring(assetFullPath.IndexOf(dir_root) + dir_root.Length);//去除PIC_PATH的内容保留后面的路径
                string assetPath = dir_root + assetBundlePath;//unity 导入路径
                AssetImporter.GetAtPath(assetPath).assetBundleName = assetBundleName;
            }
        }
        private static void _packAllDir_New(string dir_root)
        {
            bundlesMap.Clear();
            DirectoryInfo directory_info = new DirectoryInfo(dir_root);
            List<FileInfo> files = new List<FileInfo>();
            FileUtil.GetFliesRecursion(files, directory_info);
            int total_cnt = files.Count;
            int succ_cnt = 0;
            foreach (var item in files)
            {
                AddAssetBundleName_New(dir_root, item);
                succ_cnt++;
            }
            Debug.Log("dir: " + dir_root + " Complete: succ/total: " + succ_cnt + "/" + total_cnt);
        }
        public static void AddAssetBundleName_New(string dir_root,FileInfo item)
        {
            string assetBundleName = AssetBundleFilePath.GetAssetBundleName(dir_root,item);
            if (assetBundleName.Length!=0)
            {
                string assetFullPath = item.FullName.Replace("\\", "/");
                string assetBundlePath = assetFullPath.Substring(assetFullPath.IndexOf(dir_root) + dir_root.Length);//去除PIC_PATH的内容保留后面的路径
                string assetPath = dir_root + assetBundlePath;//unity 导入路径
                if (bundlesMap.TryGetValue(assetBundleName,out HashSet<string> assets))
                {
                    assets.Add(assetPath);
                }
                else
                {
                    HashSet<string> assetSet = new HashSet<string>();
                    assetSet.Add(assetPath);
                    bundlesMap.Add(assetBundleName,assetSet);
                }
                //AssetImporter.GetAtPath(assetPath).assetBundleName = assetBundleName;
            }
        }
        public static string GetModuleDirName(string dir_full_root, string full_path)
        {
            full_path = full_path.Replace("\\", "/");
            int index = full_path.IndexOf(dir_full_root) + dir_full_root.Length;
            if (index != -1)
            {
                string start = full_path.Substring(index);
                string module = start.Substring(0, start.IndexOf("/"));
                return module;
            }
            return "";
        }


        /// <summary>  
        /// 清除之前设置过的AssetBundleName，避免产生不必要的资源也打包  
        /// 之前说过，只要设置了AssetBundleName的，都会进行打包，不论在什么目录下  
        /// </summary>  
        public static void ClearAssetBundlesName()
        {
            int length = AssetDatabase.GetAllAssetBundleNames().Length;
            string[] oldAssetBundleNames = new string[length];
            for (int i = 0; i < length; i++)
            {
                oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
            }

            for (int j = 0; j < oldAssetBundleNames.Length; j++)
            {
                AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
            }
            length = AssetDatabase.GetAllAssetBundleNames().Length;
        }


        public static void packageUIOriginalRes()
        {
            List<FileInfo> files = new List<FileInfo>();

            //FileUtil.GetFliesRecursion(files, new DirectoryInfo(UI_RES_COMMON_IMAGES));
            //FileUtil.GetFliesRecursion(files, new DirectoryInfo(UI_RES_COMMON_ANIMATION));
            //FileUtil.GetFliesRecursion(files, new DirectoryInfo(UI_RES_CANVAS));

            int total_cnt = files.Count;
            int succ_cnt = 0;
            foreach (var item in files)
            {
                if (item.Extension != ".meta" && item.Extension != ".DS_Store")
                {
                    string full_path = item.FullName.Replace("\\", "/");

                    string assetPath = full_path.Substring(full_path.IndexOf("/Assets/") + 1);//unity 导入路径
                    string assetBundleName = Path.GetDirectoryName(assetPath).Replace('\\','/') + ".ab";// assetPath.Substring(0, assetPath.LastIndexOf(".")) + ".ab";
                    var asset = AssetImporter.GetAtPath(assetPath);
                    asset.assetBundleName = assetBundleName;

                    //Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    //asset.textureType = TextureImporterType.Sprite;
                    //asset.mipmapEnabled = false;

                    //if (texture.width <= 128 && texture.height <= 128 && Mathf.IsPowerOfTwo(texture.width) && Mathf.IsPowerOfTwo(texture.height))
                    //{
                    //    TextureImporterPlatformSettings set = new TextureImporterPlatformSettings();
                    //    set.overridden = true;
                    //    set.format = TextureImporterFormat.RGBA32;
                    //    asset.SetPlatformTextureSettings(set);
                    //}
                    //asset.SaveAndReimport();
                    if (asset.assetBundleName == "")
                    {
                        Debug.LogError("assetBundleName error ,need ClearAssetBundlesName");
                    }

                }
                succ_cnt++;
            }
            Debug.Log("Package ui original Res Complete: succ/total: " + succ_cnt + "/" + total_cnt);
        }


        public static void CompressionSpineImage()
        {
            return;//del
            List<FileInfo> files = new List<FileInfo>();
            FileUtil.GetFliesRecursion(files, new DirectoryInfo(SPINE_PATH));

            int total_cnt = files.Count;
            int succ_cnt = 0;
            foreach (var item in files)
            {
                if (item.Extension == ".png" || item.Extension == ".jpg")
                {
                    string full_path = item.FullName.Replace("\\", "/");
                    string assetPath = full_path.Substring(full_path.IndexOf("/Assets/") + 1);//unity 导入路径
                    Debug.Log("asset " + assetPath);
                    var import = AssetImporter.GetAtPath(assetPath) as TextureImporter;


                    Debug.Log("import " + import);
                    //if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                    {
                        TextureImporterPlatformSettings textureImporterSettings = import.GetPlatformTextureSettings("Android");
                        textureImporterSettings.overridden = true;
                        textureImporterSettings.format = TextureImporterFormat.RGBA32;
                        import.SetPlatformTextureSettings(textureImporterSettings);
                    }


                    TextureImporterPlatformSettings textureImporterSettings_ios = import.GetPlatformTextureSettings("iPhone");
                    textureImporterSettings_ios.overridden = true;
                    textureImporterSettings_ios.format = TextureImporterFormat.RGBA32;
                    import.SetPlatformTextureSettings(textureImporterSettings_ios);
                    import.SaveAndReimport();
                }
                succ_cnt++;
            }
            Debug.Log("Package spine image Complete: succ/total: " + succ_cnt + "/" + total_cnt);
        }
    }
#endif
}