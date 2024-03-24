/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/6 21:33:38
 */

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// ReplayPanel
    /// </summary>
    public class ReplayPanel : UIPanel
    {
        public override string panelName => "Replay";

        [SerializeField]
        protected GameObject _inputObj;

        [SerializeField]
        protected PlayerInfoControl selfInfoControl;

        [SerializeField]
        protected Transform _playerPartyInfoRoot;

        protected override void Awake()
        {
            base.Awake();
#if UNITY_STANDALONE
            _inputObj.SetActive(false);
#endif
        }

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    InitPlayerInfo();//创建要在父级调用之前，那样才能接收到状态改变的消息
                    break;
            }

            base.OnStateChange(status, enterOrExit);
        }

        private void InitPlayerInfo()
        {
            List<GameObject> childs = new List<GameObject>(_playerPartyInfoRoot.childCount);
            foreach (Transform item in _playerPartyInfoRoot)
            {
                childs.Add(item.gameObject);
            }
            foreach (var item in childs)
            {
                App.DestroyGO(item);
            }

            int selfId = App.game.uplayer.selfUid;
            selfInfoControl.Initialize(selfId);

            foreach (var uid in App.game.uplayer.uids)
            {
                if (selfId == uid)
                {
                    continue;
                }

                GameObject infoObj = App.CreateGO(50000003);
                infoObj.transform.SetParent(_playerPartyInfoRoot, false);
                PlayerInfoControl playerInfoCtrl = infoObj.GetComponent<PlayerInfoControl>();
                playerInfoCtrl.Initialize(uid);
            }
        }

        public void OnMenuButton()
        {
            App.Trigger(EventTypes.UI_GameMenu);
        }
    }
}