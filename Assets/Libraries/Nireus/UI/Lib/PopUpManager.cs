//using UnityEngine;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using DG.Tweening;
//using System;

//namespace Nireus
//{

//    public class PopUpManager : MonoBehaviour
//    {
//        public class Data
//        {
//            public DepthSortUIRoot depth_ui_root;
//            public GameObject mask_gameobject;
//            public UIDialog tpl;
//            public object param;
//            public bool needsWaiting;
//            public float mask_alpha;
//            public bool show_mask;
//            public bool click_mask_close;
//            public int lastshow_timestamp;
//            public Func<bool> clickFunc;
//			public void Reset()
//            {
//                Destroy(depth_ui_root.gameObject);
//                Destroy(tpl.gameObject);
//                Destroy(mask_gameobject.gameObject);
//                depth_ui_root = null;
//                tpl = null;
//                mask_gameobject = null;
//                param = null;
//            }
//        }

//        private static PopUpManager _instance;
//        public static PopUpManager Instance => _instance;

//        void Awake()
//        {
//            _instance = this;
//        }

//        private readonly ObjectPoolImpl<Data> _dataPool =
//            new ObjectPoolImpl<Data>(OnGetComponentObject, OnReleaseComponentObject, OnNewComponentObject);

//        private readonly List<Data> _displayingList = new List<Data>();

//        private Data _displayingDialogFromWaitingQueue;
//        private readonly Queue<Data> _waitingQueue = new Queue<Data>();

//        private bool _block_popup = false;
//        private readonly Queue<Data> _blockQueue = new Queue<Data>();

//        private static void OnNewComponentObject(Data obj)
//        {
//            var gameObject =
//                GameObject.Instantiate(UnityEngine.Resources.Load<GameObject>("Prefabs/UI/DepthSortUIRoot"));
//            gameObject.name = "DepthSortUIRoot";
//            obj.depth_ui_root = gameObject.GetComponent<DepthSortUIRoot>();
//            obj.mask_gameobject = GameObject.Instantiate(UnityEngine.Resources.Load(NIREUS_UI_MASK_PATH, typeof(GameObject)) as GameObject);

//        }

//        private static void OnReleaseComponentObject(Data obj)
//        {
//            Transform paraent = Instance.transform;
//            obj.depth_ui_root.transform.SetParent(paraent);
//            obj.mask_gameobject.transform.SetParent(paraent);
//            obj.depth_ui_root.gameObject.SetActive(false);
//            obj.mask_gameobject.gameObject.SetActive(false);
//        }

//        private static void OnGetComponentObject(Data obj)
//        {            
//            obj.depth_ui_root.gameObject.SetActive(true);
//            obj.mask_gameobject.gameObject.SetActive(true);
//        }

//        public void SetBlockPopUp(bool value)
//        {
//            if (_block_popup == value)
//            {
//                return;
//            }
//            _block_popup = value;
//            if (!_block_popup && _blockQueue.Count > 0)
//            {
//                _AddPopUp(_blockQueue.Dequeue());
//            }
//        }

//        public void AddPopUp(UIDialog tpl, bool needsWaiting = false, object param = null, bool show_mask = true, bool click_mask_close = true,float mask_alpha = 0.7f,Func<bool> clickFunc = null)
//        {
//            if(tpl == null) return;
//            RemovePopUp(tpl,true);
//            Data data = _dataPool.Get();
//            data.tpl = tpl;
//            data.param = param;
//            data.needsWaiting = needsWaiting;
//            data.show_mask = show_mask;
//            data.mask_alpha = mask_alpha;
//            data.click_mask_close = click_mask_close;
//            data.lastshow_timestamp = TimeUtil.now;
//            data.clickFunc = clickFunc;
//            if (_block_popup)
//            {
//                _blockQueue.Enqueue(data);
//                return;
//            }
//            _AddPopUp(data);
//        }

//        private void _AddPopUp(Data data)
//        {
//            if (data.needsWaiting && _displayingDialogFromWaitingQueue != null)
//            {
//                _addWaitingDialog(data);
//            }
//            else
//            {
//                _showDialog(data);
//            }
//        }

//        private void _addWaitingDialog(Data data)
//        {
//            _waitingQueue.Enqueue(data);
//        }

//        private void _showDialog(Data data)
//        {
//            if (data.needsWaiting)
//            {
//                _displayingDialogFromWaitingQueue = data;
//            }
//            data.lastshow_timestamp = TimeUtil.now;
//            data.tpl.transform.SetParent(data.depth_ui_root.transform, false);
//            LayerManager.getInstance().addToLayer(data.depth_ui_root.transform, LayerType.POP_UP);
//            data.depth_ui_root.transform.position = new Vector3((_displayingList.Count*2 + 100) * 20, 0, 0);
//            InitMask(data);
//            _displayingList.Add(data);
//            _resetDepth();
//            data.tpl.OnShowDialog(data.param);
//        }

//        private void _resetDepth()
//        {
//            int i = 10;
//            foreach(var dialog in _displayingList)
//            {
//                dialog.depth_ui_root.uiCamera.depth = i++;
//            }
//        }

//        public void RemovePopUp(UIDialog tpl,bool is_open = false)
//        {
//            int index = Find(tpl);
//            if (index == -1) return;
//            Data data = _displayingList[index];
//            string _tplName = tpl.name;
//            _removeDialog(data,is_open);
            
//            ClientEventManager.Instance.Update(ClientEventDef.POPUP_DIALOG_CLOSE, _tplName);

//            if (!_block_popup && !IsAnyDialogDisplaying())
//            {
//                if (_blockQueue.Count > 0)
//                {
//                    _AddPopUp(_blockQueue.Dequeue());
//                }
//                else if (_displayingDialogFromWaitingQueue == null &&
//                    _waitingQueue.Count > 0)
//                {
//                    var waiting = _waitingQueue.Dequeue();
//                    _showDialog(waiting);
//                }
//            }
//            if (!IsAnyDialogDisplaying())
//            {
//                if (tpl != MainTaskNoticeUIDialog.Instance && AutoPopUpManager.Instance.Empty())
//                {
//                    MainTaskManager.Instance.CheckIfCanShowMainTaskNoticeUIDialog();
//                }
//                AutoPopUpManager.Instance.CheckPopNode();

//                ClientEventManager.Instance.Update(ClientEventDef.POPUP_DIALOG_CLEAR);
//            }
//        }

//        private void _removeDialog(Data data,bool is_open = false)
//        {
//            if (data.needsWaiting)
//            {
//                _displayingDialogFromWaitingQueue = null;
//            }

//            _displayingList.Remove(data);
//            _resetDepth();
//            LayerManager.getInstance().addToLayer(data.tpl, LayerType.HIDE);
//             if (is_open)
//             {
//                 data.tpl.OnHideDialog();
//                 _dataPool.Release(data);
//             }
//            else
//            {
//                data.tpl.OnHideDialog();
//                data.Reset();
//                #if !FAST_UNITY_EDITOR || !UNITY_EDITOR
//                //if(FightAniClipLoadManager.Instance.IsCanClean())
//                //{
//                //    Resources.UnloadUnusedAssets();
//                //}                    
//                #endif
//            }
//            //_dataPool.Release(data);
            
//        }

//        public void RemoveAllPopUp()
//        {
//            while(_displayingList.Count > 0)
//            {
//                _removeDialog(_displayingList[_displayingList.Count - 1]);
//            }
//            ClientEventManager.Instance.Update(ClientEventDef.POPUP_DIALOG_CLEAR);
//        }
//        int Find(UIDialog tpl)
//        {
//            if (tpl == null)
//            {
//                GameDebug.LogError("UIDialog tpl is null");
//                return -1;
//            }
//            for (int i = 0; i < _displayingList.Count; i++)
//            {
//                if (_displayingList[i].tpl == tpl)
//                    return i;
//            }
//            return -1;
//        }

//	    public Data GetTopLayerData(int reversedIndex = 0)
//	    {
//	        if (_displayingList.Count > reversedIndex)
//	        {
//	            return _displayingList[_displayingList.Count - 1 - reversedIndex];
//	        }

//	        return null;
//	    }

//        public Data GetData(UIDialog tpl)
//        {
//            int index = Find(tpl);
//            if (index == -1) return null;
//            Data data = _displayingList[index];
//            return data;
//        }

//        public Camera GetUICamera(UIDialog tpl)
//        {
//            var data = GetData(tpl);
//            return data?.depth_ui_root?.uiCamera;
//        }

//        public void SetVisible(bool active)
//        {
//            foreach (Data data in _displayingList)
//            {
//                UIDialog tpl = data.tpl;
//                if (active)
//                {
//                    tpl.OnShowDialog(data.param);
//                }
//                else
//                {
//                    tpl.OnHideDialog();
//                }
//            }
//        }

//        public bool IsShow(string dialogName)
//        {
//            for (int i = 0; i < _displayingList.Count; i++)
//            {
//                if (_displayingList[i].tpl.GetType().ToString() == dialogName)
//                    return true;
//            }
//            return false;
//        }

//        public bool IsAnyDialogDisplaying()
//        {
//            return _displayingList.Count > 0;
//        }
//        public int DialogDisplayingCount()
//        {
//            return _displayingList.Count;
//        }

//        private static readonly string NIREUS_UI_MASK_PATH = "Prefabs/UI/NireusMaskPrefab";

//        void InitMask(Data data)
//        {
//            var ui_dialog = data.tpl;
//            RectTransform find_mask = ui_dialog.transform.Find("UIMaskLayer") as RectTransform;
//            GameObject mask_go;
//            if (find_mask != null)
//            {
//                mask_go = find_mask.gameObject;
//                data.mask_gameobject.gameObject.SetActive(false);
//            }
//            else
//            {
//                mask_go = data.mask_gameobject;
//                mask_go.transform.SetParent(ui_dialog.transform, false);
//                var rect_transform = mask_go.GetComponent<RectTransform>();
//                mask_go.transform.SetSiblingIndex(0);//设置到root第一位置
//                rect_transform.sizeDelta = ui_dialog.rectTransform.sizeDelta + new Vector2(10,10);
//                rect_transform.anchoredPosition = ui_dialog.rectTransform.anchoredPosition;
//                rect_transform.localScale = Vector3.one;
//            }
//            mask_go.gameObject.SetActive(data.show_mask);

//            Image image = mask_go.GetComponent<Image>();
//            if (image == null)
//            {
//                image = mask_go.AddComponent<Image>();
//            }
//            image.color = Color.black;
//            image.fillCenter = true;
//            image.SetAlpha(data.mask_alpha);

//            if (data.click_mask_close)
//            {
//                image.AddOnElementClickListener((e)=>{

//                    if (data.clickFunc != null)
//                    {
//                        var value = data.clickFunc.Invoke();
//                        if (value)
//                        {
//                            data.clickFunc = null;
//                        }
//                        return;
//                    }

//                    ui_dialog.CloseDialog();

//                }, ui_dialog.ToString(), "UIMaskLayer",null);
//                //image.onClick((img) =>
//                //{
//                //    //AkSoundEngine.PostEvent("btn_close",data.tpl.gameObject);
//                //    if (data.clickFunc != null)
//                //    {
//                //        var value = data.clickFunc.Invoke();
//                //        if (value)
//                //        {
//                //            data.clickFunc = null;
//                //        }
//                //        return;
//                //    }

//                //    ui_dialog.CloseDialog();
//                //});
//            }
//            else
//            {
//                image.onClick(null);
//            }
//        }

//        public void ClearHideDialog()
//        {
//            Transform hideTrans = LayerManager.getInstance().getLayer(LayerType.HIDE);
//            UIDialog[] tempDialog = hideTrans.GetComponentsInChildren<UIDialog>(true);
//            for (int i = 0; i < tempDialog.Length; i++)
//            { 
//                PopUpManager.Instance.RemovePopUp(tempDialog[i]);
//                Destroy(tempDialog[i].gameObject);
//            }
//            Resources.UnloadUnusedAssets();
//        }
//    }
//}
