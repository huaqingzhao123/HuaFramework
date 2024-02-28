//#define USE_ENCRYPTION

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Nireus.Editor
{
	public class PackingBundlesPostProcess
	{
		const string INPUT_ENCRYPTION_FILE_PATH = "Temp/AssetsBundle/";
		public const string OUT_ENCRYPTION_FILE_PATH = "Assets/StreamingAssets/";
        
#if UNITY_ANDROID
        public const string WWISE_PROJECT_PATH = "client_WwiseProject/GeneratedSoundBanks/Android/";
#else
        public const string WWISE_PROJECT_PATH = "";
#endif
        static int total_cnt = 0;
        static int succ_cnt = 0;
        
        public static void Process()
        {
            total_cnt = 0;
            succ_cnt = 0;
            string dir = new DirectoryInfo(INPUT_ENCRYPTION_FILE_PATH).FullName;
            EncryptionDir(dir);
            GenerateAssetsVersionInfo();
           // FileUtil.DelDirectory(ENCRYPTION_FILE_PATH);
          //  Directory.CreateDirectory(ENCRYPTION_FILE_PATH);
            Debug.Log("PackingBundlesPostProcess Complete: succ/total: " + succ_cnt + "/" + total_cnt);
		}

        static void EncryptionDir(string dir_path)
        {
            {
                DirectoryInfo directory_info = new DirectoryInfo(dir_path);
                FileInfo[] files = directory_info.GetFiles();

                for (int i = 0; i < files.Length; ++i)
                {
                    try
                    {
                        if (files[i].Extension == ".meta") continue;
                        ++total_cnt;
                        encryption(dir_path, files[i].Name);
                        ++succ_cnt;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Log("Encryption.encryption catch exception: " + ex.Message);
                    }
                }
            }

            {
                DirectoryInfo directory_info = new DirectoryInfo(dir_path);
                DirectoryInfo[] dirs = directory_info.GetDirectories();

                for (int i = 0; i < dirs.Length; ++i)
                {
                    EncryptionDir(dir_path + dirs[i].Name + "/");
                }
            }
        }


        static void encryption(string dir_path, string file_name)
		{
#if USE_ENCRYPTION
            string outFullFileName = OUT_ENCRYPTION_FILE_PATH + Nireus.Encryption.md5(dir_path.Replace(ENCRYPTION_FILE_PATH, "") + file_name).Replace("/", "_");
            FileStream in_file = new FileStream(dir_path + file_name, FileMode.Open);
			byte []file_data = new byte[in_file.Length];
			in_file.Read(file_data, 0, (int)in_file.Length);
			in_file.Close();

            Nireus.Encryption.xor(file_data);
			FileStream out_file = new FileStream(outFullFileName, FileMode.OpenOrCreate);
			out_file.Write(file_data, 0, file_data.Length);
			out_file.Close();
#else
            string outFullFileName = dir_path.Replace("\\", "/");
            outFullFileName = outFullFileName.Substring(outFullFileName.IndexOf(INPUT_ENCRYPTION_FILE_PATH) + INPUT_ENCRYPTION_FILE_PATH.Length);
            outFullFileName = OUT_ENCRYPTION_FILE_PATH + outFullFileName.Replace("/", "_") + file_name;
            if (File.Exists(outFullFileName))
            {
                File.Delete(outFullFileName);
            }
            File.Copy(dir_path + file_name, outFullFileName);
#endif
        }

        public static string md5(string encrypt_string)
        {
#if USE_ENCRYPTION
            if (string.IsNullOrEmpty(encrypt_string)) throw new System.Exception("encrypt_string is null.");
            return md5(Encoding.Default.GetBytes(encrypt_string));
#else
            return encrypt_string;
#endif
        }

        public static string md5(byte[] encrypte_data)
        {
            string md5_str = string.Empty;
			if (encrypte_data == null) throw new System.Exception("encrypt_string is null.");
			MD5 md5_class = new MD5CryptoServiceProvider();
			md5_str = System.BitConverter.ToString(md5_class.ComputeHash(encrypte_data)).Replace("-", "");
            return md5_str;
        }

	    public static void GenerateAssetsVersionInfo()
	    {
#if !ASSET_RESOURCES  && (UNITY_ANDROID || USE_SUB_PACKAGE)
            var assetsVersionInfo = new AssetsVersionInfo();
	        assetsVersionInfo.version = Application.version;
            assetsVersionInfo.assets = new Dictionary<string, AssetBundleInfo>();

            //to md5 
            
	        var md5Provider = new MD5CryptoServiceProvider();
            List<string> temp_dir_list = new List<string>();
            temp_dir_list.Add(OUT_ENCRYPTION_FILE_PATH);
            if (WWISE_PROJECT_PATH != "")
            {
                temp_dir_list.Add(WWISE_PROJECT_PATH);
            }
            for (var i = 0;i< temp_dir_list.Count;i++)
            {
                if (Directory.Exists(temp_dir_list[i]) == false) continue;
                var dirInfo = new DirectoryInfo(temp_dir_list[i]);
                var files = dirInfo.GetFiles();
                foreach (var file in files)
                {
                    var fileName = file.Name;
                    if (fileName.EndsWith(".meta"))
                    {
                        continue;
                    }
                    if (i == 0)
                    {
                        var need = fileName == "AssetsBundle" || fileName.EndsWith(".ab");
                        if (!need) continue;
                    }
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
                    var abInfo = new AssetBundleInfo { size = size, md5 = md5 };
                    assetsVersionInfo.assets[file.Name] = abInfo;
                }
            }
            
	        
	        

	        var serialized = assetsVersionInfo.ToString();
	        var path = OUT_ENCRYPTION_FILE_PATH + PathConst.ASSETS_VERSION_INFO_FILENAME;
	        File.WriteAllText(path, serialized);

            Debug.Log($"{PathConst.ASSETS_VERSION_INFO_FILENAME} generated.");
#else
            Debug.Log($"Can't generate in Resources mode.");
#endif
        }
        
    }
}
