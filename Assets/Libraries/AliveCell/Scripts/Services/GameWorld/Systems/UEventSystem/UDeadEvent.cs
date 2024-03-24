/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/21 15:15:21
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// UDeadEvent
    /// </summary>
    public class UDeadEvent : UEvent
    {
        public override void Execute()
        {
            App.Trigger(EventTypes.Game_Dead, ID);
        }
    }
}