using System.IO;
using UnityEditor;

namespace Nireus.Editor
{
	public class AssetBundleAutoMgr : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(          
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			autoSetAssetBundleName(importedAssets);
			autoSetAssetBundleName(movedAssets);
		}
		
		static string RootDir = PathConst.BUNDLE_RES;
 
		/// <summary>
		/// 资源导入时自动设置AssetBundle name
		/// </summary>
		/// <param name="importedAssets"></param>
		static void autoSetAssetBundleName(string[] importedAssets)
		{
			foreach (var path in importedAssets)
			{
				//是否GameContent目录下
				int firstIndex = path.IndexOf(RootDir);
				if (firstIndex == -1)
				{
					var ab_name = AssetImporter.GetAtPath(path).assetBundleName;
					if ( ab_name != string.Empty)
					{
						AssetDatabase.RemoveAssetBundleName(ab_name,true);
					}
					continue;
				}

				var dir_root = RootDir;
				if (Directory.Exists(path))//判断是否是文件夹
				{
					//文件夹不用打AB信息
				}
				else
				{
					FileInfo file_info  = new FileInfo(path);
					PackageBundleRes.AddAssetBundleName(dir_root, file_info);
				}
			}
		}
	}
}