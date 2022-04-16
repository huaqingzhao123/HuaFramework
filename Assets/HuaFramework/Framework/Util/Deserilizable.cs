using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace HuaFramework.Utility
{
    public class Deserilizable
    {
        public static T DeserilizableXml<T>(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogErrorFormat("路径:{0}不存在，检查！", path);
                return default(T);
            }
            FileStream fs = File.Open(path, FileMode.Open);
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
            {
                XmlSerializer xz = new XmlSerializer(typeof(T));
                T dept = (T)xz.Deserialize(sr);
                return dept;
            }
        }
    }

}
