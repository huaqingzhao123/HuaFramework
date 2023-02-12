/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/28 11:02:29
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// TrialSubView
    /// </summary>
    [RequireComponent(typeof(TrailRenderer))]
    public class TrailSubView : SubAssetView
    {
        public TrailRenderer trail;

        public override void OnPushPool()
        {
            base.OnPushPool();

            if(trail != null)
            {
                trail.Clear();
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (trail == null)
            {
                trail = GetComponent<TrailRenderer>();
            }
        }
#endif
    }
}