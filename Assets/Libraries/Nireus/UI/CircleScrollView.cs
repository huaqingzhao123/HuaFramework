using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening.Core;
using System;

namespace Nireus
{
    [ExecuteInEditMode]
    public class CircleScrollView : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private List<CircleScrollViewItem> list = new List<CircleScrollViewItem>();

        [HideInInspector]
        [SerializeField]
        private List<float> _base_angle_list = new List<float>();
        public DynamicScrollView dynamic_scroll_view;
        public float angle = 20;
        public float radius = 200;
        public float speed = 1.0f;
        public float clickTween= 0.5f;
        public float maxScale = 0.2f;//最大透视缩放
        public float maxOffset = 0;//最大位置上下偏移量
        public bool alwaysShow = false;
        public bool is_can_drag = true; 
        private float _limit(float angle)
        {
            //限制角度在0°- 360°之间
            if (angle < 0)
            {
                angle = 360 + (angle % 360);
            }
            else
            {
                angle = angle % 360;
            }
            return angle;
        }



#if UNITY_EDITOR
        private void Update()
        {
            //不知道为何按下apply后child的位置会变。。。只好这么办
            if (Application.isPlaying == false)
            {
                _render();
            }
        }
#endif

        private void _set(RectTransform target, float angle)
        {
            //GameDebug.Log("set " + angle);

            //angle = _limit(angle);


            bool visible = angle < 90 || angle > 270;

            //要注意避开tan90°
            if (visible || alwaysShow)//这里其实就是【-90°】到【+90°】
            {
                //if (alwaysShow && visible == false)
                //{
                //    angle = _limit(angle + 180);
                //}

                var radian = angle * Mathf.PI / 180;// 角度转化为弧度
                target.gameObject.SetActive(true);
                float x = radius * Mathf.Sin(radian);
                float y = maxOffset * Mathf.Abs(Mathf.Cos(radian)) - maxOffset/2;
                float scale = 1.0f - maxScale * Mathf.Abs(Mathf.Sin(radian));

                target.pivot = new Vector2(0.5f, 0.5f);
                target.anchorMin = new Vector2(0.5f, 0.5f);
                target.anchorMax = new Vector2(0.5f, 0.5f);

                target.localPosition = new Vector3(x, y, 0);
                target.localRotation = Quaternion.Euler(0, angle, 0);
                target.localScale = new Vector3(scale, scale, 1);
            }
            else
            {
                target.gameObject.SetActive(false);
            }
        }

        [HideInInspector]
        [SerializeField]
        private float _cur_angle = 0;

        private bool _is_dragging = false;

        private void _render()
        {
            for (var i = 0; i < list.Count; i++)
            {
                _set(list[i].rect_transform, _cur_angle + _base_angle_list[i]);
            }
        }

        public int GetMaxItemCount()
        {
            return list.Count;
        }


        private void Reset()
        {
            list.Clear();
            _base_angle_list.Clear();
            _is_scrolling = false;
            _is_dragging = false;

            var image = gameObject.AddComponentIfNeeded<Image>();    
            if(image)
            {
                image.color = new Color(0, 0, 0, 0);
            }
            bind();
        }

        private void Start()
        {
            //因为child在Start中执行了DrivenRectTransformTracker相关操作
            //我经过实验发现该操作必须写在Start函数里,且该操作会重置child的position
            //所以本脚本也必须在Start的时候调用一次render来重置position
            dynamic_scroll_view = this.transform.parent.parent.GetComponentInChildren<DynamicScrollView>();
            //dynamic_scrollview.OnIndexChange += OnIndexChange;
            _render();

            dynamic_scroll_view.OnDragCallback += OnDragCallback;
            dynamic_scroll_view.OnClickItemCallback += OnClickItemCallback;
            dynamic_scroll_view.OnEndDragCallback += OnEndDragCallback;
        }

        private void OnIndexChange(int arg1, int index)
        {
            scrollToIdx(index);
        }

        public void OnDragCallback(PointerEventData eventData)
        {
            _cur_angle = _cur_angle + speed * eventData.delta.x;
            //_cur_angle = _limit(_cur_angle);
            _render();
        }
        public void OnEndDragCallback(int obj)
        {
            scrollToIdx(obj);
        }

        public void OnClickItemCallback(int obj)
        {
            scrollToIdx(obj);
        }

        //每当child个数变化，或是调整了ScrollRect的宽度时都要按这个按钮
        [ContextMenu("绑定")]
        public void bind()
        {
            list.Clear();
            _base_angle_list.Clear();

            var child_count = transform.childCount;

            if(child_count == 0)
            {
                return;
            }
            bool even = (child_count & 1) == 0;
            int mid = child_count / 2;

            for (int i = 0; i < child_count; i++)
            {
                list.Add(transform.GetChild(i).gameObject.AddComponentIfNeeded<CircleScrollViewItem>());
                list[i].view = this;
                list[i].idx = i;

                //记录_base_angle是多少°时， _list[0]会显示在正中间
                float base_angle_when_mid = (mid - i) * angle;

                if (even)
                {
                    base_angle_when_mid -= (angle / 2);
                }
                _base_angle_list.Add(base_angle_when_mid);
            }
            _cur_angle = _limit(-_base_angle_list[mid]);
            _render();
        }

        public void scrollTo(GameObject obj)
        {
            if (_is_dragging == true)
            {
                return;
            }
            CircleScrollViewItem item = obj.GetComponent<CircleScrollViewItem>();

            if (item == null)
            {
                return;
            }
            if (item.view != this)
            {
                return;
            }
            var idx = list.IndexOf(item);

            if (idx == -1)
            {
                return;
            }
            scrollToIdx(idx);
        }

        private bool _is_scrolling = false;

        public void scrollToIdx(int idx)
        {
            if(_is_dragging || _is_scrolling)
            {
                return;
            }
            if (idx >= list.Count || idx < 0)
            {
                return;
            }
            var target_angle = (_base_angle_list[idx]);
            //_cur_angle = _limit(_cur_angle);
            //要选择合适的转动方向，根据点击的位置来判断
            //if (list[idx].rect_transform.localPosition.x > 0)
            //{
            //    //点击在右侧，要向右滑动，所以要减小_cur_angle
            //    if (_cur_angle < target_angle)
            //    {
            //        target_angle -= 360;
            //    }
            //    if (alwaysShow && Mathf.Abs(_cur_angle - target_angle) > 180)
            //    {
            //        target_angle += 180;
            //    }
            //}
            //else
            //{
            //    //点击在左侧，要向左滑动，所以要增大_cur_angle
            //    if (_cur_angle > target_angle)
            //    {
            //        target_angle += 360;
            //    }
            //    if (alwaysShow && Mathf.Abs(_cur_angle - target_angle) > 180)
            //    {
            //        target_angle -= 180;
            //    }
            //}

            if (target_angle == _cur_angle)
            {
                return;
            }

            DOTween.Kill(this.GetHashCode(),false);
            DOGetter<float> getter = () => { return _cur_angle; };
            DOSetter<float> setter = (v) => { _cur_angle = v;  _render(); };
            DOTween.To(getter, setter, target_angle, clickTween).OnComplete(() => 
            {
                _cur_angle = target_angle;
              //  _cur_angle = _limit(_cur_angle);
            }).SetId(this.GetHashCode());
        }
    }
}
