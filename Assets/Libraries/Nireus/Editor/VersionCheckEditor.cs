using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class VersionCheckEditor : UnityEditor.Editor
{
    [MenuItem("Nireus/正式包打包前检查",false, 1101)]
    private static void ZhengShiBuildCheck()
    {
        //1.检查宏定义
        ScriptingDefineSymbolsCheck();
        //2.检查版本号是否一致
        ChechVersionCode();
        //3.检查分包是否未删除
        CheckFenBao();
        //4.提示检查分包版本号
        AlertFenBaoVersion();
    }

    public static void ScriptingDefineSymbolsCheck()
    {
        string symbols = "";
#if UNITY_ANDROID
        symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        if (symbols != "ASSET_BUNDLE;U8SDK;ODIN_INSPECTOR;BEHAVIAC_RELEASE;#OUTER_TEST;UNITY_SWITCH;USE_SUB_PACKAGE")
        {
            if (EditorUtility.DisplayDialog("确认宏定义是否正确",
                "确认宏定义是否正确", "好的", "继续"))
            {
                Debug.Log("==================================" + "宏定义错误");

            }
            else
            {
                Debug.Log("==================================" + "宏定义错误");
            }
        }
#elif UNITY_IOS
        symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
        if (symbols != "ASSET_BUNDLE;ODIN_INSPECTOR;BEHAVIAC_RELEASE;#OUTER_TEST;USE_SUB_PACKAGE")
        {
            if (EditorUtility.DisplayDialog("确认宏定义是否正确",
                "确认宏定义是否正确", "好的", "继续"))
            {
                Debug.Log("==================================" + "宏定义错误");

            }
            else
            {
                Debug.Log("==================================" + "宏定义错误");
            }
        }
#endif
        if (symbols == "")
        {
            if (EditorUtility.DisplayDialog("确认宏定义是否正确",
                "确认宏定义是否正确", "好的", "继续"))
            {
                Debug.Log("==================================" + "宏定义错误");

            }
            else
            {
                Debug.Log("==================================" + "宏定义错误");
            }
        }
    }

    public static void ChechVersionCode()
    {
        string bundleVersion = PlayerSettings.bundleVersion.Replace(".", "");
        //long versionCode = VersionConfig.Instance.version;
        //string version_big = (versionCode / 1000000000).ToString();
        //if (bundleVersion != version_big)
        //{
        //    if (EditorUtility.DisplayDialog("确认版本号是否正确",
        //        "确认版本号是否正确", "好的", "继续"))
        //    {
        //        Debug.Log("==================================" + "版本号不正确");
        //    }
        //    else
        //    {
        //        Debug.Log("==================================" + "版本号不正确");
        //    }
        //}
    }

    public static void CheckFenBao()
    {
        List<string> dirs = new List<string>(Directory.GetDirectories(Application.streamingAssetsPath, "*", System.IO.SearchOption.TopDirectoryOnly));
        //有一个Audio文件夹
        if (dirs.Count > 1)
        {
            if (EditorUtility.DisplayDialog("确认分包文件夹状态",
                "确认分包文件夹是否删除", "好的", "继续"))
            {
                Debug.Log("==================================" + "分包未删除");

            }
            else
            {
                Debug.Log("==================================" + "分包未删除");
            }
        }
    }

    public static void AlertFenBaoVersion()
    {
        if (EditorUtility.DisplayDialog("确认分包版本号是否一致",
            "确认分包版本号是否一致", "好的", "继续"))
        {
            //Debug.Log("==================================" + "取消打包");

        }
        else
        {
            //Debug.Log("==================================" + "继续打包");
        }
    }
    
}
