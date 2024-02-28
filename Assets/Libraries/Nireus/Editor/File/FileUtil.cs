using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Nireus.Editor
{
	public class FileUtil
	{
		static private FileUtil _instance;
		static public FileUtil getInstance() { return _instance == null ? _instance = new FileUtil() : _instance; }

		// 从Assets文件夹中读取文件;
		public Object loadObject(string file_path)
		{
			Object obj = AssetDatabase.LoadAssetAtPath(file_path, typeof(Object));
			if (obj == null)
			{
				Debug.LogError("File Not Found: " + file_path);
				return null;
			}
			return obj;
		}

		public string loadString(string file_path)
		{
			TextAsset text_asset = loadObject(file_path) as TextAsset;
			if (text_asset == null)
			{
				Debug.Log("File Not Found: " + file_path);
				return null;
			}
			return text_asset.text;
		}

		public byte[] loadByte(string file_path)
		{
			return System.Text.Encoding.ASCII.GetBytes(loadString(file_path));
		}
#if !ASSET_RESOURCES
        public static byte[] LoadBytes(string file_name)
        {
            FileStream file = File.Open(file_name, FileMode.Open);
            BinaryReader reader = new BinaryReader(file);
            byte[] ret = reader.ReadBytes((int)file.Length);
            file.Close();
            return ret;
        }
        public static void SaveBytes(byte[] bytes, string file_name)
        {
            FileStream file = File.Open(file_name, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(bytes);
            file.Close();
        }

        public static void SaveString(string content, string file_name)
        {
            FileStream file = File.Open(file_name, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(System.Text.Encoding.UTF8.GetBytes(content));
            file.Close();
        }
#endif
        public Texture2D loadTexture2D(string file_path)
		{
			return loadObject(file_path) as Texture2D;
		}

		public Sprite loadSpriteDirectly(string file_path)
		{
			return AssetDatabase.LoadAssetAtPath<Sprite>(file_path);
		}

		public Sprite loadSprite(string file_path)
		{
			Texture2D texture = loadTexture2D(file_path);
			if (texture == null) return null;
			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}

		public Sprite loadScale9Sprite(string file_path, float left, float bottom, float right, float top)
		{
			Texture2D texture2d = loadTexture2D(file_path);
			if (texture2d == null) return null;
			Vector4 border = new Vector4(left, bottom, right, top);
			return Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
		}

		public GameObject loadPrefab(string file_path)
		{
			return loadObject(file_path) as GameObject;
		}




        private static void CopyDirectory(DirectoryInfo srcDirectory, DirectoryInfo destParentDirectory, bool ignoreMeta = false)
        {
            string newDirectoryFullName = destParentDirectory.FullName + @"/" + srcDirectory.Name;

            if (!Directory.Exists(newDirectoryFullName))
                Directory.CreateDirectory(newDirectoryFullName);

            FileInfo[] oldFileArray = srcDirectory.GetFiles();
            foreach (FileInfo file in oldFileArray)
            {
                if (ignoreMeta && file.Name.EndsWith(".meta")) continue;
                File.Copy(file.FullName, newDirectoryFullName + @"/" + file.Name, true);
            }

            DirectoryInfo[] oldDirectoryArray = srcDirectory.GetDirectories();
            foreach (DirectoryInfo dir in oldDirectoryArray)
            {
                DirectoryInfo aNewDirectory = new DirectoryInfo(newDirectoryFullName);
                CopyDirectory(dir, aNewDirectory);
            }
        }

        //CopyDirectory
        public static void CopyDirectory(string srcDirectoryPath, string destParentDirectoryPath, bool ignoreMeta = false)
        {
            DirectoryInfo srcDir = new DirectoryInfo(srcDirectoryPath);
            DirectoryInfo destParentDir = new DirectoryInfo(destParentDirectoryPath);
            CopyDirectory(srcDir, destParentDir, ignoreMeta);
        }

        //DelDirectory
        public static void DeleteDirectory(string srcDirectoryPath)
        {
            DirectoryInfo oldDirectory = new DirectoryInfo(srcDirectoryPath);
            oldDirectory.Delete(true);
        }

        //CopyDirectory And DelDirectory
        public static void MoveDirectory(string OldDirectory, string NewDirectory)
        {
            CopyDirectory(OldDirectory, NewDirectory);
            DeleteDirectory(OldDirectory);
        }

        public static string GetMd5Dir()
        {
            string libraryDir = Directory.GetParent(Application.dataPath).FullName + "/Library/MD5FileCache/";
            return libraryDir;
        }

#if !ASSET_RESOURCES
        public static bool CheckMD5Changed(string file_path)
        {
            string json_str = getInstance().loadString(file_path);
            
            string libraryDir = GetMd5Dir() + Path.GetDirectoryName(file_path);
            string md5FileName = Path.GetFileNameWithoutExtension(file_path) + "_md5.txt";
            string md5FilePath = libraryDir + "/" + md5FileName;

            if (string.IsNullOrEmpty(json_str))
            {//清理不存在的文件.
                File.Delete(md5FilePath);
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(md5FilePath));

            string md5_str = null;
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(md5FilePath);
                md5_str = sr.ReadToEnd();
                sr.Close();
                sr = null;
            }
            catch
            {
                if(sr != null) sr.Close();
                sr = null;
            }

            if (string.IsNullOrEmpty(md5_str))
            {
                StreamWriter sw = new StreamWriter(md5FilePath);
                sw.Write(PackingBundlesPostProcess.md5(System.Text.Encoding.Default.GetBytes(json_str)));
                sw.Close();
                sw = null;
            }
            else
            {
                string now_md5_str = PackingBundlesPostProcess.md5(System.Text.Encoding.Default.GetBytes(json_str));
                if (md5_str == now_md5_str)
                {
                    return false;
                }
                else
                {
                    StreamWriter sw = new StreamWriter(md5FilePath);
                    sw.Write(PackingBundlesPostProcess.md5(System.Text.Encoding.Default.GetBytes(json_str)));
                    sw.Close();
                    sw = null;
                }
            }
            return true;
        }

#endif
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


    }
}