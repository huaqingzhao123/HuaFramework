/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/26 23:52:00
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// Direction
    /// </summary>
    public enum Direction
    {
        None = 0b0000_0000,
        Up = 0b0000_0001,
        Down = 0b0000_0010,
        Left = 0b0000_0100,
        Right = 0b0000_1000,
        Forward = 0b0001_0000,
        Back = 0b0010_0000
    }
}