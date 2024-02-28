using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nireus
{
    class EventTriggerListener : EventTrigger
    {
        public Nireus.Delegate.VoidDelegate onBeginDrag;
        public Nireus.Delegate.VoidDelegate onDrag;
        public Nireus.Delegate.VoidDelegate onEndDrag;
        public Nireus.Delegate.VoidDelegate onPointerClick;
        public Nireus.Delegate.VoidDelegate onPointerDown;
        public Nireus.Delegate.VoidDelegate onPointerEnter;
        public Nireus.Delegate.VoidDelegate onPointerExit;
        public Nireus.Delegate.VoidDelegate onPointerUp;
        static public EventTriggerListener Get(GameObject go)
        {
            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener == null) listener = go.AddComponent<EventTriggerListener>();
            return listener;
        }


        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) onBeginDrag(gameObject);
        }
        public override void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null) onDrag(gameObject);
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) onEndDrag(gameObject);
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (onPointerClick != null) onPointerClick(gameObject);
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onPointerDown != null) onPointerDown(gameObject);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onPointerEnter != null) onPointerEnter(gameObject);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onPointerExit != null) onPointerExit(gameObject);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onPointerUp != null) onPointerUp(gameObject);
        }
    }
}
