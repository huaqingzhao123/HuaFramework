using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
#if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

[Serializable]
public class LocalizationConfig
{

    public Dictionary<string,string> LocalizationsDictionary;

#if UNITY_EDITOR
    [MenuItem("Assets/生成本地化配置")]
    public static void GenerateLocalizationConfig()
    {
        var selecton = Selection.activeObject;
        if (selecton != null)
        {
            var path = AssetDatabase.GetAssetPath(selecton);
            if (!string.IsNullOrEmpty(path))
            {
                if (!Directory.Exists(path))
                    return;
                LocalizationConfig localizationConfig = new LocalizationConfig();
                //生成配置文件
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LocalizationConfig));
                path = path + "/LocalizationConfig.xml";
                if (File.Exists(path))
                    return;
                using (StreamWriter streamWriter = new StreamWriter(File.Create(path)))
                {
                    xmlSerializer.Serialize(streamWriter, localizationConfig);
                }
                AssetDatabase.Refresh();
            }
        }
    }

    public static LocalizationConfig LoadConfig(string path)
    {
        LocalizationConfig config;
        using (StreamReader streamReader = new StreamReader(File.OpenRead(path)))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(LocalizationConfig));
            config = xmlSerializer.Deserialize(streamReader) as LocalizationConfig;
        }
        return config;
    }
#endif
}

public struct TextLocalization
{

    public string CNText;
    public string EN_USText;
}
public struct SpriteLocalization
{
    public string CNName;
    public string ENName;
}

public struct AudioLocalization
{
    public string CNPath;
    public string ENPath;
}