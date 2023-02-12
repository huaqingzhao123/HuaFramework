using Nireus;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

namespace Nireus
{
    public class DynamicScrollViewItem : MonoBehaviour
    {
        public int _Index;
        public DynamicScrollView dynamic_scroll_view;
        public RectTransform rectTransform;

        public Image imgSelectTag;

        RectTransform m_ItemRoot;



        void Awake()
        {
            m_ItemRoot = transform.parent as RectTransform;
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        void Update()
        {
            if (dynamic_scroll_view == null)
            {
                dynamic_scroll_view = transform.parent.GetComponentInChildren<DynamicScrollView>();
            }
            var abs_x = Mathf.Abs(rectTransform.anchoredPosition.x);
            var rate_scale = Mathf.Clamp01(abs_x / dynamic_scroll_view.scale_rate);
            var rate = Mathf.Clamp01(abs_x / dynamic_scroll_view.space);
            rectTransform.localScale = Vector3.one * (1 - 0.2f * rate_scale);
            if (imgSelectTag != null) imgSelectTag.color = new Color(imgSelectTag.color.r, imgSelectTag.color.g, imgSelectTag.color.b, rate_scale);
            var spine = GetComponentInChildren<SkeletonGraphic>();
            float alpha = (1 - rate);
            if (spine != null)
            {
                spine.raycastTarget = false;
                spine.color = new Color(1, 1, 1, alpha);
            }
            if (rate < 0.5f)
            {
                if (dynamic_scroll_view.CurIndex != _Index)
                {
                    dynamic_scroll_view.CurIndex = _Index;
                    OnFocus();
                    dynamic_scroll_view.SortIndex();
                }
            }
        }

        void OnClick()
        {
            if (dynamic_scroll_view.is_draging == false && dynamic_scroll_view.CurIndex == _Index)
            {
                dynamic_scroll_view.ClickFocus(_Index,true);
            }
            else
            {
                dynamic_scroll_view.ClickFocus(_Index, false);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            name = "item-" + _Index;
        }

        /// <summary>
        /// 获得焦点时触发
        /// </summary>
        public virtual void OnFocus()
        {
            GameDebug.Log("On Focus item " + _Index);
            dynamic_scroll_view.FocusIndex(_Index);
        }
    }
}