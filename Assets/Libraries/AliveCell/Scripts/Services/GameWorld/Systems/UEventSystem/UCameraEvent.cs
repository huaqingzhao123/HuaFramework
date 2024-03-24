/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/24 16:55:28
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// UCameraEvent
    /// </summary>
    public class UCameraEvent : UEvent
    {
        public string shakeName;

        public override void Execute()
        {
            if (!string.IsNullOrEmpty(shakeName))
            {
                App.game.ucamera.Shake(shakeName);
            }
        }

        public override void Reset()
        {
            base.Reset();
            shakeName = string.Empty;
        }
    }
}