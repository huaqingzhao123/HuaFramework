/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/11/26 14:58:57
 */

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public struct InputData
    {
        public ActionKeyCode keyCode;
        public byte axisValue;

        public static readonly InputData none = new InputData() { keyCode = 0, axisValue = byte.MaxValue };

        public static bool operator ==(InputData a, InputData b)
        {
            return a.keyCode == b.keyCode && a.axisValue == b.axisValue;
        }

        public static bool operator !=(InputData a, InputData b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (int)keyCode & axisValue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is InputData data))
            {
                return false;
            }

            return this == data;
        }
    }
}