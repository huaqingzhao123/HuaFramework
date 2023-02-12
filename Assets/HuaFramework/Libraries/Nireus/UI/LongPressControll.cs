using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nireus
{
    class LongPressControll : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private bool invokeOnce = true;//是否只调用一次
        private bool pressHadInvoke = false;
        private bool releaseHadInvoke = false;
        public float interval = 0.5f;//超过这个时间则定为长按
        private bool isPointDown = false;
        private float recordTime;

        public UnityEvent onDown = new UnityEvent();//按下回调
        public UnityEvent onLongPress = new UnityEvent();//长按住的时候调用
        public UnityEvent onRelease = new UnityEvent();//松开的时候调用


        void Awake()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            GetComponent<Graphic>().raycastTarget = true;
        }
        public void SetLongEventTime(float float_event_time)
        {
            if (float_event_time > 0)
            {
                interval = float_event_time;
            }

        }
        void Update()
        {
            if (invokeOnce && pressHadInvoke)
            {
                return;
            }
            if (isPointDown)
            {
                if ((Time.time - recordTime) > interval)
                {
                    onLongPress.Invoke();
                    pressHadInvoke = true;
                    releaseHadInvoke = false;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            releaseHadInvoke = false;
            isPointDown = true;
            recordTime = Time.time;
            if (onDown != null)
            {
                onDown.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointDown = false;
            pressHadInvoke = false;
            if (!releaseHadInvoke)
            {
                onRelease.Invoke();
                releaseHadInvoke = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointDown = false;
            pressHadInvoke = false;
            if (!releaseHadInvoke)
            {
                onRelease.Invoke();
                releaseHadInvoke = true;
            }
        }

    }
}
