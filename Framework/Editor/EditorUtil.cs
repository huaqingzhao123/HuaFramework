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
        /// 执行指定的Menuitem
        /// </summary>
        public static void ExcuteMenuItem(string menuitemName)
        {
            UnityEditor.EditorApplication.ExecuteMenuItem(menuitemName);
        }
    }
}