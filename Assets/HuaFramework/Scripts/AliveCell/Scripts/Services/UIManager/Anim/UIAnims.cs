/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/17 11:59:34
 */

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.Extensions;
using Ease = DG.Tweening.Ease;

namespace AliveCell.UIAnims
{
    public class UIMoveFrom : UIAnimBase
    {
        public Vector3 startOffsetValue;

        private RectTransform rectTransform;
        private Vector3 endValue;

        public override void OnInitialize(GameObject target)
        {
            base.OnInitialize(target);
            rectTransform = (RectTransform)target.transform;
            endValue = rectTransform.anchoredPosition;
        }

        protected override Tween CreateTween()
        {
            var tween = rectTransform.DOAnchorPos(endValue, duration).ChangeStartValue(endValue + startOffsetValue);
            return tween;
        }
    };

    public class UIRotateFrom : UIAnimBase
    {
        public Vector3 startOffsetValue;
        private RectTransform rectTransform;
        private Vector3 endValue;

        public override void OnInitialize(GameObject target)
        {
            base.OnInitialize(target);
            rectTransform = (RectTransform)target.transform;
            endValue = rectTransform.localRotation.eulerAngles;
        }

        protected override Tween CreateTween()
        {
            var tween = rectTransform.DOLocalRotate(endValue, duration).ChangeStartValue(endValue + startOffsetValue);
            return tween;
        }
    };

    public class UIScaleFrom : UIAnimBase
    {
        public Vector3 endValue;
        public bool changeStartValue;
        public Vector3 startValue;
        private RectTransform rectTransform;

        public override void OnInitialize(GameObject target)
        {
            base.OnInitialize(target);
            rectTransform = (RectTransform)target.transform;
        }

        protected override Tween CreateTween()
        {
            var tween = rectTransform.DOScale(endValue, duration);
            if (changeStartValue)
            {
                tween.ChangeStartValue(startValue);
            }
            return tween;
        }
    };

    public class UIFade : UIAnimBase
    {
        public float endValue;
        public bool changeStartValue;
        public float startValue;

        private CanvasGroup canvasGroup;

        public override void OnInitialize(GameObject target)
        {
            base.OnInitialize(target);
            canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }
        }

        protected override Tween CreateTween()
        {
            var tween = canvasGroup.DOFade(endValue, duration);
            if (changeStartValue)
            {
                tween.ChangeStartValue(startValue);
            }

            return tween;
        }
    };
}