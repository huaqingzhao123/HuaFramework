using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nireus
{
    public class UIEventTriggerListener : UnityEngine.EventSystems.EventTrigger
    {
        public static UIEventTriggerListener Get(GameObject go)
        {
            UIEventTriggerListener listener = go.GetComponent<UIEventTriggerListener>();
            if (listener == null) listener = go.AddComponent<UIEventTriggerListener>();
            return listener;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            GameDebug.Log("OnPointerEnter:" + eventData.pointerEnter + "=>" + gameObject.name);
        }
    }
}
