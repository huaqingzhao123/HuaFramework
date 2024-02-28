using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nireus
{
    public class EventBehaviour : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
    {
        public delegate void PointerEventCallback(PointerEventData pe_data);

        public enum EventType
        {
            BEGIN_DRAG,
            END_DRAG,
            DRAG,
            POINTER_CLICK,
            POINTER_DOWN,
            POINTER_EXIT,
            POINTER_UP
        };

        private PointerEventCallback _begin_drag_handler;
        private PointerEventCallback _end_drag_handler;
        private PointerEventCallback _drag_handler;
        private PointerEventCallback _pointer_click_handler;
        private PointerEventCallback _pointer_down_handler;
        private PointerEventCallback _pointer_exit_handler;
        private PointerEventCallback _pointer_up_handler;

        public void setEventHandler(EventType event_type, PointerEventCallback callback)
        {
            switch (event_type)
            {
                case EventType.BEGIN_DRAG: _begin_drag_handler = callback; break;
                case EventType.END_DRAG: _end_drag_handler = callback; break;
                case EventType.DRAG: _drag_handler = callback; break;
                case EventType.POINTER_CLICK: _pointer_click_handler = callback; break;
                case EventType.POINTER_DOWN: _pointer_down_handler = callback; break;
                case EventType.POINTER_UP: _pointer_up_handler = callback; break;
                case EventType.POINTER_EXIT: _pointer_exit_handler = callback; break;
            }
        }

        public virtual void OnBeginDrag(PointerEventData pe_data)
        {
            if (_begin_drag_handler != null) _begin_drag_handler(pe_data);
        }

        public virtual void OnEndDrag(PointerEventData pe_data)
        {
            if (_end_drag_handler != null) _end_drag_handler(pe_data);
        }

        public virtual void OnDrag(PointerEventData pe_data)
        {
            if (_drag_handler != null) _drag_handler(pe_data);
        }

        public virtual void OnPointerClick(PointerEventData pe_data)
        {
            if (_pointer_click_handler != null) _pointer_click_handler(pe_data);
        }

        public virtual void OnPointerDown(PointerEventData pe_data)
        {
            if (_pointer_down_handler != null) _pointer_down_handler(pe_data);
        }
        public virtual void OnPointerExit(PointerEventData pe_data)
        {
            if (_pointer_exit_handler != null) _pointer_exit_handler(pe_data);
        }

        public virtual void OnPointerUp(PointerEventData pe_data)
        {
            if (_pointer_up_handler != null) _pointer_up_handler(pe_data);
        }
    }
}
