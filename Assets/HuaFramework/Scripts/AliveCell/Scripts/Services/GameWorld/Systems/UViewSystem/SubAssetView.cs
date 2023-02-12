/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/28 10:46:19
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// SubAssetView
    /// </summary>
    public abstract class SubAssetView : MonoBehaviour
    {
        public AssetView parentView { get; set; }

        public virtual void OnViewBind()
        {
        }

        public virtual void OnViewUnbind()
        {
        }

        public virtual void LogicUpdateView(Single deltaTime)
        {
        }

        public virtual void UpdateView(Single deltaTime)
        {
        }

        public virtual void SyncLogicUpdate(Single deltaTime)
        {
        }

        public virtual void OnPopPool()
        {
        }

        public virtual void OnPushPool()
        {
        }
    }
}