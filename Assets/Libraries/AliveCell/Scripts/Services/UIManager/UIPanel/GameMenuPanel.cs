/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/6 13:25:11
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// GameMenuPanel
    /// </summary>
    public class GameMenuPanel : UIPanel
    {
        public override string panelName => "GameMenu";
        public override int sortWeight => 2;

        [SerializeField]
        protected GameObject _replayBtnObj;

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    _replayBtnObj.SetActive(App.game.gameMode == GameMode.Replay);
                    App.camera.isRunning = false;
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    App.camera.isRunning = true;
                    break;
            }
        }

        public void OnResumeButton()
        {
            App.Trigger(EventTypes.UI_ResumeGame);
        }

        public void OnQuitButton()
        {
            App.Trigger(EventTypes.UI_EndGame);
        }

        public void OnReplayButton()
        {
            App.Trigger(EventTypes.UI_ReplayGame);
        }
    }
}