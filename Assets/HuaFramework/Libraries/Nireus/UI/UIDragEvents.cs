using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Nireus
{
    public class UIDragEvents : MonoBehaviour, IBeginDragHandler, IDragHandler,IEndDragHandler,IPointerEnterHandler,ICancelHandler,IPointerClickHandler
    {
        
        public Action<PointerEventData> OnBeginDragAction;
        public Action<PointerEventData> OnDragAction;
        public Action<PointerEventData> OnEndDragAction;
        public Action<PointerEventData> OnClickAction;
        public Action<PointerEventData> OnPointerEnterAcion;
        public Action OnCancelAction;
        private bool is_drag;
        public void OnBeginDrag(PointerEventData eventData)
        {
            is_drag = true;
            OnBeginDragAction?.Invoke(eventData);
        }


        public void OnDrag(PointerEventData eventData)
        {
            OnDragAction?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            is_drag = false;
            OnEndDragAction?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           OnPointerEnterAcion?.Invoke(eventData);
        }
        
        public void OnCancel(BaseEventData eventData)
        {
            is_drag = true;
            OnCancelAction?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (is_drag == false)
            {
                OnClickAction?.Invoke(eventData);
            }
        }
    }

}