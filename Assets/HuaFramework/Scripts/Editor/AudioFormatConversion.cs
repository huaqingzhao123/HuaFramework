using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HuaFramework.Utility
{
    /// <summary>
    /// 音频转换
    /// </summary>
    public class AudioFormatConversion
    {

        [MenuItem("Tools/音频转换/转换为mp3,支持ogg,wav格式")]
        static void AudioToMp3()
        {
            var assetes = GetSelectAssets();
            for (int i = 0; i < assetes.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetes[i]);
                Debug.LogErrorFormat("文件路径:{0},资源路径:{1}", assetPath, Application.dataPath);
                WavToMp3(assetPath);
                OggToMp3(assetPath);
            }
            AssetDatabase.Refresh();
        }
        [MenuItem("Tools/音频转换/转换为ogg,支持mp3,wav格式")]
        static void AudioToOgg()
        {
            var assetes = GetSelectAssets();
            for (int i = 0; i < assetes.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetes[i]);
                if (assetPath.EndsWith(".WAV", StringComparison.OrdinalIgnoreCase))
                {
                    //如果是wave先转为mp3
                    var newPath = WavToMp3(assetPath);
                    //再从mp3转为ogg
                    Mp3ToOgg(newPath);
                }
                else
                    //从mp3转为ogg
                    Mp3ToOgg(assetPath);
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/音频转换/转换为wav,支持mp3,ogg格式")]
        static void AudioToWave()
        {
            var assetes = GetSelectAssets();
            for (int i = 0; i < assetes.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetes[i]);
                //是ogg先转为mp3
                if (assetPath.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                {
                    //如果是ogg先转为mp3
                    var newPath = OggToMp3(assetPath);
                    //再从mp3转为wave
                    Mp3TpWave(newPath);
                }
                else
                    //从mp3转为wave
                    Mp3TpWave(assetPath);
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// mp3转换为wav
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static string Mp3TpWave(string assetPath)
        {
            if (assetPath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                var tempPath = Regex.Replace(assetPath, @"\.mp3", ".Wav", RegexOptions.IgnoreCase);
                //Debug.LogError("新文件路径为:" + path);
                if (!File.Exists(tempPath))
                {
                    try
                    {
                        //FileStream _fileStream = File.Open(assetPath, FileMode.Open);
                        Mp3FileReader _mp3FileReader = new Mp3FileReader(assetPath);
                        WaveFileWriter.CreateWaveFile(tempPath, _mp3FileReader);
                        //_fileStream.Close();
                        _mp3FileReader.Close();
                        File.Delete(assetPath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("报错:{0},新文件路径为:{1}", e.Message, tempPath);
                    }
                }
            }
            return "";
        }
        static string WavToMp3(string assetPath)
        {
            if (assetPath.EndsWith(".WAV", StringComparison.OrdinalIgnoreCase))
            {
                var tempPath = Regex.Replace(assetPath, @"\.WAV", ".mp3", RegexOptions.IgnoreCase);
                if (!File.Exists(tempPath))
                {
                    try
                    {
                        FileStream _fileStream = File.Open(assetPath, FileMode.Open);
                        WaveFileReader _mp3FileReader = new WaveFileReader(_fileStream);
                        WaveFileWriter.CreateWaveFile(tempPath, _mp3FileReader);
                        _fileStream.Close();
                        File.Delete(assetPath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("报错:{0}", e.Message);
                    }
                    Debug.LogErrorFormat("新文件路径为:{0}", tempPath);
                }
                return tempPath;
            }
            return "";
        }
        static string OggToMp3(string assetPath)
        {
            if (assetPath.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                var tempPath = Regex.Replace(assetPath, @"\.ogg", ".mp3", RegexOptions.IgnoreCase);
                if (!File.Exists(tempPath))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(assetPath);
                        File.WriteAllBytes(tempPath, bytes);
                        File.Delete(assetPath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("报错:{0}", e.Message);
                    }
                    Debug.LogErrorFormat("新文件路径为:{0}", tempPath);
                }
                return tempPath;
            }
            return "";
        }



        static void Mp3ToOgg(string assetPath)
        {
            if (assetPath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                var tempPath = Regex.Replace(assetPath, @"\.mp3", ".ogg", RegexOptions.IgnoreCase);
                if (!File.Exists(tempPath))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(assetPath);
                        File.WriteAllBytes(tempPath, bytes);
                        File.Delete(assetPath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("报错:{0}", e.Message);
                    }
                    Debug.LogErrorFormat("新文件路径为:{0}", tempPath);
                }
            }
        }

        static string[] GetSelectAssets()
        {
            var assetes = Selection.assetGUIDs;
            if (assetes.Length <= 0)
                EditorUtil.ShowDialog("提示", "\n\n\t请选择至少一个音频文件", "Ok");
            return assetes;
        }
    }
}

