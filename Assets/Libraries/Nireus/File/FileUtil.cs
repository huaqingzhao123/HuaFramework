#define USE_ASSETBUNDLE

using UnityEngine;
using System.IO;
using System.Collections;
using System;

namespace Nireus
{
    public class FileUtil
    {
        static private FileUtil _instance;
        static public FileUtil getInstance() { return _instance == null ? _instance = new FileUtil() : _instance; }
        
        // 对文件全路径进行md5加密;
        public string md5FilePath(string file_path)
        {
            int index = file_path.LastIndexOf("/");
            string path = "";
            string file_name = file_path;
            if (index != -1)
            {
                path = file_path.Substring(0, index + 1);
                file_name = Encryption.md5(file_path.Substring(index + 1));
            }
            return path + "/" + file_name;
        }

        // 读取二进制文件;
        public byte[] loadFile(string file_path)
        {
            //file_path = md5FilePath(file_path);

            byte[] file_data = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                file_data = new byte[fs.Length];
                fs.Read(file_data, 0, (int)fs.Length);
                fs.Close();

                Encryption.xor(file_data);
            }
            catch (System.Exception ex)
            {
                GameDebug.LogError("FileUtil::loadFile catch exception: " + ex.Message + ", path=" + file_path);
            }
            finally
            {
                if (fs != null) fs.Close();
            }

            return file_data;
        }
        public bool tryLoadFile(string file_path, out byte[] file_data)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                file_data = new byte[fs.Length];
                fs.Read(file_data, 0, (int)fs.Length);
                fs.Close();

                Encryption.xor(file_data);
            }
            catch (System.Exception ex)
            {
                file_data = null;
                return false;
            }
            finally
            {
                if (fs != null) fs.Close();
            }

            return true;
        }
        // 读取字符串文件;
        public string loadFileString(string file_path)
        {
            byte[] file_data = loadFile(file_path);
            if (file_data == null) return null;
            return System.Text.Encoding.UTF8.GetString(file_data);
        }

        public string loadUTF8String(string file_path)
        {
            byte[] file_data;
            if (tryLoadFile(file_path, out file_data))
            {
                return System.Text.Encoding.UTF8.GetString(file_data);
            }
            return "";
        }

        public void deleteFile(string file_path)
        {
            if (File.Exists(file_path)) File.Delete(file_path);
        }

        // 保存二进制文件;
        public void saveFile(string file_path, byte[] file_data)
        {
            //file_path = md5FilePath(file_path);

            FileStream fs = null;
            try
            {
                if (File.Exists(file_path)) File.Delete(file_path);

                fs = new FileStream(file_path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                Encryption.xor(file_data);

                fs.Write(file_data, 0, file_data.Length);
            }
            catch (System.Exception ex)
            {
                GameDebug.LogError("FileUtil::saveFile catch exception: " + ex.Message);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        // 保存字符串文件;
        public void saveFileString(string file_path, string file_data_str)
        {
            saveFile(file_path, System.Text.Encoding.UTF8.GetBytes(file_data_str));
        }

/*
        // 同步读取一个assetbundle;
        public AssetBundle loadAssetBundle(string asset_bundle_path)
        {
            AssetBundle asset_bundle = AssetBundle.LoadFromMemory(loadFile(asset_bundle_path));
            return asset_bundle;
        }
        
        public GameObject loadTemplatePrefab(string template_name)
        {
            GameObject template_obj = null;

#if USE_ASSETBUNDLE

#else
			const string TEMPLATE_PREFAB_PATH = "Assets/Prefabs/UI/UITemplates/";
			template_obj = UnityEditor.AssetDatabase.LoadAssetAtPath(TEMPLATE_PREFAB_PATH + template_name, typeof(GameObject)) as GameObject;
#endif
            return GameObject.Instantiate(template_obj);
        }


        public IEnumerator CopySteamingFileToRWPath_Co(string fileName, string toName, System.Action finishCB)
        {
            WWW www = new WWW(GetStreamingPath(true) + fileName);
            while (!www.isDone) yield return www;
            SaveFileToRWPath(toName, www.bytes);
            finishCB();
        }

        public void SaveFileToRWPath(string file_name, byte[] fileData)
        {
            string target_file_path = FilePathHelper.PATCH_GAME_RES_DIR + "/" + file_name;
            string full_dir_path = Path.GetDirectoryName(target_file_path);
            if (File.Exists(target_file_path)) File.Delete(target_file_path);
            if (Directory.Exists(full_dir_path) == false) Directory.CreateDirectory(full_dir_path);
            try
            {
                FileStream fs = new FileStream(target_file_path, FileMode.Create);
                fs.Write(fileData, 0, fileData.Length);
                fs.Close();
            }
            catch (Exception e)
            {
                GameDebug.LogError("SaveFileToRWPath, target_file_path=" + target_file_path + " error: " + e.Message);
            }
        }
*/

        public string GetStreamingPath(bool useForWWW)
        {
            string path = string.Empty;
#if UNITY_EDITOR
            path = (useForWWW ? "file://" : "") + Application.dataPath + "/StreamingAssets/";
#elif UNITY_IOS
            path = (useForWWW ? "file://" : "") + Application.dataPath + "/Raw/";
#elif UNITY_ANDROID
            path = "jar:file://" + Application.dataPath + "!/assets/" ;
#else
            path =  Application.dataPath + "/StreamingAssets/" ;
#endif
            return path;

        }
#if UNITY_ANDROID || USE_SUB_PACKAGE
        public string LoadStreamOrPatchTextAsset(string fileName)
        {
            string patch_path = FilePathHelper.GetPatchFilePath(fileName);

            if (string.IsNullOrEmpty(patch_path) == false)
            {
                return loadUTF8String(patch_path);
            }
            else
            {
                WWW www = new WWW(Path.Combine(GetStreamingPath(true),fileName));
                while (!www.isDone)
                {
                    if (string.IsNullOrEmpty(www.error) == false)
                    {
                        break;
                    }
                }
                return www.text;
            }
        }
#endif
        public string LoadPatchTextAsset(string fileName)
        {
            string patch_path = Path.Combine(FilePathHelper.PATCH_GAME_RES_DIR, fileName);

            if (File.Exists(patch_path))
            {
                return loadUTF8String(patch_path);
            }
            return "";
        }

        public string LoadModulePatchTextAsset(string moduleName,string fileName)
        {
            string patch_path = Path.Combine(FilePathHelper.PATCH_GAME_RES_MODULE_DIR, moduleName,fileName);

            if (File.Exists(patch_path))
            {
                return loadUTF8String(patch_path);
            }
            return "";
        }
        
    }
}