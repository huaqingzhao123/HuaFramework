using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nireus.Editor
{
	public class GameToolsEditor : UnityEditor.Editor
	{
		[MenuItem("Nireus/Check Prefab Binder",false, 1101)]
		private static void GetAllPrefabs()
		{
			List<GameObject> prefabs = new List<GameObject>();
			var resourcesPath = Application.dataPath;
			var absolutePaths = System.IO.Directory.GetFiles(resourcesPath, "*.prefab", System.IO.SearchOption.AllDirectories);
			for (int i = 0; i < absolutePaths.Length; i++)
			{
				EditorUtility.DisplayProgressBar("获取预制体……", "获取预制体中……", (float)i/absolutePaths.Length);

				string path = "Assets" + absolutePaths[i].Remove(0, resourcesPath.Length);
				path = path.Replace("\\", "/");
				GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
				if (prefab != null)
				{
					prefabs.Add(prefab);
					CheckBinderValid(prefab, path);
				}
				else
					Debug.Log("预制体不存在！ "+path);
			}
			EditorUtility.ClearProgressBar();
		}
		
		[MenuItem("Nireus/统计资源大图",false, 1102)]
		private static void GetAllAllBigTextures()
		{
			//预设文件名后缀
			string imgtype = "*.BMP|*.JPG|*.GIF|*.PNG";
			string[] ImageType = imgtype.Split('|');
			//存储图片路径
			string[] files = {};
			//Texture2D texture;
			for (int i = 0; i < ImageType.Length; i++)
			{
				//获取Application.dataPath文件夹下所有的图片路径  
				files = Directory.GetFiles("Assets/", ImageType[i], SearchOption.AllDirectories);
			}

			StreamWriter sw = new StreamWriter(Application.dataPath + "/../ui_res_image_toobig.log", false);
			sw.Write("==============================================================\r\n");
			for (int i = 0; i < files.Length; i++)
			{
				EditorUtility.DisplayProgressBar("获取图片信息……", "获取图片中……", (float)i/files.Length);

				Texture2D tx = new Texture2D(100, 100);
				tx.LoadImage(getImageByte(files[i]));
				if (tx.width > 512 || tx.height > 512)
				{
					var size = new Vector2(tx.width,tx.height);
					Debug.LogWarningFormat("This texture's size is too big  size={0} , path={1} " ,size.ToString(), files[i].ToString());
					sw.Write("texture size : " + size.ToString() + " texture path : " + files[i].ToString() + "\r\n");
				}
			}
			sw.Write("==============================================================\r\n");
			sw.Close();
			Debug.Log("finish:"+ Application.dataPath + "/../ui_res_image_toobig.log");
			
			EditorUtility.ClearProgressBar();
		}

        [MenuItem("Nireus/获取所有界面Prefab中文字", false, 1103)]
        public static void CheckTextFont()
        {
            HashSet<string> all_text = new HashSet<string>();
            string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] { $"{PathConst.BUNDLE_RES}UIPrefabs" });

            //string log_path = string.Format(Application.dataPath + "/../ui_all_text_{0}.csv", TimeUtil.now);
            //StreamWriter sw = new StreamWriter(log_path, false);
            //sw.Write("name\r\n");
            for (int i = 0; i < allPath.Length; i++)
            {
                EditorUtility.DisplayProgressBar("获取文字信息……", "获取文字中……", (float)i / allPath.Length);

                string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
                if (path.Contains("UnbelievableUIDialog"))
                {
                    continue;
                }
                var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (obj != null)
                {
                    var texts = obj.GetComponentsInChildren<Text>(true);
                    foreach (var text in texts)
                    {
                        var text_cut = text.text.Trim();
                        text_cut = text_cut.TrimEnd(new char[] { '\r', '\n' });//去除末尾换行符
                        if (CheckIsHaveHanZi(text_cut))
                        {
                            if (all_text.Add(text_cut))
                            {
                                var text_cut_change = text_cut.Replace("\r\n", @"<rn>");
                                text_cut_change = text_cut_change.Replace("\r", @"<rr>");
                                text_cut_change = text_cut_change.Replace("\n", @"<br>");
                                if (text_cut_change.IndexOf('，') >= 0)
                                {
                                    text_cut_change = string.Format("\"{0}\"", text_cut_change);
                                }
                                text_cut = text_cut.Replace(',', '，');
                                //sw.Write(text_cut_change + "\r\n");
                            }
                        }
                    }
                }
            }

            //sw.Close();

            var show_text = all_text;

            //Debug.Log("get all text finish:" + log_path);

            EditorUtility.ClearProgressBar();
        }

		#region tools
		//根据路径将文件转化为字节流
		private static byte[] getImageByte(string imagePath)
		{
			FileStream files = new FileStream(imagePath, FileMode.Open);
			byte[] imgByte = new byte[files.Length];
			files.Read(imgByte, 0, imgByte.Length);
			files.Close();
			return imgByte;
		}

		private static void CheckBinderValid(GameObject prefab,string path)
		{
			var binder = prefab.GetComponent<UIBinder>();
			if (binder)
			{
				var dic = binder.getItemMap();
				foreach (var item in dic)
				{
					if (item.Value == null)
					{
						Debug.LogError("预制体UIBinder error！ path = "+path);
						break;
					}
				}
			}
		}

		private static bool CheckIsHaveHanZi(string text)
		{
			if (text == String.Empty || text == "")
			{
				return false;
			}
				
			return Regex.IsMatch(text,@"[\u4e00-\u9fa5]") ;
		}
		#endregion
	}
}