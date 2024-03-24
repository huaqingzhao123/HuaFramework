/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/7 14:49:11
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
    /// ReplayControl
    /// </summary>
    public class ReplayControl : UISubControl
    {
        [SerializeField]
        protected Slider _progress;

        [SerializeField]
        protected Text _timeText;

        [SerializeField]
        protected Text _scaleText;

        [SerializeField]
        protected GameObject _tipText;

        [SerializeField]
        protected GameObject _playBtn;

        [SerializeField]
        protected GameObject _pauseBtn;

        protected GameWorld world => (GameWorld)App.game;

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    _playBtn.SetActive(false);
                    _pauseBtn.SetActive(true);
                    //_tipText.SetActive(world.isMultiPlayer);
                    UpdateValue();
                    break;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateValue();
        }

        private void UpdateValue()
        {
            //_progress.value = world.frameProgress;
            //_timeText.text = $"{world.frameIndex}/{world.frameMax}";
            _scaleText.text = $"x{world.timeScale}";
        }

        private void UpdateButton()
        {
            _playBtn.SetActive(!world.isRunning);
            _pauseBtn.SetActive(world.isRunning);
        }

        public void OnAddTimeScaleButton(float offset)
        {
            App.Trigger(EventTypes.UI_AddTimeScale, offset);
        }

        public void OnPlayButton()
        {
            App.Trigger(EventTypes.UI_PlayPause, true);
            UpdateButton();
        }

        public void OnPauseButton()
        {
            App.Trigger(EventTypes.UI_PlayPause, false);
            UpdateButton();
        }
    }
}