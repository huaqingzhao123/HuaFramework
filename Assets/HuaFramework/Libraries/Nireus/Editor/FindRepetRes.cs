using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class FindRepetRes : MonoBehaviour
{
    [MenuItem("Tools/Report/查找重复贴图")]
    static void ReportTexture()
    {
        Dictionary<string,string> md5dic = new Dictionary<string, string> ();
        string[] paths = AssetDatabase.FindAssets("t:prefab",new string[]{"Assets/Res/BundleRes"});

        foreach (var prefabGuid in paths) {
            string prefabAssetPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            string[] depend = AssetDatabase.GetDependencies (prefabAssetPath,true);
            for (int i = 0; i < depend.Length; i++) {
                string assetPath = depend [i];
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                //满足贴图和模型资源
                if (importer is TextureImporter || importer is ModelImporter) {
                    string md5 = GetMD5Hash(Path.Combine(Directory.GetCurrentDirectory(),assetPath));
                    string path;
				
                    if (!md5dic.TryGetValue (md5, out path)) {
                        md5dic [md5] = assetPath;
                    }else {
                        if (path != assetPath) {
                            Debug.LogFormat ("{0} {1} 资源发生重复！", path, assetPath);
                        }
                    }
                }
            }
        }
    }
	
    /// <summary>
    /// 获取文件Md5
    /// </summary>
    /// <returns>The M d5 hash.</returns>
    /// <param name="filePath">File path.</param>
    static string GetMD5Hash(string filePath)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        return BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(filePath))).Replace("-", "").ToLower();
    }

}
