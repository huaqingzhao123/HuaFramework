using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if PACK_OLD
namespace Nireus.Editor
{
	class PackageEditorBack : UnityEditor.Editor
	{
		public static new BuildTarget target = BuildTarget.Android;
        const string BUNDLE_OUT_PATH = "Temp/AssetsBundle/BeforeEncryption/";
        const string TEXTURE_COMMON_FULL_DIR = "Assets/Res/Textures/UI/Common/";
        const string BUNDLE_REDUNDANCE_PATH = "Assets/Res/BundleRes/Redundance/";

        struct UIResStats
        {
            public string res_path;
            public string res_name;
            public List<string> ref_path;

            public UIResStats(string res_name,string res_path, string ref_path_v)
            {
                this.res_name = res_name;
                this.res_path = res_path;
                this.ref_path = new List<string>();
                this.ref_path.Add(ref_path_v);
            }
        }

        [MenuItem("Nireus/UI资源冗余统计", false, 601)]
        private static void UIResRedundanceStats()
        {
            Dictionary<string, UIResStats> stats_map = new Dictionary<string, UIResStats>();
            DirectoryInfo directory_info = new DirectoryInfo(PathConst.BUNDLE_RES_UI_PREFABS);
            _UIResRedundanceStatsImpl(directory_info, ref stats_map);
            List<UIResStats> stats_list = new List<UIResStats>();
            foreach (var item in stats_map)
            {
                if (item.Value.ref_path.Count > 1)
                    stats_list.Add(item.Value);
            }

            stats_list.Sort((left, right) => {
                int ret = right.ref_path.Count.CompareTo(left.ref_path.Count);
                return ret == 0 ? left.res_path.CompareTo(right.res_path) : ret;
            });
            StreamWriter sw = new StreamWriter(Application.dataPath + "/../ui_res_stats.log", false);
            UIResStats stats_data;
            for (int i = 0; i < stats_list.Count; ++i)
            {
                stats_data = stats_list[i];

                sw.Write("res_name:" + stats_data.res_path + "\r\nref_count:" + stats_data.ref_path.Count + "\r\n");
                sw.Write("ref_list:\r\n");
                bool need_move = false;
                string module_name = GetModuleDirName(directory_info.Name, stats_data.ref_path[0]);
                for (int j = 0; j < stats_data.ref_path.Count; ++j)
                {
                    sw.Write(stats_data.ref_path[j] + "\r\n");
                    string module_name_t = GetModuleDirName(directory_info.Name, stats_data.ref_path[j]);
                    if (stats_data.res_path != "Resources/unity_builtin_extra" && module_name != module_name_t)
                    {
                        need_move = true;
                    }
                }
                if (need_move)
                {
                    sw.Write("need! move to common " + stats_data.res_path + "!! \r\n");
                    Debug.LogWarning("move to common :" + stats_data.res_path);
                    try
                    {
                        string target_path = Path.Combine(TEXTURE_COMMON_FULL_DIR, stats_data.res_name);
                        if (File.Exists(target_path))
                        {
                            target_path = Path.Combine(TEXTURE_COMMON_FULL_DIR, Path.GetFileNameWithoutExtension(stats_data.res_name)+ module_name.ToLower()+Path.GetExtension(stats_data.res_name));
                            Debug.LogWarning("重命名:" + target_path);
                        }

                        File.Move(stats_data.res_path, target_path);
                        File.Move(stats_data.res_path + ".meta", target_path + ".meta");
                        
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                sw.Write("==============================================================\r\n");
            }
            sw.Close();
            Debug.Log("finish:"+ Application.dataPath + "/../ui_res_stats.log");
        }

        private static string GetModuleDirName(string dir_full_root,string full_path)
        {
            full_path = full_path.Replace("\\", "/");
            int index = full_path.IndexOf(dir_full_root) + dir_full_root.Length + 1;
            if (index != -1)
            {
                string start = full_path.Substring(index);
                string module = start.Substring(0, start.IndexOf("/"));
                return module;
            }
            return "";
        }


        private static void _UIResRedundanceStatsImpl(DirectoryInfo directory_info, ref Dictionary<string, UIResStats> stats_map)
        {
            Dictionary<string, int> tmp_map = new Dictionary<string, int>();
            FileInfo[] file_infos = directory_info.GetFiles();
            UIResStats stats_data;
            for (int i = 0; i < file_infos.Length; ++i)
            {
                FileInfo file_info = file_infos[i];
                tmp_map.Clear();
                if (file_info.Extension == ".prefab")
                {
                    string file_path = file_info.FullName.Substring(file_info.FullName.IndexOf("Assets"));
                    GameObject game_obj = AssetDatabase.LoadAssetAtPath<GameObject>(file_path);
                    if (game_obj != null)
                    {
                        AssetImporter tmp_ai = AssetImporter.GetAtPath(file_path);
                        string prb_ab_name = tmp_ai == null ? null : tmp_ai.assetBundleName;
                        UnityEngine.UI.Image[] imgs = game_obj.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                        for (int j = 0; j < imgs.Length; ++j)
                        {
                            UnityEngine.UI.Image img = imgs[j];
                            if (img.sprite == null) continue;
                            string key = img.sprite.name;
                            string key2 = AssetDatabase.GetAssetPath(img.sprite);
                            tmp_ai = AssetImporter.GetAtPath(key2);
                            string img_ab_name = tmp_ai == null ? null : tmp_ai.assetBundleName;
                            if (img_ab_name == prb_ab_name) continue;
                            if (img_ab_name == "assets/textures/uiresources/common.ab") continue;
                            string res_name = Path.GetFileName(key2);
                            string res_path =  key2;
                            if (tmp_map.ContainsKey(res_path)) continue;
                            tmp_map.Add(res_path, 1);
                            if (stats_map.TryGetValue(res_path, out stats_data))
                            {
                                stats_data.ref_path.Add(file_path);
                            }
                            else
                            {
                                stats_map.Add(res_path, new UIResStats(res_name, res_path, file_path));
                            }
                        }
                    }
                }
            }

            DirectoryInfo[] directory_infos = directory_info.GetDirectories();
            for (int i = 0; i < directory_infos.Length; ++i)
            {
                _UIResRedundanceStatsImpl(directory_infos[i], ref stats_map);
            }
        }


        [MenuItem("Nireus/Make AssetBundle Label", false, 1)]
        static void MakeResBundleName()
        {
#if !ASSET_RESOURCES
            PackageBundleRes.package();
            PackageBundleRes.packageUIOriginalRes();
#endif
        }
        [MenuItem("Nireus/Clear AssetBundle Label", false, 2)]
		static void ClearAssetBundlesName()
		{
#if !ASSET_RESOURCES
			PackageBundleRes.ClearAssetBundlesName();
#endif
        }

        [MenuItem("Nireus/Clear Bundles", false, 101)]
        static void ClearAllAssetBundleFiles()
        {
            ClearAssetBundleDirectory();
        }

        [MenuItem("Nireus/Pack Bundles (with Clear)", false, 103)]
		static void PackBundlesWithClear()
		{
#if !ASSET_RESOURCES
		    ClearResourcesBundleResDirectory();
            ClearAssetBundleDirectory();
            PackageBundleRes.ClearAssetBundlesName();
            PackageBundleRes.package();
            PackageBundleRes.packageUIOriginalRes();
            PackageBundleRes.CompressionSpineImage();
            BuildPipeline.BuildAssetBundles(GetBundleOutputPath(), BuildAssetBundleOptions.None,EditorUserBuildSettings.activeBuildTarget);
            PackingBundlesPostProcess.Process();
            AssetDatabase.Refresh();
#endif
        }

        [MenuItem("Nireus/Pack Bundles (don't Clear)", false, 102)]
        static void PackBundlesWithoutClear()
        {
#if !ASSET_RESOURCES
            ClearResourcesBundleResDirectory();
            PackageBundleRes.package();
            PackageBundleRes.packageUIOriginalRes();
            PackageBundleRes.CompressionSpineImage();
            BuildPipeline.BuildAssetBundles(GetBundleOutputPath(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
            PackingBundlesPostProcess.Process();
            AssetDatabase.Refresh();
#endif
        }

	    [MenuItem("Nireus/Generate AssetsVersionInfo", false, 104)]
        static void GenerateAssetsVersionInfo()
	    {
	        PackingBundlesPostProcess.GenerateAssetsVersionInfo();
            ClearResourcesBundleResDirectory();
	        AssetDatabase.Refresh();
        }

	    [MenuItem("Nireus/Move BundleRes To Resources", false, 201)]
	    static void MoveBundleResToResources()
	    {
	        ClearAssetBundleDirectory();
            FileUtil.MoveDirectory(PathConst.BUNDLE_RES, PathConst.RESOURCES);
            File.Move(PathConst.BUNDLE_RES_META, PathConst.RESOURCES_BUNDLE_RES_META);
            AssetDatabase.Refresh();
            Debug.Log("Move BundleRes To Resources Successfully.");
        }

	    [MenuItem("Nireus/Restore BundleRes from Resources", false, 202)]
	    static void RestoreBundleResFromResources()
	    {
	        FileUtil.MoveDirectory(PathConst.RESOURCES_BUNDLE_RES, PathConst.ASSETS);
	        File.Move(PathConst.RESOURCES_BUNDLE_RES_META, PathConst.BUNDLE_RES_META);
            AssetDatabase.Refresh();
            Debug.Log("Restore BundleRes from Resources Successfully.");
        }

        private static Dictionary<string, int> s_DependencyTracker = new Dictionary<string, int>();
        //[MenuItem("Nireus/创建AssetBundle冗余资源文件夹", false, 1001)]
        static void CreateDicWithBundleLabel()
        {
#if !ASSET_RESOURCES
            //PackageBundleRes.ClearAssetBundlesName();
            PackageBundleRes.package();
            PackageBundleRes.packageUIOriginalRes();
            DoDependencyTracker();
            ////创建文件夹
            var copy_root = GetRedundanceTempPath(true);
            foreach (var temp in s_DependencyTracker)
            {
                if (temp.Value > 1)
                {
                    //copy
                    string copy_path = (copy_root + temp.Key.Substring(0, temp.Key.LastIndexOf('/')));
                    if (System.IO.Directory.Exists(copy_path) == false)
                        System.IO.Directory.CreateDirectory(copy_path);
                    
                    //Debug.Log(temp.Key);
                    string ori_dic_path = temp.Key.Replace("Assets/", "");
                    string dic_path = (BUNDLE_REDUNDANCE_PATH + ori_dic_path.Substring(0, ori_dic_path.LastIndexOf('/')));
                    if (System.IO.Directory.Exists(dic_path) == false)
                        System.IO.Directory.CreateDirectory(dic_path);
                }
            }
            s_DependencyTracker.Clear();
            AssetDatabase.Refresh();
            Debug.Log("创建AssetBundle冗余资源文件夹 Successfully.");
#endif
        }

        //[MenuItem("Nireus/移动冗余资源到Bundle文件夹", false, 1002)]
        static void MoveResWithBundleLabel()
        {
#if !ASSET_RESOURCES
            DoDependencyTracker();
            
            var copy_root = GetRedundanceTempPath(false);
            foreach (var temp in s_DependencyTracker)
            {
                if (temp.Value > 1)
                {
                    //COPY
                    File.Copy(temp.Key,copy_root+temp.Key,true);
                    File.Copy(temp.Key + ".meta",copy_root+temp.Key + ".meta",true);
                    
                    //MOVE
                    string ori_dic_path = temp.Key.Replace("Assets/", "");
                    string error = AssetDatabase.MoveAsset(temp.Key, (BUNDLE_REDUNDANCE_PATH + ori_dic_path));
                    if (error != "")
                    {
                        Debug.LogError(error);
                    }
                }
                
            }
            s_DependencyTracker.Clear();
            AssetDatabase.Refresh();
            Debug.Log("移动冗余资源到Bundle文件夹 Successfully.");
#endif
        }
        [MenuItem("Nireus/移动冗余资源回原位", false, 1003)]
        static void ReturnResWithBundleLabel()
        {

            if (System.IO.Directory.Exists(BUNDLE_REDUNDANCE_PATH))
            {
                System.IO.Directory.Delete(BUNDLE_REDUNDANCE_PATH, true);
            }

            var copy_root = GetRedundanceTempPath(false);
            var copy_dic_path = copy_root + "Assets" + @"\";
            copy_dic_path = new System.IO.DirectoryInfo(copy_root + "Assets" + @"\").FullName;
            string target_path = new System.IO.DirectoryInfo("Assets/").FullName;
            CopyDireToDir(copy_dic_path, target_path);
            
            AssetDatabase.Refresh();
            Debug.Log("移动冗余资源回原位 Successfully.");
        }
        
        [MenuItem("Nireus/一键打冗余包(don't Clear)", false, 1004)]
        static void QuickPackBundles()
        {
            CreateDicWithBundleLabel();
            MoveResWithBundleLabel();
            PackBundlesWithoutClear();
        }
        
        [MenuItem("Nireus/一键打冗余包(with Clear)", false, 1005)]
        static void QuickPackBundlesWithClear()
        {
            CreateDicWithBundleLabel();
            MoveResWithBundleLabel();
            PackBundlesWithClear();
        }
        
        [MenuItem("Nireus/检查分包配置是否有交错", false, 1106)]
        static void CheckModuleConfig()
        {
            var config = UnityEditor.AssetDatabase.LoadAssetAtPath<ModulePackageConfigAsset>(PackageBundleRes.UI_MODULE_CONFIG_DIR);
            var ab_dic = config.ModulePackageListDictionary_AB;
            List<List<string>> two_list = new List<List<string>>();
            List<ModuleNameIndex> module_name_list = new List<ModuleNameIndex>();
            foreach (var pair in ab_dic)
            {
                two_list.Add(pair.Value);
                module_name_list.Add(pair.Key);
            }

            for (var i=0;i<two_list.Count-1;i++)
            {
                var a = two_list[i];
                for (var j=i+1;j<two_list.Count;j++)
                {
                    var b = two_list[j];
                    foreach (var a_s in a)
                    {
                        foreach (var b_s in b)
                        {
                            if (a_s == b_s)
                            {
                                Debug.LogErrorFormat("分包资源不正确，存在交叉，分包1={0}，分包2={1},重复ab={2}",module_name_list[i],module_name_list[j],a_s);
                            }
                        }
                    }
                }
            }
        }

        [MenuItem("Nireus/不移动冗余文件打包", false, 1106)]
        static void PackBundlesNew()
        {
            //DoDependencyTracker();
            var watch = new Stopwatch();
            watch.Start();
            PackageBundleRes.packageNew();
            //PackageBundleRes.packageUIOriginalResNew();
            
            const BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
            var builds = GetBuilds ();
            var assetBundleManifest = BuildPipeline.BuildAssetBundles (GetBundleOutputPath(), builds, options, EditorUserBuildSettings.activeBuildTarget);
            if (assetBundleManifest == null) {
                Debug.LogError("PackBundlesNew fail");
                return;
            }
            PackingBundlesPostProcess.Process();
            AssetDatabase.Refresh();
            watch.Stop();
            UnityEngine.Debug.Log("PackBundlesNew cost: " + watch.ElapsedMilliseconds + " ms.");
        }

        static bool IsInIgnoreDictionary(string path)
        {
            if (path.Contains("Assets/Res/BundleRes/") 
                || path.Contains("Assets/Scripts/") 
                || path.Contains("Packages/") || path.EndsWith(".cs") || path.Contains("Assets/Res/BundleRes_Future/")
                || path.Contains("Assets/Res/Textures/") || path.Contains("Assets/Res/EffectBossTemp/")
                || path.Contains("Assets/Res/MonsterResources/") || path.Contains("Assets/Libraries/")
                || path.Contains("Assets/Res/Effect/Model/") || path.Contains("Assets/Res/Effect_dy/Model/")
                || path.Contains("Assets/Res/EffectBossTemp/Model/") || path.Contains("Assets/Res/EffectHM/Model/")
                || path.Contains("Assets/Res/EffectsSJX/Models/") || path.Contains("Assets/Res/MonsterResources/Models/")
                || path.Contains("Assets/Res/TypeA/") || path.Contains("Assets/Res/Shaders/")
            )
            {
                return true;
            }
            return false;
        }
        static AssetBundleBuild[] GetBuilds()
        {
            var builds = new List<AssetBundleBuild>();
            Dictionary<string, HashSet<string>> bundlesMap = PackageBundleRes.bundlesMap;
            
            Dictionary<string,int> optimize = new Dictionary<string, int>();
            HashSet<string> rongyuData = new HashSet<string>();
            //找到冗余资源
            foreach (var data in bundlesMap)
            {
                //string key = data.Key;
                if (data.Value.Count != 0)
                {
                    //一个包里的内容  获取整个包里的相关文件
                    HashSet<string> dependenceAsset= new HashSet<string>();
                    foreach (var tempData in data.Value)
                    {
                        string[] dependence = AssetDatabase.GetDependencies(tempData);
                        dependenceAsset.AddRange(dependence.ToList());
                    }
                    foreach (var tempData in dependenceAsset)
                    {
                        if (optimize.ContainsKey(tempData))
                        {
                            //optimize[tempData] += 1;
                            if (IsInIgnoreDictionary(tempData))
                            {
                                continue;
                            }
                            rongyuData.Add(tempData);
                        }
                        else
                        {
                            optimize.Add(tempData,1);
                        }
                    }
                }
            }
            

            foreach (var data in rongyuData)
            {
                FileInfo fileInfo = new FileInfo(data);
                string assetBundleName = AssetBundleFilePath.GetAssetBundleName("Assets/",fileInfo);
                if (assetBundleName.Length != 0)
                {
                    string assetFullPath = data.Replace("\\", "/");
                    string assetBundlePath = assetFullPath.Substring(assetFullPath.IndexOf("Assets/") + "Assets/".Length);//去除PIC_PATH的内容保留后面的路径
                    string assetPath = "Assets/" + assetBundlePath;
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
                    
                }
            }
            
            foreach (var data in bundlesMap)
            {
                AssetBundleBuild bundle = new AssetBundleBuild();
                bundle.assetBundleName = data.Key.Replace('/','_');
                bundle.assetNames = data.Value.ToArray();
                builds.Add(bundle);
            }
            
            return builds.ToArray();
        }

        static void DoDependencyTracker()
        {
            s_DependencyTracker.Clear();
            string[] all_asset_bundle_name = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < all_asset_bundle_name.Length; i++)
            {
                string[] asset_path_from_bundle = AssetDatabase.GetAssetPathsFromAssetBundle(all_asset_bundle_name[i]);
                string[] dependence = AssetDatabase.GetDependencies(asset_path_from_bundle);
                for (int j = 0; j < dependence.Length; j++)
                {
                    if (dependence[j].Contains("Assets/Res/BundleRes/") || dependence[j].Contains("Assets/Scripts/")
                       || dependence[j].Contains("Packages/") || dependence[j].EndsWith(".cs") || dependence[j].Contains("Assets/Res/BundleRes_Future/")
                       || dependence[j].Contains("Assets/Res/Textures/") || dependence[j].Contains("Assets/Res/EffectBossTemp/")
                       || dependence[j].Contains("Assets/Res/MonsterResources/") || dependence[j].Contains("Assets/Libraries/")
                        || dependence[j].Contains("Assets/Res/Effect/Model/") || dependence[j].Contains("Assets/Res/Effect_dy/Model/")
                        || dependence[j].Contains("Assets/Res/EffectBossTemp/Model/") || dependence[j].Contains("Assets/Res/EffectHM/Model/")
                        || dependence[j].Contains("Assets/Res/EffectsSJX/Models/") || dependence[j].Contains("Assets/Res/MonsterResources/Models/")
                       || dependence[j].Contains("Assets/Res/TypeA/") || dependence[j].Contains("Assets/Res/Shaders/")
                       )
                    {
                        continue;
                    }
                    if (s_DependencyTracker.ContainsKey(dependence[j]))
                    {
                        s_DependencyTracker[dependence[j]]++;
                    }
                    else
                    {
                        s_DependencyTracker.Add(dependence[j], 1);
                    }
                }
            }
        }
        static void ClearAssetBundleDirectory()
        {
            System.IO.Directory.Delete(GetBundleOutputPath(), true);
            GetBundleOutputPath();
            System.IO.Directory.Delete(GetStreamingAssetsPath(), true);
            GetStreamingAssetsPath();

        }

	    static void ClearResourcesBundleResDirectory()
	    {
	        System.IO.Directory.Delete(GetResourcesBundleResPath(), true);
	        GetResourcesBundleResPath();
        }

        //生成到项目外部
        static string GetBundleOutputPath()
        {
            string path = new System.IO.DirectoryInfo(BUNDLE_OUT_PATH).Parent.FullName;
            if (System.IO.Directory.Exists(path) == false)
                System.IO.Directory.CreateDirectory(path);
            return path;
        }

        public static string GetStreamingAssetsPath()
        {
            string path = new System.IO.DirectoryInfo("Assets/StreamingAssets/").FullName;
            if (System.IO.Directory.Exists(path) == false)
                System.IO.Directory.CreateDirectory(path);
            return path;
        }

	    static string GetResourcesBundleResPath()
	    {
	        string path = new System.IO.DirectoryInfo("Assets/Resources/BundleRes/").FullName;
	        if (System.IO.Directory.Exists(path) == false)
	            System.IO.Directory.CreateDirectory(path);
	        return path;
        }

		static string GetRedundanceTempPath(bool clear)
	    {
            string EditRedundanceTempPath = FilePathHelper.getResFolderRW() + "/EditTemp/Redundance/"; // 存放更新文件的路径;
	        string path = EditRedundanceTempPath;
	        if (System.IO.Directory.Exists(path) == false){
                System.IO.Directory.CreateDirectory(path);
			}
			else{  
                DirectoryInfo dic_info = new DirectoryInfo(path);
                if (clear && !TimeUtil.isSameDay(dic_info.LastWriteTime,DateTime.Now))
                {
                    System.IO.Directory.Delete(path,true);
                    System.IO.Directory.CreateDirectory(path);
                }
            }
            
	        return path;
        } 
        
        /// <summary>
        /// 将一个文件夹下的所有东西复制到另一个文件夹 (可备份文件夹)
        /// </summary>
        /// <param name="sourceDir">源文件夹全名</param>
        /// <param name="destDir">目标文件夹全名</param>
        /// <param name="backupsDir">备份文件夹全名</param>
        public static void CopyDireToDir(string sourceDir, string destDir, string backupsDir = null)
        {
            if (Directory.Exists(sourceDir))
            {
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                DirectoryInfo sourceDireInfo = new DirectoryInfo(sourceDir);
                FileInfo[] fileInfos = sourceDireInfo.GetFiles();
                foreach (FileInfo fInfo in fileInfos)
                {
                    string sourceFile = fInfo.FullName;
                    string destFile = sourceFile.Replace(sourceDir, destDir);
                    
                    if (backupsDir != null && File.Exists(destFile))
                    {     
                        Directory.CreateDirectory(backupsDir);
                        string backFile = destFile.Replace(destDir, backupsDir);
                        File.Copy(destFile, backFile, true); 
                    }

                    if (!File.Exists(destFile))
                    {
                        File.Copy(sourceFile, destFile, true);
                    }
                }
                DirectoryInfo[] direInfos = sourceDireInfo.GetDirectories();
                foreach (DirectoryInfo dInfo in direInfos)
                {
                    string sourceDire2 = dInfo.FullName;
                    string destDire2 = sourceDire2.Replace(sourceDir, destDir);
                    string backupsDire2 = null;
                    if (backupsDir != null)
                    {
                        backupsDire2 = sourceDire2.Replace(sourceDir, backupsDir);
                    }
                    //Directory.CreateDirectory(destDire2);
                    CopyDireToDir(sourceDire2, destDire2, backupsDire2);
                }
            }
        }
    }
}
#endif