using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening.Core;

namespace Nireus
{
    [ExecuteInEditMode]
    public class ExternLoopToggleGroupItem : MonoBehaviour, IPointerClickHandler
    {
        [HideInInspector] public ExternLoopToggleGroup group;

        [HideInInspector] public RectTransform rect_transform;

        public Toggle toggle;

        // the locked item cannot be switched to.
        public bool locked;

        [HideInInspector]
        public int idx;

        private DrivenRectTransformTracker _tracker = new DrivenRectTransformTracker();

        private void Reset()
        {
            rect_transform = GetComponent<RectTransform>();
        }

        public void Start()
        {
            if(toggle == null)
            {
                return;
            }
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(_onToggleValueChanged);
        }

        private void _onToggleValueChanged(bool v)
        {
            if(group == null)
            {
                return;
            }
            if(group.moving)
            {
                return;
            }
            if(v)
            {
                group.scrollTo(gameObject);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (group == null)
            {
                return;
            }
            if (group.moving)
            {
                return;
            }
            group.scrollTo(gameObject);
        }

        private static string ON_SELECT = "OnSelect"; //ISelectHandler
        private static string ON_DESELECT = "OnDeselect"; //IDeselectHandler
        private static BaseEventData event_data = new BaseEventData(null);
        public void onSelect()
        {
            this.gameObject.SendMessage(ON_SELECT, event_data, SendMessageOptions.DontRequireReceiver);
        }

        public void onDeselect()
        {
            this.gameObject.SendMessage(ON_DESELECT, event_data, SendMessageOptions.DontRequireReceiver);
        }
    }
}
