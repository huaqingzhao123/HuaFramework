using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HuaFramework.Utility
{
    public partial class ResolutionCheck : MonoBehaviour
    {
        /// <summary>
        /// 检测屏幕分辨率是否为指定分辨率
        /// </summary>
        public static bool ScreenIsAsecpt(float asecptCheck)
        {
            //得到屏幕分辨率
            var asecpt = GetScreenAsecpt();
            //浮点数有误差，加减0.05f
            //Debug.LogErrorFormat("输入的分辨率:{0},得到的屏幕分辨率:{1},是否匹配:{2},屏幕的宽:{3},屏幕的高:{4}",
            //    asecptCheck, asecpt, asecptCheck > (asecpt - 0.05f) && asecptCheck < (asecpt + 0.05f),Screen.width,Screen.height);
            return asecptCheck > (asecpt - 0.05f) && asecptCheck < (asecpt + 0.05f);

        }
        private static float GetScreenAsecpt()
        {
            var asecpt = Screen.width > Screen.height ? (float)Screen.width / Screen.height : (float)Screen.height / Screen.width;
            return asecpt;
        }

    }
}

