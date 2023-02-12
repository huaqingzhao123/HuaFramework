using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using Object = UnityEngine.Object;

namespace Nireus.Editor
{

    public class AnimFileOptimizer
    {
        static int num = 3;   //压缩精度
        static List<string> listPath = new List<string>();  //存储anim文件的路径
        static List<AnimationClip> listClips = new List<AnimationClip>();

        [MenuItem("Nireus/Optimize Anim Files")]
        public static void Optimize()
        {
            FindAnims();
            CompressAll();
        }

        private static void FindAnims()
        {
            var objs = Selection.GetFiltered<AnimationClip>(SelectionMode.DeepAssets);
            //UnityEngine.Object[] objs = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
            if (objs.Length > 0)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    //if (objs[i].GetType() != typeof(AnimationClip)) continue;
                    string path = AssetDatabase.GetAssetPath(objs[i]);
                    listPath.Add(path);
                }
            }

        }
        public static void CompressAll()
        {

            //for (int i = 0; i < listClips.Count; i++)
            //{
            //    CompressAnimationClip(listClips[i], i);
            //}

            for (int i = 0; i < listPath.Count; i++)
            {
                CompressFile(listPath[i], i);
            }

            EditorUtility.ClearProgressBar();
            Resources.UnloadUnusedAssets();
            AssetDatabase.SaveAssets();
            listPath.Clear();
            GC.Collect();

        }


        private static void CompressAnimationClip(AnimationClip clip, int index)
        {
            /*
            var curveArr = AnimationUtility.GetCurveBindings(clip);
            Keyframe keyframe;
            Keyframe[] keyframeArr;

            for (int i = 0; i < curveArr.Length; i++)
            {
                var curveData = curveArr[i];
            }
            */
        }

        private static void CompressFile(string path, int index)
        {
            try
            {
                Debug.Log("Compress " + path);
                var filename = Path.GetFileName(path);

                EditorUtility.DisplayProgressBar("AnimFileOptimizer", "Compressing " + filename,
                    ((float) index / listPath.Count));

                //File.Delete(path);
                var targetDir = Path.GetDirectoryName(path) + "/Temp/";
                var targetPath = targetDir + filename;
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                StreamReader sr = new StreamReader(path, Encoding.UTF8);
                FileStream fs = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                sw.Flush();
                sw.BaseStream.Seek(0, SeekOrigin.Begin);
                while (!sr.EndOfStream)
                {
                    var lineText = sr.ReadLine();
                    if (lineText.Contains("time"))
                    {
                        string[] txts = lineText.Split(':');
                        if (txts != null)
                        {
                            if (txts[1].Contains(".") && (txts[1].Length - txts[1].IndexOf('.') - 1) >= num)
                            {
                                txts[1] = float.Parse(txts[1]).ToString("f" + num);
                                if (float.Parse(txts[1]) == 0)
                                {
                                    txts[1] = "0";
                                }

                            }

                            lineText = txts[0] + ": " + txts[1];
                        }
                    }

                    if (lineText.Contains("value:") ||
                        lineText.Contains("inSlope:") ||
                        lineText.Contains("outSlope:") ||
                        lineText.Contains("inWeight:") ||
                        lineText.Contains("outWeight:"))
                    {
                        //lineText.Trim();
                        int frontindex = lineText.IndexOf('{');

                        if (frontindex >= 0)
                        {
                            int behindindex = lineText.IndexOf('}');

                            while (behindindex < 0)
                            {
                                lineText += sr.ReadLine();
                                lineText = lineText.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                                behindindex = lineText.IndexOf('}');
                            }

                            string beginstr = lineText.Substring(0, frontindex);

                            string str = lineText.Substring(frontindex + 1, behindindex - frontindex - 1);

                            string[] txts = str.Split(',');
                            string tt_new = "";
                            for (int k = 0; k < txts.Length; k++)
                            {
                                string[] newstr = txts[k].Split(':');
                                var processed = ProcessFloatString(newstr[1]);

                                tt_new += newstr[0] + ": " + processed + (k == txts.Length - 1 ? "" : ",");

                            }

                            lineText = beginstr + "{" + tt_new + "}";
                        }
                        else
                        {
                            string[] kvArr = lineText.Split(':');
                            if (kvArr.Length == 2)
                            {
                                var vStr = kvArr[kvArr.Length - 1].Trim();
                                var processed = ProcessFloatString(vStr);
                                lineText = kvArr[0] + ": " + processed;
                            }
                        }
                        
                    }

                    sw.WriteLine(lineText);
                }

                sw.Flush();
                sw.Close();
                sr.Close();
            }
            catch (Exception e)
            {
                GameDebug.LogError(e);
            }
        }

        private static string ProcessFloatString(string fStr)
        {
            string result = fStr;
            if (fStr.Contains(".") && (fStr.Length - fStr.IndexOf('.') - 1) >= num)
            {
                var parsed = float.Parse(fStr);
                result = parsed.ToString("f" + num);
                if (Mathf.Abs(parsed) < 0.0001f)
                {
                    result = "0";
                }
            }

            return result;
        }

        [MenuItem("Nireus/ExportAnimFromFbx")]
        public static void ExportAnimFromFbx()
        {
            var objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            foreach(var asset in objects)
            {
				string asset_path = AssetDatabase.GetAssetPath(asset);
				string suffix = asset_path.Substring(asset_path.Length - 4);
				if (suffix != ".FBX" && suffix != ".fbx")
					continue;
                var fbxAnim = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(asset),typeof(AnimationClip)) as AnimationClip;
                if (!fbxAnim) continue;
                var path = AssetDatabase.GetAssetPath(asset).Replace(suffix, ".anim");
                var anim = new AnimationClip();
                EditorUtility.CopySerialized(fbxAnim, anim);
                AssetDatabase.CreateAsset(anim, path);
            }
            AssetDatabase.Refresh();
        }
    }

   

}
