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
        /// <summary>
        /// 上一个人和下一个人的间隔时间
        /// </summary>
        public float EveryOneDeltaTime;
        /// <summary>
        /// 上面第一个按钮相对与初始的偏移量
        /// </summary>
        public Vector2Point TheUpOnePos;
        /// <summary>
        /// 上面一行的每个按钮的间隔距离
        /// </summary>
        public float UpDeltaDistance;
        /// <summary>
        /// 下面第一个按钮相对与初始的偏移量
        /// </summary>
        public Vector2Point TheDownOnePos;
        /// <summary>
        /// 下面一行的每个按钮的间隔距离
        /// </summary>
        public float DownDeltaDistance;
        /// <summary>
        /// 上面行的按钮数量
        /// </summary>
        public int UpRawButtonNumer;

        /// <summary>
        /// 下面行的按钮数量
        /// </summary>
        public int DownRawButtonNumer;

#if UNITY_EDITOR
        [MenuItem("Assets/生成游戏配置模板/趣味蛙跳")]
        [MenuItem("Tools/生成游戏配置模板/趣味蛙跳")]
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
            path += "\\LeapFrogConfig.xml";
            if (File.Exists(path))
            {
                Debug.LogError("该路径下已经存在该配置，检查！");
                return;
            }
            var config = new AutoGenConfig();
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("UTF-8"));
            XmlSerializer xz = new XmlSerializer(config.GetType());
            xz.Serialize(sw, config);
            File.WriteAllBytes(path, ms.ToArray());
            AssetDatabase.Refresh();
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(object));
            Selection.activeObject = obj;
            //using (StringWriter sw = new StringWriter())
            //{
            //    var config = new AutoGenConfig();
            //    XmlSerializer xmlSerializer = new XmlSerializer(config.GetType());
            //    xmlSerializer.Serialize(sw, config);
            //    File.WriteAllText(path, sw.ToString());
            //    AssetDatabase.Refresh();
            //    var obj = AssetDatabase.LoadAssetAtPath(path, typeof(object));
            //    Selection.activeObject = obj;
            //}
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
