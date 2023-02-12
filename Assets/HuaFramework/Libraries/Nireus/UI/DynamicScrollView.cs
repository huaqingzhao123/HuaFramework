using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Specialized;
using System.Collections;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Nireus
{
    //public enum DynamicScrollViewType
    //{
    //    SCALE = 1,
    //    POSOTION = 2,
    //}

    //public enum DragDirection
    //{
    //    LEFT = 1,
    //    RIGHT = 2,
    //}

    public class DynamicScrollView : MonoBehaviour, ICancelHandler, IDragHandler, IEndDragHandler
    {
        public Action<int> OnEndDragCallback { get; set; }
        public Action<int> OnIndexChange { get; internal set; }

        public Action<PointerEventData> OnDragCallback;
        public Action<int> OnClickItemCallback;

        public float scale_rate = 600;
        public float space = 185;
        public GameObject goItem;
        //个数为奇数
        public int ItemCount = 5;
        public int CurIndex;
        public bool is_draging;

        public bool EditorMode = false;

        //选项个数为奇数，中间项显示在最中间
        public List<DynamicScrollViewItem> Items { get { return m_Items;} }
        List<DynamicScrollViewItem> m_Items;
        RectTransform m_ItemRoot;

        int m_MidItemIndex;
        int m_ListCount;
        float m_AnimTime = 0.3f;
        Vector2[] m_StartPoses;
        private Vector3 _deltaPos;


        void Awake()
        {
            if (m_Items != null) return;

            if (EditorMode)
            {
                Init(ItemCount);//test
            }

        }

        public void Init(int count)
        {
            ItemCount = count;
            m_MidItemIndex = (ItemCount - 1) / 2;

            m_ListCount = count;

            m_Items = new List<DynamicScrollViewItem>();
            for (int i = 0; i < ItemCount; i++)
            {
                DynamicScrollViewItem item;
                if (i == m_MidItemIndex)
                {
                    item = goItem.GetComponent<DynamicScrollViewItem>();
                }
                else
                {
                    item = Instantiate(goItem).GetComponent<DynamicScrollViewItem>();
                    item.rectTransform.SetParent(transform);
                    item.rectTransform.localScale = 0.8f * Vector2.one;
                    item.rectTransform.anchoredPosition = goItem.GetComponent<RectTransform>().anchoredPosition;
                    item.rectTransform.anchoredPosition += new Vector2((i - m_MidItemIndex) * space, 0);
                    
                }

                item.gameObject.name = $"DynamicScrollViewItem{i}";
                m_Items.Add(item);
            }

            m_ItemRoot = transform.GetChild(0) as RectTransform;
            m_StartPoses = new Vector2[m_Items.Count];
            for (int i = 0; i < m_Items.Count; i++)
            {
                m_StartPoses[i] = m_Items[i].rectTransform.anchoredPosition;
            }
            _Init();
        }


        public void FocusIndex(int index)
        {
            OnIndexChange?.Invoke(index);

        }

        void Start()
        {
            //Init(ItemCount);
        }

        void _Init()
        {
            if (m_Items == null) Awake();
            CurIndex = -1;

            for (int i = 0; i < m_Items.Count; i++)
            {
                m_Items[i].gameObject.SetActive(false);
                m_Items[i].rectTransform.anchoredPosition = m_StartPoses[i];
            }


            if (m_ListCount < m_Items.Count)
            {
                for (int i = 0; i < m_ListCount; i++)
                {
                    m_Items[i].gameObject.SetActive(true);
                    m_Items[i].GetComponent<DynamicScrollViewItem>()._Index = i;
                    m_Items[i].GetComponent<DynamicScrollViewItem>().Init();
                }

                //移位
                var midIndex = m_ListCount / 2;
                var dis = m_StartPoses[m_MidItemIndex].x - m_Items[midIndex].rectTransform.anchoredPosition.x;
                for (int i = 0; i < m_Items.Count; i++)
                {
                    m_Items[i].rectTransform.anchoredPosition += new Vector2(dis, 0);
                }

                for (int i = 0; i < m_ListCount; i++)
                {
                    if (i == midIndex)
                    {
                        //选中
                        m_Items[midIndex].rectTransform.localScale = Vector2.one;
                        //if (m_Items[midIndex].imgSelectTag != null) m_Items[midIndex].imgSelectTag.color = new Color(1, 1, 1, 1);
                    }
                    else
                    {
                        m_Items[midIndex].rectTransform.localScale = 0.8f * Vector2.one;
                        //if (m_Items[midIndex].imgSelectTag != null) m_Items[midIndex].imgSelectTag.color = new Color(1, 1, 1, 0);
                    }
                }

            }
            else
            {
                for (int i = 0; i < m_Items.Count; i++)
                {
                    m_Items[i].gameObject.SetActive(true);
                    m_Items[i].GetComponent<DynamicScrollViewItem>()._Index = i;
                    m_Items[i].GetComponent<DynamicScrollViewItem>().Init();
                }
            }
            SortIndex();
        }

        public void OnDrag(PointerEventData eventData)
        {
            is_draging = true;
            eventData.delta = eventData.delta * 0.8f;
            _deltaPos = eventData.delta;

            for (int i = 0; i < m_Items.Count; i++)
            {
                m_Items[i].rectTransform.anchoredPosition += new Vector2(_deltaPos.x, 0);
            }
            SortIndex();
            OnDragCallback?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            is_draging = true;
            var dis = m_StartPoses[(m_Items.Count - 1) / 2].x - m_Items[CurIndex].rectTransform.anchoredPosition.x;
            var time = m_AnimTime;// Mathf.Abs(dis) / cellSize * m_AnimTime;
            for (int i = 0; i < m_Items.Count; i++)
            {
                var t = m_Items[i].rectTransform.DOLocalMoveX(m_Items[i].rectTransform.anchoredPosition.x + dis, time);
                if (i == m_Items.Count - 1)
                {
                    t.OnComplete(() =>
                    {
                        {
                            is_draging = false;
                            SortIndex();
                        }
                    });
                }
            }
            OnEndDragCallback?.Invoke(CurIndex);
        }


        public void ClickFocus(int index, bool click_focused)
        {
            var dis = m_StartPoses[(m_Items.Count - 1) / 2].x - m_Items[index].rectTransform.anchoredPosition.x;
            var time = m_AnimTime;// Mathf.Abs(dis) / cellSize * m_AnimTime;
            is_draging = true;
            for (int i = 0; i < m_Items.Count; i++)
            {
                var t = m_Items[i].rectTransform.DOLocalMoveX(m_Items[i].rectTransform.anchoredPosition.x + dis, time);
                if (i == m_Items.Count - 1)
                {
                    t.OnComplete(() =>
                    {                        
                        SortIndex();      
                        is_draging = false;
                    });
                }
            }
            OnIndexChange?.Invoke(index);
            if (click_focused)
            {
                OnClickItemCallback?.Invoke(index);
            }
        }

        public void SortIndex()
        {
            List<DynamicScrollViewItem> list = new List<DynamicScrollViewItem>(m_Items);

            list.Sort((a, b) =>
            {

                if (a.transform.localScale.x < b.transform.localScale.x)
                    return -1;
                return 0;
            });

            list.ForEach(
                p => { p.transform.SetAsLastSibling(); }
                );

        }

        public void OnCancel(BaseEventData eventData)
        {
            is_draging = false;
        }

    }
}

