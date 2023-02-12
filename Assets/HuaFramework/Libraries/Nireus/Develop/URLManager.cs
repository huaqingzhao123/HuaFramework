using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class URLManager 
{
    const string s_configName = "URLConfig";

    static Dictionary<string,string> s_URLTable;

    public static string GetURL(string urlKey)
    {
        Init();

        if (s_URLTable != null)
        {
            if (s_URLTable.ContainsKey(urlKey))
            {
                return s_URLTable[urlKey];
            }
            else
            {
                return null;
            }
        }

        return null;
    }

    public static void Init()
    {
        if (s_URLTable == null)
        {
            s_URLTable = new Dictionary<string, string>();
            s_URLTable.Add("ReplayFileUpLoadURL","http://localhost/wwb/upload.php");
            // if(ConfigManager.GetIsExistConfig(s_configName))
            // {
            //     s_URLTable = ConfigManager.GetData(s_configName);
            // }
        }
    }
}
