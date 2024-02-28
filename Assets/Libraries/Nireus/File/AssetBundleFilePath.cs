using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Nireus
{
    public class AssetBundleFilePath
    {
        const string GAME_RES_PATH = "Assets/Res/BundleRes/";
        const string GAME_UI_PREFAB_PATH = GAME_RES_PATH + "UIPrefabs/";
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
        
        public static void GetFliesRecursion(List<FileInfo> list, DirectoryInfo dir,string excludeType=".meta")
        {
            FileInfo[] allFile = dir.GetFiles();
            foreach (var item in allFile)
            {
                if (item.Extension != excludeType)
                {
                    list.Add(item);
                }
            }
            DirectoryInfo[] allDirs = dir.GetDirectories();
            foreach (var item in allDirs)
            {
                GetFliesRecursion(list, item);
            }
        }
        public static string GetAssetBundleName(string dir_root, FileInfo item)
        {
            string assetBundleName = "";
            if (item.Extension != ".meta" && item.Extension != ".DS_Store")
            {
                string assetFullPath = item.FullName.Replace("\\", "/");
                assetBundleName = assetFullPath;
                assetBundleName = assetBundleName.Substring(assetBundleName.IndexOf(dir_root) + dir_root.Length);//去除PIC_PATH的内容保留后面的路径
                string assetPath = dir_root + assetBundleName;//unity 导入路径
                if (assetPath.IndexOf("@") >= 0)//一个文件一个ab
                {  
                    // assetBundleName = assetBundleName.Substring(0, assetBundleName.LastIndexOf("."))+"";
                    assetBundleName = assetPath.Substring(0, assetPath.LastIndexOf(".")) + ".ab";
                }
                else//一个目录一个ab
                {
                    //一个文件夹下所有资源 打一个AB,不受子文件夹影响
                    if (assetFullPath.IndexOf("$") >= 0)//一个文件一个ab
                    {  
                        assetBundleName = assetPath.Substring(0, assetPath.LastIndexOf("$")+1) + ".ab";
                    }
                    else if (assetFullPath.IndexOf(GAME_UI_PREFAB_PATH) >= 0)//ui 
                    {
                        string moduld_name = GetModuleDirName(GAME_UI_PREFAB_PATH, assetPath).ToLower();
                        assetBundleName =  GAME_UI_PREFAB_PATH + moduld_name + ".ab";
                    }
                    else
                    {
                        assetBundleName = assetPath.Substring(0, assetPath.LastIndexOf("/")) + ".ab";
                    }
                }
            }

            return assetBundleName.ToLower();
        }
        public static List<string> GetPackName_AB(string dir_root)
        {
            var asset_bunle_name_list = new List<string>();
            if (dir_root.Length == 0)
            {
                return asset_bunle_name_list;
            }
            DirectoryInfo directory_info = new DirectoryInfo(dir_root);
            List<FileInfo> files = new List<FileInfo>();
            GetFliesRecursion(files, directory_info);
            foreach (var item in files)
            {
                string asset_bunle_path = GetAssetBundleName(dir_root,item);
                string asset_bunle_name = asset_bunle_path.Replace('/', '_');

                if(string.IsNullOrWhiteSpace(asset_bunle_name) || asset_bunle_name_list.Contains(asset_bunle_name)) {
                    continue;
                }

                asset_bunle_name_list.Add(asset_bunle_name);
            }

            return asset_bunle_name_list;
        }
    }
}