/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/8 12:47:21
 */

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// GameOverPanel
    /// </summary>
    public class GameOverPanel : UIPanel
    {
        public override string panelName => "GameOver";

        public override int sortWeight => 3;

        [SerializeField]
        protected Text _text;

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    UpdateText();
                    break;
            }
        }

        private void UpdateText()
        {
            if (App.game.gameMode != GameMode.Replay)
            {
                switch (App.game.gameState)
                {
                    case GameState.Successed:
                        _text.text = "挑战成功";
                        break;

                    case GameState.Failed:
                        _text.text = "挑战失败";
                        break;

                    default:
                        _text.text = "";
                        break;
                }
            }
            else
            {
                _text.text = "回放结束";
            }
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