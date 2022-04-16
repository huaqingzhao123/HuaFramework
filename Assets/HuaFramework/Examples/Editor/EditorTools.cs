using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
using HuaFramework.Managers;
using HuaFramework.ResourcesRef;
using System.Linq;
using HuaFramework.Unity;

namespace HuaFramework.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public class EditorTools
    {

        [MenuItem("HuaFramework/Tools/功能测试",false,0)]
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
        private static void ExportPackageAll()
        {
            var assetsExamplePath = "Assets/HuaFramework/Examples";
            var assetsPluginsPath= "Assets/HuaFramework/Plugins";
            var dllRootPath = UnityTools.Library + "/ScriptAssemblies/";
            var dllPath = dllRootPath + "HuaFramework.dll";
            var pdbPath = dllRootPath + "HuaFramework.pdb";
            var editorDllPath = dllRootPath + "HuaFramework.dll";
            var editorPdbPath = dllRootPath + "HuaFramework.pdb";
            var newDllPath = assetsPluginsPath + "/HuaFramework.dll";
            var newPdbPath = assetsPluginsPath + "/HuaFramework.pdb";
            var newEditorDllPath = assetsPluginsPath + "/Editor/HuaFrameworkEditor.dll";
            var newEditorPdbPath = assetsPluginsPath + "/Editor//HuaFrameworkEditor.pdb";
            File.Copy(dllPath, newDllPath);
            File.Copy(pdbPath,newPdbPath);
            File.Copy(editorDllPath, newEditorDllPath);
            File.Copy(editorPdbPath, newEditorPdbPath);
            AssetDatabase.Refresh();
            var fileName = Application.dataPath + "/" + ExportPackageUtil.GetPackageName() + ".unitypackage";
            EditorUtil.ExportPackage(fileName,assetsExamplePath, assetsPluginsPath);
            File.Delete(newDllPath);
            File.Delete(newPdbPath);
            File.Delete(newEditorDllPath);
            File.Delete(newEditorPdbPath);
            AssetDatabase.Refresh();
            AssetDatabase.WriteImportSettingsIfDirty(assetsPluginsPath);
        }
        [MenuItem("HuaFramework/Tools/6.Menuitem复用", false, 6)]
        private static void MenuitemReuse()
        {
            EditorUtil.ExcuteMenuItem("HuaFramework/Tools/4.自动导出Package");
            EditorUtil.ExcuteMenuItem("HuaFramework/Tools/5.打开data目录");
        }
        [MenuItem("HuaFramework/Tools/7.自定义快捷键导完整包 %e", false, 7)]
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
            var path = HotResUtil.LocalAssetBundleFolder;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            UnityEditor.BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

            //生成版本信息文件
            var resVersionPath = HotResUtil.LocalResversionFilePath;
            var allAssetBundles = AssetDatabase.GetAllAssetBundleNames().ToList();
            var resVersion = new ResVersion() { Version = 5,AllAssetBundles= allAssetBundles};
            var resVersionJson = JsonUtility.ToJson(resVersion,true);
            File.WriteAllText(resVersionPath,resVersionJson);
            AssetDatabase.Refresh();
        }

        [Obsolete]
        [MenuItem("HuaFramework/Tools/11.自动导出不含odin", false, 11)]
        private static void ExportPackageCommon()
        {
            var assetsPluginsPath = "Assets/HuaFramework/Framework/Plugins";
            var odinConfigPath= "Assets/HuaFramework/Framework/OdinConfigs";
            File.Move(odinConfigPath, odinConfigPath + "~");
            AssetDatabase.Refresh();
            var dllRootPath = UnityTools.Library + "/ScriptAssemblies/";
            var dllPath = dllRootPath + "HuaFramework.dll";
            var pdbPath = dllRootPath + "HuaFramework.pdb";
            var editorDllPath = dllRootPath + "HuaFramework.dll";
            var editorPdbPath = dllRootPath + "HuaFramework.pdb";
            var newDllPath = assetsPluginsPath + "/HuaFramework.dll";
            var newPdbPath = assetsPluginsPath + "/HuaFramework.pdb";
            var newEditorDllPath = assetsPluginsPath + "/Editor/HuaFrameworkEditor.dll";
            var newEditorPdbPath = assetsPluginsPath + "/Editor//HuaFrameworkEditor.pdb";
            File.Copy(dllPath, newDllPath);
            File.Copy(pdbPath, newPdbPath);
            File.Copy(editorDllPath, newEditorDllPath);
            File.Copy(editorPdbPath, newEditorPdbPath);
            AssetDatabase.Refresh();
            var fileName = Application.dataPath + "/" + ExportPackageUtil.GetPackageName() + ".unitypackage";
            EditorUtil.ExportPackage(assetsPluginsPath, fileName);
            File.Delete(newDllPath);
            File.Delete(newPdbPath);
            File.Delete(newEditorDllPath);
            File.Delete(newEditorPdbPath);
            File.Move(odinConfigPath + "~",odinConfigPath);
            AssetDatabase.Refresh();
            AssetDatabase.WriteImportSettingsIfDirty(assetsPluginsPath);
        }
        [MenuItem("HuaFramework/Tools/12.自动导出框架源码工程 %l", false, 11)]
        private static void ExportFrameworkPackage()
        {
            var frameworkPath = "Assets/HuaFramework";
            var fileName = Application.dataPath + "/" + ExportPackageUtil.GetSourcePackageName() + ".unitypackage";
            EditorUtil.ExportPackage(frameworkPath, fileName);
            AssetDatabase.Refresh();
            EditorUtil.ExcuteMenuItem("HuaFramework/Tools/5.打开data目录");
        }
        //[MenuItem("HuaFramework/Tools/13.导出框架源码工程 ", false, 12)]
        //private static void QuicExcuteMenuitemCommon()
        //{
        //    EditorUtil.ExcuteMenuItem("HuaFramework/Tools/12.自动导出框架源码工程");
        //    EditorUtil.ExcuteMenuItem("HuaFramework/Tools/5.打开data目录");

        //}
    }
}

