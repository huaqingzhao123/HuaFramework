using System;
using UnityEngine;

namespace Nireus
{
    public class UICamera : MonoBehaviour
    {
        private Camera uiCamera;
        public static Camera MainUICamera { get; private set; } = null;

        private static RectTransform rectTransformParent = null;
        public static int ScreenWidth { get; private set; } = 720;
        public static int ScreenHeight { get; private set; } = 1280;

        public static Action OnScreenSizeChange { get; set; }

        private string schedulerKey = "";

        void Awake()
        {
            uiCamera = gameObject.GetComponent<Camera>();
            if (uiCamera.tag  == "UICamera" && MainUICamera == null)
            {
                MainUICamera = uiCamera;
            }

            if (rectTransformParent == null)
            {
                rectTransformParent = (RectTransform)transform.parent;
            }
        }

        void OnEnable()
        {
            //if (rectTransformParent != null)
            //{
            //    schedulerKey = "Nireus.UICamera-" + Guid.NewGuid().ToString("N");
            //    Scheduler.Instance.RegisterSecond(schedulerKey, _checkScreenSize);
            //}
        }

        void OnDisable()
        {
            //Scheduler.Instance?.Unregister(schedulerKey);
        }

        private static void _checkScreenSize()
        {
            var screen_size = rectTransformParent.sizeDelta;

            var screen_width_now = (int)screen_size.x;
            var screen_height_now = (int)screen_size.y;

            if (screen_width_now != ScreenWidth || screen_height_now != ScreenHeight)
            {
                ScreenWidth = screen_width_now;
                ScreenHeight = screen_height_now;

                OnScreenSizeChange?.Invoke();
            }
        }
    }
}