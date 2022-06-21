using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HuaFramework.Utility
{
    public partial class CommonUtil
    {
        /// <summary>
        /// 复制文本到剪贴板
        /// </summary>
        /// <param name="text"></param>
        public static void CopyText(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        /// <summary>
        /// 打开指定文件夹
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void OpenSpecificDirectory(string directoryPath)
        {
            Application.OpenURL("file://" + directoryPath);
        }
        /// <summary>
        /// 打开指定网址
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void OpenSpecificUrl(string url)
        {
            Application.OpenURL(url);
        }
    }
}
