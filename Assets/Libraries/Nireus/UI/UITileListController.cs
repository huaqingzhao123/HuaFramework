using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using System.Linq;

namespace Nireus
{
    public class UITileListController<T, K> where K : Component, IUITileListItemRenderer<T>
    {
        private struct UITileListItemData
        {
            public int dataIndex;
            public K item;
            public GameObject gameObject;
        }

        protected ScrollRect _com_scroll_rect;

        protected int _row_cnt = 0;
        protected int _col_cnt = 0;
        protected float _row_height = 0;
        protected float _col_width = 0;
        private bool _scrollEnd = false;
        private IEnumerable<T> _datas;

        private Vector2 _lastPosition;
        private int _real_row_cnt = 0;      // 实际渲染的列数和行数;
        private int _real_col_cnt = 0;
        private Vector2 _row_col_offset;//offset y pos\
        private Vector2 _pivot = default(Vector2);
        private float _vertical_space = 4;//间隙
        private float _horizontal_space = 4;//间隙

        private List<UITileListItemData> _item_list = new List<UITileListItemData>();
        private int _item_showed_count = 0;
        private int _data_start_index = 0;

        public void InitTileList(ScrollRect scroll_rect)
        {
            InitTileList(scroll_rect, 0, 0);
        }
        public void InitTileList(ScrollRect scroll_rect, float vertical_space, float horizontal_space)
        {
            InitTileList(scroll_rect, null, vertical_space, horizontal_space);
        }

        public void InitTileList(ScrollRect scroll_rect, GameObject item, float vertical_space, float horizontal_spacep)
        {
            InitTileList(scroll_rect, item, 0, 0, 0, 0, vertical_space, horizontal_spacep);
        }

        Vector3 _GetAnchorLocal(Rect parent_rect, Vector2 anchor)
        {
            return new Vector2(
                Mathf.LerpUnclamped(parent_rect.x, parent_rect.xMax, anchor.x),
                Mathf.LerpUnclamped(parent_rect.y, parent_rect.yMax, anchor.y)
                );
        }

        public void InitTileList(ScrollRect scroll_rect, GameObject item, int col, int row, float col_width, float row_height, float vertical_space, float horizontal_space /*, IIUITileListItemBase owner*/)
        {
            _vertical_space = vertical_space;
            _horizontal_space = horizontal_space;
            _com_scroll_rect = scroll_rect;
            RectTransform content_transform = scroll_rect.content;
            {//TileList会自己计算排列 所以警用Unity自带的
                var vlg = content_transform.GetComponent<VerticalLayoutGroup>();
                if (vlg != null) vlg.enabled = false;
                var hlg = content_transform.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null) hlg.enabled = false;
                var glg = content_transform.GetComponent<GridLayoutGroup>();
                if (glg != null) glg.enabled = false;
                var csf = content_transform.GetComponent<ContentSizeFitter>();
                if (csf != null) csf.enabled = false;
            }

            _pool_Parent = content_transform;
            if (item == null)
            {
                int childCount = content_transform.childCount;
                RectTransform tmp_item = null;
                for (int i = 0; i < childCount; ++i)
                {
                    tmp_item = content_transform.GetChild(i) as RectTransform;
                    Vector2 tmp_anchor = new Vector2();
                    tmp_anchor.x = tmp_item.anchorMin.x;
                    tmp_anchor.y = tmp_item.anchorMax.y;
                    Vector2 g_pos = _GetAnchorLocal(content_transform.rect, tmp_anchor);
                    //g_pos = content_transform.InverseTransformPoint(g_pos);
                    tmp_item.anchorMin = new Vector2(0, 1);
                    tmp_item.anchorMax = new Vector2(0, 1);
                    tmp_item.pivot = new Vector2(0, 1);
                    if (tmp_item.GetComponent<K>() == null)
                    {
                        tmp_item.gameObject.AddComponent(typeof(K));
                    }
                    //PoolPush(tmp_item.gameObject, true);
                    tmp_item.gameObject.SetActive(false);
                    _AddNewItem(tmp_item.gameObject);
                    //tmp_item.ForceUpdateRectTransforms();
                    //_row_col_offset = tmp_item.anchoredPosition;
                    /*if (find_tr.gameObject.activeSelf && find_tr.name == name)
                    {
                        return find_tr;
                    }*/
                }
                item = tmp_item.gameObject;
            }
            else
            {
                item.SetActive(false);
                //_AddNewItem(item);
            }
            _pool_Prefab = item;
            RectTransform item_rt = item.transform as RectTransform;

            _col_cnt = col;
            _row_cnt = row;

            _col_width = col_width;
            _row_height = row_height;

            if (_com_scroll_rect.vertical)
            {
                if (_col_cnt == 0)
                {
                    _col_cnt = 1;
                }
                if (_col_width == 0)
                {
                    _col_width = Mathf.FloorToInt(_com_scroll_rect.content.rect.width);
                }
            }
            else
            {
                if (_row_cnt == 0)
                {
                    _row_cnt = 1;
                }
                if (_row_height == 0)
                {
                    _row_height = Mathf.FloorToInt(_com_scroll_rect.content.rect.height);
                }
            }

            if (_col_width == 0)
            {
                _col_width = item_rt.sizeDelta.x;
            }

            if (_row_height == 0)
            {
                _row_height = item_rt.sizeDelta.y;
            }

            _row_height += _vertical_space;
            _col_width += _horizontal_space;
        }

        void OnEnable()
        {
            _scrollEnd = false;
        }

        public void ResetPosition(bool refresh_now = true)
        {
            _com_scroll_rect.content.anchoredPosition = Vector2.zero;
            if (refresh_now)
            {
                _RefreshForDataProvider();
            }
        }

        public void SetOffset(float x, float y)
        {
            _row_col_offset.x = x;
            _row_col_offset.y = y;
        }

        public void SetItemPivot(float x,float y)
        {
            _pivot = new Vector2(x,y);
        }

        public void ClearItems(bool is_destoryall = false)
        {
            _data_start_index = -1;
            while (_item_showed_count > 0)
            {
                _item_list[_item_showed_count - 1].gameObject.SetActive(false);
                _item_showed_count--;
            }
        }

        public void SetDataProvider(IEnumerable<T> datas)
        {
            _datas = datas;
            _RefreshForDataProvider();
        }

        private void _RefreshForDataProvider()
        {
            int item_data_count = _GetItemDataCount();
            _com_scroll_rect.onValueChanged.RemoveListener(OnValueChanged);
            ClearItems();
            Vector2 content_position = _com_scroll_rect.content.anchoredPosition;
            Rect view_rect = _com_scroll_rect.viewport.rect;
            float lastNormalizedPos = (1f - content_position.y / view_rect.height);
            _lastPosition = new Vector2(content_position.x, lastNormalizedPos);
            _real_col_cnt = _col_cnt;
            _real_row_cnt = _row_cnt;
            if (_col_cnt != 0)
            {
                // enableScrollVertical(true);
            }
            else
            {
                _real_col_cnt = item_data_count / _row_cnt + (item_data_count % _row_cnt == 0 ? 0 : 1);
            }
            if (_row_cnt != 0)
            {
                // enableScrollHorizone(true);
            }
            else
            {
                _real_row_cnt = item_data_count / _col_cnt + (item_data_count % _col_cnt == 0 ? 0 : 1);
            }

            float width = _real_col_cnt * _col_width;
            float height = _real_row_cnt * _row_height;

            if (width < view_rect.width) width = view_rect.width;
            if (height < view_rect.height) height = view_rect.height;

            // 设置Panel大小;
            _com_scroll_rect.content.sizeDelta = new Vector2(width, height);
            //_com_scroll_rect.content.anchoredPosition = new Vector2(0,0);

            if (_com_scroll_rect.vertical)
            {

                int viewRow = Mathf.CeilToInt(view_rect.height / _row_height);
                int maxRow = _row_cnt > 0 ? _row_cnt : Mathf.CeilToInt((float)item_data_count / (float)_col_cnt);
                int viewRowExpand = Mathf.Min(maxRow, Mathf.CeilToInt(view_rect.height / _row_height) + 1);//+1 top or bot
                // 竖着滑动;
                int start_row = Mathf.Clamp(Mathf.FloorToInt(content_position.y / _row_height), 0, maxRow);
                start_row = Mathf.Max(0, start_row >= maxRow - viewRow ? maxRow - viewRow - 1 : start_row);

                _UpdateItems(start_row, 0, viewRowExpand, _col_cnt);
            }
            else
            {
                int viewCol = Mathf.CeilToInt(view_rect.width / _col_width);
                int maxCol = _col_cnt > 0 ? _col_cnt : Mathf.CeilToInt((float)item_data_count / (float)_row_cnt);

                int viewColExpand = Mathf.Min(maxCol, Mathf.CeilToInt(view_rect.width / _col_width) + 1);//+1 left or right

                // 横滑动;
                int start_col = Mathf.Clamp(Mathf.FloorToInt(-content_position.x / _col_width), 0, maxCol);
                start_col = Mathf.Max(0, start_col >= maxCol - viewCol ? maxCol - viewCol - 1 : start_col);

                _UpdateItems(0, start_col, _row_cnt, viewColExpand);
            }

            _com_scroll_rect.onValueChanged.AddListener(OnValueChanged);
        }

        void LateUpdate()
        {
            _lastPosition = _com_scroll_rect.content.anchoredPosition;
        }


        protected void OnValueChanged(Vector2 pos)
        {
            lock (_item_list)
            {
                if (_com_scroll_rect.vertical)
                    OnValueChangeVertical(pos);
                else
                    OnValueChangeHorizontal(pos);
            }
        }


        void OnValueChangeVertical(Vector2 pos)
        {
            int item_data_count = _GetItemDataCount();
            if (item_data_count <= 0) return;
            int lastRow = Mathf.FloorToInt(_lastPosition.y / _row_height);
            Vector2 content_position = _com_scroll_rect.content.anchoredPosition;
            int curRow = Mathf.FloorToInt(content_position.y / _row_height);
            if (lastRow == curRow) return;
            Rect view_rect = _com_scroll_rect.viewport.rect;
            int viewRow = Mathf.CeilToInt(view_rect.height / _row_height);
            int maxRow = _row_cnt > 0 ? _row_cnt : Mathf.CeilToInt((float)item_data_count / (float)_col_cnt);
            int viewRowExpand = Mathf.Min(maxRow, Mathf.CeilToInt(view_rect.height / _row_height) + 1);//+1 top or bot
            int start_row = Mathf.Clamp(curRow, 0, maxRow);
            _lastPosition = content_position;
            start_row = Mathf.Max(0, start_row >= maxRow - viewRow ? maxRow - viewRow - 1 : start_row);

            _UpdateItems(start_row, 0, viewRowExpand, _col_cnt);
        }

        private void _UpdateItems(int start_row, int start_column, int row_showed_count, int column_showed_count)
        {
            int item_data_count = _GetItemDataCount();
            int data_start = start_row * column_showed_count;
            int show_data_count = row_showed_count * column_showed_count;
            int data_end = data_start + show_data_count - 1;
            if (data_end >= item_data_count)
                data_end = item_data_count - 1;
            if (data_end < data_start)
            {
                show_data_count = 0;
            }
            else
            {
                show_data_count = data_end - data_start + 1;
            }
            _ExternItem(show_data_count);

            int no_change_data_start = Mathf.Max(data_start, _data_start_index);
            int no_change_data_end = Mathf.Min(data_end, _data_start_index + _item_showed_count - 1);

            for (int tmp_data_index = data_start; tmp_data_index <= data_end; ++tmp_data_index)
            {
                int data_index = tmp_data_index;
                if (_data_start_index > data_start) //后移
                {
                    data_index = data_end - (tmp_data_index - data_start);
                }
                int item_index = data_index - data_start;
                UITileListItemData item = _item_list[item_index];
                if (data_index >= no_change_data_start && data_index <= no_change_data_end)
                {
                    int old_item_index = data_index - _data_start_index;
                    UITileListItemData old_item = _item_list[old_item_index];
                    _item_list[item_index] = old_item;
                    _item_list[old_item_index] = item;
                }
                else
                {
                    if (item_index >= _item_showed_count)
                    {
                        item.gameObject.SetActive(true);
                        _item_showed_count++;
                    }

                    //update postion
                    RectTransform item_updater_rt = item.gameObject.transform as RectTransform;
                    int row = Mathf.FloorToInt(1.0f * data_index / column_showed_count);
                    int col = data_index % column_showed_count;
                    float uiRowPos = -row * _row_height;

                    item_updater_rt.anchoredPosition = new Vector2(col * _col_width + _row_col_offset.x, uiRowPos + _row_col_offset.y);
                    //update data
                    item.item.UpdateTileListItemData(_datas.ElementAt(data_index), data_index);
                }
            }

            _data_start_index = data_start;
            while (_item_showed_count > show_data_count)
            {
                _item_list[_item_showed_count - 1].gameObject.SetActive(false);
                _item_showed_count--;
            }
        }


        void OnValueChangeHorizontal(Vector2 pos)
        {
            int lastCol = Mathf.FloorToInt(-_lastPosition.x / _col_width);
            Vector2 content_position = _com_scroll_rect.content.anchoredPosition;
            int curCol = Mathf.FloorToInt(-content_position.x / _col_width);
            if (lastCol == curCol) return;
            Rect view_rect = _com_scroll_rect.viewport.rect;
            int viewCol = Mathf.CeilToInt(view_rect.width / _col_width);
            int maxCol = _col_cnt > 0 ? _col_cnt : Mathf.CeilToInt((float)_GetItemDataCount() / (float)_row_cnt);
            int viewColExpand = Mathf.Min(maxCol, Mathf.CeilToInt(view_rect.width / _col_width) + 1);//+1 left or right
            // 横滑动;
            int start_col = Mathf.Clamp(curCol, 0, maxCol);
            start_col = Mathf.Max(0, start_col >= maxCol - viewCol ? maxCol - viewCol - 1 : start_col);

            _lastPosition = content_position;

            _UpdateItems(0, start_col, _row_cnt, viewColExpand);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _lastPosition = _com_scroll_rect.content.anchoredPosition;
        }

        /*public void SetScrollEndCallback(Delegate.CallFuncUIBase callback)
        {
            OnScrollEndCallback = callback;
        }*/

        private void CheckScrollEnd()
        {
            Vector2 content_position = _com_scroll_rect.content.anchoredPosition;
            Vector2 content_size = _com_scroll_rect.content.sizeDelta;
            Rect view_rect = _com_scroll_rect.viewport.rect;
            if (_scrollEnd == false && content_position.y + view_rect.height > content_size.y + _row_height / 3f)
            {
                _scrollEnd = true;
                /*if (OnScrollEndCallback != null)
                    OnScrollEndCallback(this);*/
            }
            else if (_scrollEnd && content_position.y + view_rect.height < content_size.y + _row_height / 3f)
            {
                _scrollEnd = false;
            }
        }

        private int _GetItemDataCount()
        {
            return _datas == null ? 0 : _datas.Count();
        }

        private GameObject _pool_Prefab;
        private Transform _pool_Parent;

        private void _ExternItem(int max_count)
        {
            int expand_count = max_count - _item_list.Count;
            for (int i = 0; i < expand_count; ++i)
            {
                GameObject newItem = GameObject.Instantiate(_pool_Prefab, _pool_Parent) as GameObject;
                _AddNewItem(newItem);
            }
        }

        private void _AddNewItem(GameObject newItem)
        {
            var item = newItem.GetComponent<K>();
            if (item == null)
            {
                item = newItem.AddComponent<K>();
            }
            if (_pivot != default(Vector2))
            {
                RectTransform r_transfrom = item.GetComponent<RectTransform>();
                if (r_transfrom != null)
                    r_transfrom.pivot = _pivot;
            }
            UITileListItemData item_data = new UITileListItemData
            {
                dataIndex = -1,
                gameObject = newItem,
                item = item
            };
            _item_list.Add(item_data);
        }
    }
}