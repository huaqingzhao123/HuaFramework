using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace HuaFramework.Configs
{
    [Serializable]
    public class AutoGenConfig
    {
        public int Id;
#if UNITY_EDITOR
        public static string ConfigName = "\\GameConfig.xml";
        public static AutoGenConfig musicGameConfig = new AutoGenConfig();
        [MenuItem("Assets/生成游戏配置模板/自动生成配置")]
        [MenuItem("Tools/生成游戏配置模板/自动生成配置")]
        public static void SaveTemplate()
        {
            var id = Selection.assetGUIDs;
            if (id.Length == 0)
            {
                Debug.LogError("未选中任何文件夹!");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(id[0]);
            if (!Directory.Exists(path))
            {
                Debug.LogError("选中的不是文件夹，检查！");
                return;
            }
            path += ConfigName;
            if (File.Exists(path))
            {
                Debug.LogError("该路径下已经存在该配置，检查！");
                return;
            }
            var config = musicGameConfig;
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("UTF-8"));
            XmlSerializer xz = new XmlSerializer(config.GetType());
            xz.Serialize(sw, config);
            File.WriteAllBytes(path, ms.ToArray());
            AssetDatabase.Refresh();
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(object));
            Selection.activeObject = obj;
        }
#endif



        /// <summary>
        /// 保存此配置
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(GetType());
                xmlSerializer.Serialize(sw, this);
                File.WriteAllText(path, sw.ToString());
            }
        }
    }
    public struct Vector2Point
    {
        public float X;
        public float Y;
    }

}
