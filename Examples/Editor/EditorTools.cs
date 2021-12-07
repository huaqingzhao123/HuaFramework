using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
using HuaFramework.Managers;
using HuaFramework.ResourcesManager;
using System.Linq;

namespace HuaFramework.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public class EditorTools
    {

        [MenuItem("HuaFramework/Tools/功能测试 %l",false,0)]
        private static void Test()
        {
            //Debug.LogError(MathfUtil.GetRandomElement(1, 2, 3));
            //Debug.LogError(MathfUtil.GetRandomElement("d", "a", "y"));
            //Debug.LogError(MathfUtil.GetRandomElement(1.1f, 2.0f, 3.0f));
        }
        [MenuItem("HuaFramework/Tools/1.生成Unitypackage名字",false,1)]
        private static void GeneratePackageName()
        {
            Debug.Log(ExportPackageUtil.GetPackageName());
        }

        [MenuItem("HuaFramework/Tools/2.Copy文本到剪贴板", false, 2)]
        private static void CopyPackageName()
        {
            CommonUtil.CopyText("复制到剪贴板");
        }

        [MenuItem("HuaFramework/Tools/3.复制文件名到剪贴版", false, 3)]
        private static void CutFileNameToCutBoardFunc()
        {
            CommonUtil.CopyText("要复制的文本");
        }

        [MenuItem("HuaFramework/Tools/5.打开data目录", false, 5)]
        private static void OpenDirectory()
        {
            CommonUtil.OpenSpecificDirectory(Application.dataPath);
        }
        [MenuItem("HuaFramework/Tools/4.自动导出Package", false, 4)]
        private static void MenuClicked()
        {
            var assetsPath = "Assets/HuaFramework";
            var fileName = Application.dataPath + "/" + ExportPackageUtil.GetPackageName() + ".unitypackage";
            EditorUtil.ExportPackage(assetsPath, fileName);
        }
        [MenuItem("HuaFramework/Tools/6.Menuitem复用", false, 6)]
        private static void MenuitemReuse()
        {
            EditorUtil.ExcuteMenuItem("HuaFramework/Tools/4.自动导出Package");
            EditorUtil.ExcuteMenuItem("HuaFramework/Tools/5.打开data目录");
        }
        [MenuItem("HuaFramework/Tools/7.自定义快捷键 %e", false, 7)]
        private static void QuicExcuteMenuitem()
        {
            Debug.Log("%e是指定快捷键为Ctrl+E");
            EditorUtil.ExcuteMenuItem("HuaFramework/Tools/6.Menuitem复用");
        }
        [MenuItem("HuaFramework/Tools/概率函数",false,8)]
        private static void JudgeProbability()
        {
            Debug.Log(MathfUtil.JudgePercent(50));
        }

        [MenuItem("HuaFramework/Tools/分辨率检测",false, 9)]

        private static void AsecptCheck()
        {
            Debug.Log(ResolutionCheck.ScreenIsAsecpt(4.0f / 3) ? "是4:3分辨率" : "不是4:3分辨率");
            Debug.Log(ResolutionCheck.ScreenIsAsecpt(16.0f / 9) ? "是16:9分辨率" : "不是16:9分辨率");
            Debug.Log(ResolutionCheck.ScreenIsAsecpt(3.0f / 2) ? "是32分辨率" : "不是3:2分辨率");
        }

        [MenuItem("HuaFramework/Tools/打包AssetBundle", false, 10)]

        private static void PackAssetBundle()
        {
            var path = HotUpdateManager.Instance.HotUpdateConfig.LocalAssetBundleFolder;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            UnityEditor.BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

            //生成版本信息文件
            var resVersionPath = HotUpdateManager.Instance.HotUpdateConfig.LocalAssetBundleFolder + HotUpdateManager.ResVersionName;
            var allAssetBundles = AssetDatabase.GetAllAssetBundleNames().ToList();
            var resVersion = new ResVersion() { Version = 5,AllAssetBundles= allAssetBundles};
            var resVersionJson = JsonUtility.ToJson(resVersion,true);
            File.WriteAllText(resVersionPath,resVersionJson);
            AssetDatabase.Refresh();
        }
    }
}

