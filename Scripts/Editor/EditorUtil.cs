using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace HuaFramework.Utility
{
    /// <summary>
    /// 框架Editor工具类
    /// </summary>
    public partial class EditorUtil
    {

        /// <summary>
        /// 导出package包
        /// </summary>
        public static void ExportPackage(string assetsPath, string fileName)
        {
            AssetDatabase.ExportPackage(assetsPath, fileName, ExportPackageOptions.Recurse);
        }
        /// <summary>
        /// 导出package包
        /// </summary>
        public static void ExportPackage( string fileName, params string[] assetsPaths)
        {
            AssetDatabase.ExportPackage(assetsPaths, fileName, ExportPackageOptions.Recurse);
        }
        /// <summary>
        /// 执行指定的Menuitem
        /// </summary>
        public static void ExcuteMenuItem(string menuitemName)
        {
            UnityEditor.EditorApplication.ExecuteMenuItem(menuitemName);
        }

        /// <summary>
        /// 打开指定文件并返回指定扩展名的文件路径
        /// </summary>
        /// <param name="defaultPath"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string SelectFilePath(string defaultPath = "", string extension = "")
        {
            if (string.IsNullOrEmpty(defaultPath))
                return EditorUtility.OpenFilePanel("选择配置文件路径", Application.dataPath, extension);
            else
                return EditorUtility.OpenFilePanel("选择配置文件路径", defaultPath, extension);
        }

        /// <summary>
        /// 弹出选择对话框
        /// </summary>
        /// <returns></returns>
        public static bool ShowDialog(string title, string message, string ok, string cancel = "")
        {
            if (string.IsNullOrEmpty(cancel))
                return EditorUtility.DisplayDialog(title, message, ok);
            else
                return EditorUtility.DisplayDialog(title, message, ok, cancel);
        }
    }
}
