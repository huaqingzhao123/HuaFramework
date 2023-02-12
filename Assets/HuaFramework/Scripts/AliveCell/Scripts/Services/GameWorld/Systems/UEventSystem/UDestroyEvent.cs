/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/25 2:39:08
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// UDestroyEvent
    /// </summary>
    public class UDestroyEvent : UEvent
    {
        public override void Execute()
        {
            App.game.Destroy(ID);
        }
    }
}