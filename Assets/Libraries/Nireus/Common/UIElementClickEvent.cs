using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Nireus
{
    public class UIElementClickEvent : MonoBehaviour, IPointerClickHandler
    {
        public string soundFileName = "default_click.mp3";
        System.Action<GameObject> onClickHandle;
        void Awake()
        {
            if (GetComponent<CanvasGroup>() != null)
                GetComponent<CanvasGroup>().blocksRaycasts = true;
            GetComponent<Graphic>().raycastTarget = true;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(soundFileName) == false)
                if (onClickHandle != null)
                {
                    onClickHandle(this.gameObject);
                }
        }

        public void SetClickCallback(System.Action<GameObject> callback)
        {
            onClickHandle = callback;
        }


    }

}