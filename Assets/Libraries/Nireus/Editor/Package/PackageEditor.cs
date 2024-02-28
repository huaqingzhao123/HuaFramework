using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nireus.Editor
{
	class PackageEditor : UnityEditor.Editor
	{
		public static new BuildTarget target = BuildTarget.Android;
        const string BUNDLE_OUT_PATH = "Temp/AssetsBundle/BeforeEncryption/";
        const string TEXTURE_COMMON_FULL_DIR = "Assets/Textures/UI/Common/";

        struct UIResStats
        {
            public string res_path;
            public string res_name;
            public List<string> ref_path;

            public UIResStats(string res_name,string res_path, string ref_path_v)
            {
                this.res_path = res_path;
                this.res_name = res_name;
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
                        GameDebug.LogError(e);
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
        
        [MenuItem("Nireus/Clear Bundles", false, 101)]
        public static void ClearAllAssetBundleFiles()
        {
            ClearAssetBundleDirectory();
        }

        [MenuItem("Nireus/Generate AssetsVersionInfo", false, 102)]
        static void GenerateAssetsVersionInfo()
	    {
	        PackingBundlesPostProcess.GenerateAssetsVersionInfo();
            ClearResourcesBundleResDirectory();
	        AssetDatabase.Refresh();
        }

        [MenuItem("Nireus/检查分包配置是否有交错", false, 1106)]
        static public void CheckModuleConfig()
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
                                var des_text= string.Format("分包资源不正确，存在交叉，分包1={0}，分包2={1},重复ab={2}",module_name_list[i],module_name_list[j],a_s);
                                throw new Exception(des_text);
                            }
                        }
                    }
                }  
            }
            
            //遍历所有ab
            // string path = FilePathHelper.GetManifestAssetBundleFullPath();
            // Debug.Log("Manifest：  " + path);
            // AssetBundle ab = AssetBundle.LoadFromFile(path);
            // if (ab == null)
            // {
            //     var des_text = string.Format("Not found Manifest :" + path);
            //     throw new Exception(des_text);
            // }
            // var m_Manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            // if (m_Manifest == null)
            // {
            //     var des_text = string.Format("There is no AssetBundleManifest in file StreamingAssets");
            //     throw new Exception(des_text);
            // }
            // ab.Unload(false);
            
            
            
            Debug.LogFormat("分包资源配置检查通过");
        }

        [MenuItem("Nireus/一键打包(测试包)(自动去冗余)", false, 103)]
        static void PackBundlesWithoutCheck()
        {
			try
			{
				PublicCheckEditor.TestBuildCheck();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
			delayPackBundlesWithCheck();
		}

		[MenuItem("Nireus/一键打包(正式包)(自动去冗余)", false, 104)]
        static void PackBundlesWithCheck()
        {
            try
            {
                PublicCheckEditor.ZhengShiBuildCheck();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            delayPackBundlesWithCheck();
        }
        
        private static async void delayPackBundlesWithCheck(){
            await System.Threading.Tasks.Task.Delay(1000);
            if (EditorUtility.DisplayDialog("确认",
                "确认正式包参数，继续打包？", "继续", "取消"))
            {
                PackBundlesNew();

            }
            else
            {
                Debug.Log("==================================" + "手动取消打包");
            }
        }

        public static void PackBundlesNew()
        {
            //DoDependencyTracker();
            var watch = new Stopwatch();
            watch.Start();
            PackageBundleRes.packageNew();
            //PackageBundleRes.packageUIOriginalResNew();
            
            const BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;
            var builds = GetBuilds ();
            var assetBundleManifest = BuildPipeline.BuildAssetBundles (GetBundleOutputPath(), builds, options, EditorUserBuildSettings.activeBuildTarget);
            if (assetBundleManifest == null) {
                GameDebug.LogError("PackBundlesNew fail");
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
                string assetBundleNameT = assetBundleName.Replace("\\", "/");
                assetBundleNameT = assetBundleName.Substring(assetBundleName.IndexOf("Assets/") + "Assets/".Length);
                string realName = "Assets/Res/BundleRes/Redundance"+assetBundleNameT;
                if (realName.Length != 0)
                {
                    string assetFullPath = data.Replace("\\", "/");
                    string assetBundlePath = assetFullPath.Substring(assetFullPath.IndexOf("Assets/") + "Assets/".Length);//去除PIC_PATH的内容保留后面的路径
                    string assetPath = "Assets/" + assetBundlePath;
                    if (bundlesMap.TryGetValue(realName, out HashSet<string> assets))
                    {
                        assets.Add(assetPath);
                    }
                    else
                    {
                        HashSet<string> assetSet = new HashSet<string>();
                        assetSet.Add(assetPath);
                        bundlesMap.Add(realName, assetSet);
                    }
                    
                }
            }
            
            foreach (var data in bundlesMap)
            {
                AssetBundleBuild bundle = new AssetBundleBuild();
                bundle.assetBundleName = data.Key;//.Replace('/','_');
                bundle.assetNames = data.Value.ToArray();
                builds.Add(bundle);
            }
            
            return builds.ToArray();
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
