using System;
using System.Collections;
using System.Collections.Generic;
using Nireus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 兼容刘海屏
/// </summary>
/// 
namespace Nireus
{
    public class NotchAdapter : UIBehaviour
    {
        public bool useCustomNotchHeight = false;
        public float customNotchHeight = 40f;

        private string SchedulerKeyPrefix = "NotchAdapter_AutoAdpat";
        private string schedulerKey;

        public const float NOTCH_HEIGHT = 40f;
        public const float NOTCH_HW = 2.001f;

        private RectTransform rectTransform;

        protected override void Awake()
        {
            base.Awake();
            schedulerKey = $"{SchedulerKeyPrefix}_{Guid.NewGuid():N}";
            rectTransform = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            base.Start();
            Adapt();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Scheduler.Instance?.RegisterSecond(schedulerKey, Adapt);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Scheduler.Instance?.Unregister(schedulerKey);
        }

        public void Adapt()
        {
            if (!rectTransform) return;

            if (HasNotch())
            {
                var notchHeight = NOTCH_HEIGHT;
                if (useCustomNotchHeight)
                {
                    notchHeight = customNotchHeight;
                }

                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -notchHeight);
            }
            else
            {
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
            }
        }

        public static bool HasNotch()
        {
            return Screen.height / (float)Screen.width > NOTCH_HW;
        }

    }
}