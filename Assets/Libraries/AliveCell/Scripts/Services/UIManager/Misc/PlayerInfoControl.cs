/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/8 16:39:34
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// PlayerInfoControl
    /// </summary>
    public class PlayerInfoControl : UISubControl
    {
        [SerializeField]
        protected ValueProgressBar _bloodBar;

        [SerializeField]
        protected ValueProgressBar _powerBar;

        [SerializeField]
        protected int _playerId;

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    {
                        App.On<int, PropertyType, object, object>(EventTypes.Game_PropertyChanged, OnGamePropertyChanged).SetFilter((args) => (((int)args[0]) == _playerId));
                        PlayerObject player = App.game.uplayer.GetPlayer(_playerId);
                        _bloodBar.Initialize(player.maxHp, player.hp);
                        _powerBar.Initialize(player.maxPower, player.power);
                    }
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    {
                        App.Off(this);
                    }
                    break;
            }
        }

        private void OnGamePropertyChanged(int id, PropertyType type, object oldValue, object newValue)
        {
            int value = (int)newValue;
            switch (type)
            {
                case PropertyType.Hp:
                    _bloodBar.SetValue(value);
                    break;

                case PropertyType.Power:
                    _powerBar.SetValue(value);
                    break;
            }
        }

        public void OnHeaderClick()
        {
            App.Trigger(EventTypes.UI_PlayerHeader, _playerId);
        }

        public void Initialize(int playerId)
        {
            this._playerId = playerId;
        }
    }
}