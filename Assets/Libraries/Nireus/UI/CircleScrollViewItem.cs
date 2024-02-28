using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening.Core;

namespace Nireus
{
    [ExecuteInEditMode]
    public class CircleScrollViewItem : MonoBehaviour, IPointerClickHandler
    {
        [HideInInspector] public CircleScrollView view;

        [HideInInspector] public int idx;

        [HideInInspector] public RectTransform rect_transform;

        private DrivenRectTransformTracker _tracker = new DrivenRectTransformTracker();

        private void Reset()
        {
            rect_transform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            _tracker.Clear();
            _tracker.Add(gameObject, rect_transform,
                DrivenTransformProperties.AnchorMax |
                DrivenTransformProperties.AnchorMin |
                DrivenTransformProperties.Pivot |
                DrivenTransformProperties.Scale |
                DrivenTransformProperties.Rotation |
                DrivenTransformProperties.AnchoredPosition3D
            );
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            //if (view)
            //{
            //    view.scrollTo(this.gameObject);
            //}
        }
    }
}
