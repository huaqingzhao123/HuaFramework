/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/17 11:55:47
 */

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

using XMLib.Extensions;
using Ease = DG.Tweening.Ease;

namespace AliveCell
{
    /// <summary>
    /// UIAnimBase
    /// </summary>
    [System.Serializable]
    public abstract class UIAnimBase
    {
        public UIPanelStatus status;
        public bool enterOrExit;
        public bool joinOrAppend;

        public float duration;
        public float delay;
        public Ease ease;

        protected GameObject target;

        public Tween GetTween()
        {
            Tween tween = CreateTween();

            if (delay.NotEqualToZero())
            {
                tween.SetDelay(delay);
            }

            if (ease != Ease.Unset)
            {
                tween.SetEase(ease);
            }

            return tween;
        }

        protected abstract Tween CreateTween();

        public virtual void OnInitialize(GameObject target)
        {
            this.target = target;
        }
    }
}