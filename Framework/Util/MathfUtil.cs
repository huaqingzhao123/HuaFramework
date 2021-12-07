using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HuaFramework.Utility
{
    /// <summary>
    /// 框架数学工具类
    /// </summary>
    public partial class MathfUtil
    {
        /// <summary>
        /// 判断百分之多少的概率是否满足
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static bool JudgePercent(int percent)
        {
            return Random.Range(0, 100) < percent;
        }
        /// <summary>
        /// 返回给定数组中的一个随机元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static T GetRandomElement<T>(params T[] objects)
        {
            return objects[Random.Range(0, objects.Length)];
        }
    }


}