/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/8 12:46:59
 */

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// MainPanel
    /// </summary>
    public class MainPanel : UIPanel
    {
        public override string panelName => "Main";

        public override int sortWeight => 1;

        [SerializeField]
        protected GameObject _startBtn;

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    SceneService.SceneInfo info = App.scene.GetInfo();
                    _startBtn.SetActive((info.mode & GameMode.PVP) == 0);
                    break;

                case UIPanelStatus.Exit:
                    break;
            }
        }

        public void OnStartButton()
        {
            App.Trigger(EventTypes.UI_StartGame);
        }

        public void OnStartMatchButton()
        {
            App.Trigger(EventTypes.UI_StartMatch);
        }

        public void OnReplayButton()
        {
            App.Trigger(EventTypes.UI_ReplayGameList);
        }

        public void OnSelectLevelButton()
        {
            App.Trigger(EventTypes.UI_LevelList);
        }
    }
}