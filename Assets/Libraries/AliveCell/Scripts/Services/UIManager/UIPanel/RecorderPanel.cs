/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/21 23:49:09
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// RecorderPanel
    /// </summary>
    public class RecorderPanel : UIPanel
    {
        [SerializeField]
        protected ScrollRect _scrollRect;

        public void OnBackButton()
        {
            manager.Hide<RecorderPanel>();
            manager.Show<MainPanel>();
        }

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    InitRecoderItem();//创建要在父级调用之前，那样才能接收到状态改变的消息
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    break;
            }

            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    InitRecoderItem();
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    App.camera.fsm.ChangeState<CameraStates.None>();
                    break;
            }
        }

        private void InitRecoderItem()
        {
            List<GameObject> childs = new List<GameObject>(_scrollRect.content.childCount);
            foreach (Transform item in _scrollRect.content)
            {
                childs.Add(item.gameObject);
            }
            foreach (var item in childs)
            {
                App.DestroyGO(item);
            }

            //var records = App.record.recordDatas;
            //int length = records.Count;
            //for (int i = 0; i < length; i++)
            //{
            //    var record = records[i];
            //    GameObject obj = App.CreateGO(50000002);
            //    obj.transform.SetParent(_scrollRect.content, false);
            //    var item = obj.GetComponent<RecorderListItem>();
            //    item.Initialize(this, record);
            //}
        }

        public void OnPlayRecoder(RecorderListItem recorderListItem)
        {
            //LogHandler.Log($"Play {recorderListItem.data.displayName}");
            //App.Trigger(EventTypes.UI_ReplaySelectGame, recorderListItem.data);
        }

        public void OnDeleteRecoder(RecorderListItem recorderListItem)
        {
            //LogHandler.Log($"Delete {recorderListItem.data.displayName}");
            //App.record.RemoveRecord(recorderListItem.data.fileName);
            App.DestroyGO(recorderListItem);
        }
    }
}