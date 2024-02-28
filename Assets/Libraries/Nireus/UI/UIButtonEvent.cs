using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nireus
{
    public enum EventPointerType
    {
        Click,
        Press,
        DubleClick
    }


    public class UIButtonEvent : Button, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        private float pressDurationTime = 1;

        private bool responseOnceByPress = false;

        private float doubleClickIntervalTime = 0.5f;

        private bool isDown = false;

        private bool isPress = false;

        private float downTime = 0;

        private float clickIntervalTime = 0;

        private int clickTimes = 0;

        private UnityEvent onPressEvent;

        private UnityEvent onClickEvent;

        private UnityEvent onDoubleClickEvent;


        private bool canDouble => onDoubleClickEvent == null ? false : true;
        private bool canPress => onPressEvent == null ? false : true;
        private bool canClick => onClickEvent == null ? false : true;

        public void Addlisener(UnityAction action, EventPointerType type = EventPointerType.Click)
        {
            switch (type)
            {
                case EventPointerType.Click:
                    onClickEvent = new UnityEvent();
                    onClickEvent.AddListener(action);
                    break;
                case EventPointerType.Press:
                    onPressEvent = new UnityEvent();
                    onPressEvent.AddListener(action);
                    break;
                case EventPointerType.DubleClick:
                    onDoubleClickEvent = new UnityEvent();
                    onDoubleClickEvent.AddListener(action);
                    break;
                default:
                    break;
            }
        }

        private void Update()
        {
            if (!canDouble && !canPress && !canClick)
                return;
            if (isDown && canPress)
            {
                if (responseOnceByPress && isPress)
                {
                    return;
                }
                downTime += Time.deltaTime;
                if (downTime > pressDurationTime)
                {
                    isPress = true;
                    onPressEvent?.Invoke();
                }
            }

            if (clickTimes >= 1)
            {
                if (canDouble)
                {
                    clickIntervalTime += Time.deltaTime;
                    if (clickIntervalTime >= doubleClickIntervalTime)
                    {
                        if (clickTimes >= 2)
                        {
                            onDoubleClickEvent?.Invoke();
                        }
                        else
                        {
                            onClickEvent?.Invoke();
                        }
                        clickTimes = 0;
                        clickIntervalTime = 0;
                    }
                }
                else
                {
                    onClickEvent?.Invoke();
                    clickTimes = 0;
                }

            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            downTime = 0;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            isDown = false;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            isDown = false;
            isPress = false;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!isPress)
            {
                clickTimes++;
            }
            else
            {
                isPress = false;
            }
        }
    }
}
