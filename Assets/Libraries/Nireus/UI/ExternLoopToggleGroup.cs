using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening.Core;

namespace Nireus
{
    [ExecuteInEditMode]
    public class ExternLoopToggleGroup : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [HideInInspector]
        [SerializeField]
        private List<ExternLoopToggleGroupItem> list = new List<ExternLoopToggleGroupItem>();


        public float tween = 1.0f;

        public uint spacing = 0;
        public uint extraSpacing = 0;
        public float offset = 0;

        public RectTransform mask;
        public int maskSblingIndex = 0;

        private RectTransform rect_transform = null;

        public delegate void OnItemSelectedDelegate(int index, ExternLoopToggleGroupItem selectedItem);
        public event OnItemSelectedDelegate onItemSelected;

        public delegate void OnLockedItemSelectingDelegate(int index, ExternLoopToggleGroupItem selectedItem);
        public event OnItemSelectedDelegate onLockedItemSelecting;

        private bool _isInitialItemSelectedEvent = true;

        public void Start()
        {
            rect_transform = GetComponent<RectTransform>();

            if (list.Count == 0)
            {
                return;
            }
            _render(_step);

            _showToggle(_step);

            scrollTo(list[0].gameObject);
        }

        [HideInInspector]
        [SerializeField]
        private float _range = 0;

        //每当child个数变化，或是调整了ScrollRect的宽度时都要按这个按钮
        [ContextMenu("绑定")]
        public void bind()
        {
            list.Clear();
           
            int child_count = transform.childCount;

            if(child_count == 0)
            {
                return;
            }

            _step = 0;
            
            for (int i = 0; i < child_count ; i++)
            {
                var game_object = transform.GetChild(i).gameObject;
                if(mask && Object.ReferenceEquals(game_object, mask.gameObject))
                {
                    continue;
                }
                var item = game_object.AddComponentIfNeeded<ExternLoopToggleGroupItem>();
                list.Add(item);
                item.group = this;
                item.idx = i;
            }

            if(list.Count == 0)
            {
                return;
            }

            _range = spacing * list.Count + extraSpacing;

            rect_transform.sizeDelta = new Vector2(_range, rect_transform.sizeDelta.y);




            _render(_step);
            _showToggle(_step);
        }



        private float _start_drag_pos = 0;
        private float _step = 0;

        public void OnDrag(PointerEventData eventData)
        {
            //不加IDragHandler接口的话OnBeginDrag、OnEndDrag不会触发
        }

        public void scrollTo(GameObject obj)
        {
            ExternLoopToggleGroupItem item = obj.GetComponent<ExternLoopToggleGroupItem>();

            if (item == null)
            {
                return;
            }
            if (item.group != this)
            {
                return;
            }
            
            var idx = list.IndexOf(item);

            if (idx == -1)
            {
                return;
            }

            if (item.locked)
            {
                onLockedItemSelecting?.Invoke(idx, item);
                return;
            }

            var cur_idx = _getMidIdx((int)_step);


            //因为是循环的，所以首节点跳转到末节点的方式是-1，而不是+(list.Count-1)
            var distance1 = ((idx - cur_idx) - list.Count) % list.Count;
            var distance2 = ((idx - cur_idx) + list.Count) % list.Count;

            var cur_item = list[cur_idx];
            bool left = (cur_item.rect_transform.anchoredPosition.x > item.rect_transform.anchoredPosition.x);



            int target_step = (int)(_step + (left ? distance1 : distance2));
            scrollToStep(target_step);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _start_drag_pos = eventData.position.x;
        }

        public bool enableMoveLeft = true;
        public bool enableMoveRight = true;

        public void OnEndDrag(PointerEventData eventData)
        {
            var delta = eventData.position.x - _start_drag_pos;

            if ( (enableMoveLeft && delta < 0) || (enableMoveRight && delta > 0))
            {
                var targetStep = _step - Mathf.Sign(delta);
                var item = _getItemByStep(targetStep);
                if (item == null) return;
                if (item.locked)
                {
                    var idx = list.IndexOf(item);
                    onLockedItemSelecting?.Invoke(idx, item);
                    return;
                }

                scrollToStep((int)targetStep);
            }
        }

        private int _to = 0;
        private int _from = 0;

        [HideInInspector]
        public bool moving = false;
        private void scrollToStep(int step)
        {
            if(moving)
            {
                return;
            }
            moving = true;
            foreach(var item in list)
            {
                if(item.toggle)
                {
                    item.toggle.interactable = false;
                }
            }

            _from = _to;
            _to = step;

            DOGetter<float> getter = () => { return _step; };
            DOSetter<float> setter = (v) => { _step = v; _render(_step); };

            _showToggle(step);

            DOTween.To(getter, setter, step, tween).OnComplete(() =>
            {
                moving = false;
                foreach (var item in list)
                {
                    if (item.toggle)
                    {
                        item.toggle.interactable = true;
                    }
                }
            });

            var cur_idx = _getMidIdx(step);
            var selectedItem = _getItem(cur_idx);
            onItemSelected?.Invoke(cur_idx, selectedItem);
            foreach (var item in list)
            {
                if (item == selectedItem)
                {
                    item.onSelect();
                }
                else
                {
                    item.onDeselect();
                }
            }
        }


        private void _showToggle(float step)
        {
            var item = _getItemByStep(step);
            if(item == null)
            {
                return;
            }
            var toggle = item.toggle;
            if (toggle== null)
            {
                return;
            }
            toggle.isOn = true;
        }

        private int _getMidIdx(int step)
        {
            if (step >= 0)
            {
                return (int)(step % list.Count);
            }
            else
            {
                return (int)((list.Count + step % list.Count) % list.Count);
            }
        }

        private ExternLoopToggleGroupItem _getItem(int idx)
        {
            if (idx < 0 || idx >= list.Count)
            {
                return null;
            }
            return list[idx];
        }

        private ExternLoopToggleGroupItem _getItemByStep(int step)
        {
            var idx = _getMidIdx((int)step);
            if (idx < 0 || idx >= list.Count)
            {
                return null;
            }
            return list[idx];
        }

        private ExternLoopToggleGroupItem _getItemByStep(float step)
        {
            return _getItemByStep((int)step);
        }


        private void _render(float cur_step)
        {
            if(cur_step == _to)
            {
                return;
            }
            var delta = cur_step * spacing;


            int all_item_finished_jump_count = (int)(_from / list.Count);
          
            
            float cur_jumping_progress = _from == _to ? 0 : 1 - Mathf.Abs((cur_step - _to) / (_from - _to));

            for (int i = 0; i < list.Count; i++)
            {
                var pos = list[i].rect_transform.anchoredPosition;

                //计算初始位置,该位置会让排在最前面的item是list中最后第一个item
                pos.x = spacing * (i - list.Count/2 + 1.5f) + offset;
                pos.x += extraSpacing / 2;

                //进行完毕的基础位移
                pos.x -= delta;

                //list所有item都已经进行完毕的迁跃
                //比如说一共4个item, 当前step为5.5, 那么完整位移了5次，那么所有item都必定进行迁跃过1次（step % list.Count = 1)
                pos.x -= all_item_finished_jump_count * extraSpacing;

                //list中部分item已经进行完毕的迁跃
                int remain = (int)(_from % list.Count);
                if (remain > 0)
                {
                    if (i < remain)
                    {
                        pos.x -= extraSpacing;
                    }
                }
                else if(remain < 0)
                {
                    if (i >= ((remain + list.Count) % list.Count))
                    {
                        pos.x += extraSpacing;
                    }
                }


                //进行中的迁跃
                int start = _from < _to ? _from : _to;
                int end = _from > _to ? _from : _to;

                for (int j = start; j < end; j++)
                {
                    int m = (j % list.Count + list.Count) % list.Count;

                    if (i == m)
                    {
                        pos.x -= Mathf.Sign(_to - _from) * cur_jumping_progress * extraSpacing;
                    }
                }
                 
               
                //把x限制在指定范围内
                //如果range是3000，则要把x映射到[-1500,1500],即-1600->1400, 2100->-900
                if(pos.x < 0)
                {
                    pos.x = (pos.x - (_range / 2)) % _range + (_range / 2);
                }
                else
                {
                    pos.x = (pos.x + (_range / 2)) % _range - (_range / 2);
                }
                list[i].rect_transform.anchoredPosition = pos;
                
            }

            //if(Application.isPlaying)
            {
                //按x位置排序，先后设置各child的渲染顺序
                List<RectTransform> temp_list = new List<RectTransform>();

                foreach(var item in list)
                {
                    temp_list.Add(item.rect_transform);
                }

                temp_list.Sort((RectTransform a, RectTransform b) => {
                    return (int)(a.localPosition.x - b.localPosition.x);
                });
                foreach (var rect in temp_list)
                {
                    rect.SetAsFirstSibling();
                }
                if(mask)
                {
                    mask.SetSiblingIndex(maskSblingIndex);
                }
            }
        }
    }
}
