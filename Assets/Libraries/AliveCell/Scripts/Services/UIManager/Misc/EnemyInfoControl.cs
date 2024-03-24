/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/8 18:03:34
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using DG.Tweening;

namespace AliveCell
{
    /// <summary>
    /// EnemyInfoControl
    /// </summary>
    public class EnemyInfoControl : UISubControl
    {
        [SerializeField]
        protected ValueProgressBar _bloodBar;

        [SerializeField]
        protected CanvasGroup _canvasGroup;

        [SerializeField]
        protected float _fadeTime = 0.5f;

        [SerializeField]
        protected float _hideWaitTime = 2f;

        private int _enemyId;
        private float _showTimer = 0f;
        private bool _isCombatMode = false;
        private bool _isShowing = false;

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    {
                        _enemyId = UObjectSystem.noneID;
                        _canvasGroup.alpha = 0f;
                        _showTimer = 0f;
                        _isCombatMode = false;
                        _isShowing = false;
                        App.On<InjuredResult, InjuredInfo>(EventTypes.Game_Injured, OnGameInjured).SetFilter((args) =>
                        {
                            int id = ((InjuredResult)args[0]).id;
                            InjuredInfo info = (InjuredInfo)args[1];
                            return App.game.uenemy.IsEnemy(id);
                            //&& App.game.ucamera.CheckFollow(info.info.id)
                            //&& App.game.uplayer.IsPlayer(info.info.id);//只显示自己的攻击目标
                        });
                    }
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    {
                        App.Off(this);
                    }
                    break;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            PlayerObject player = App.game.uplayer.GetSelfPlayer();
            _isCombatMode = player?.CheckCombatMode() ?? false;
            if (!_isCombatMode || _enemyId == UObjectSystem.noneID)
            {
                _showTimer += Time.deltaTime;
                if (_isShowing && _showTimer > _hideWaitTime)
                {
                    _isShowing = false;

                    _canvasGroup.DOKill(this);
                    _canvasGroup.DOFade(0f, _fadeTime);
                }
            }
        }

        private void OnGameInjured(InjuredResult result, InjuredInfo info)
        {
            PlayerObject player = App.game.uplayer.GetPlayer(info.info.id);
            EnemyObject enemy = App.game.uenemy.GetEnemy(_enemyId);

            bool isFollow = App.game.ucamera.CheckFollow(info.info.id);
            if (!isFollow)
            {//其他东西攻击造成
                if (_isShowing && _enemyId != UObjectSystem.noneID && _enemyId == result.id)
                {
                    _bloodBar.SetValue(enemy.hp);
                }
                return;
            }

            if (player == null)
            {//不是当前望向的玩家，则直接退出
                return;
            }

            bool needShowInfo = false;

            int aimEnemyObjId = UObjectSystem.noneID;

            if ((player == null)
                || ((result.id != aimEnemyObjId) && (_enemyId == UObjectSystem.noneID)))
            {
                aimEnemyObjId = result.id;
            }
            else
            {
                aimEnemyObjId = player.datas.GetValue(DataTag.AimObjID, UObjectSystem.noneID);
            }

            if (aimEnemyObjId != _enemyId)
            {
                enemy = App.game.uenemy.GetEnemy(aimEnemyObjId);
                _enemyId = enemy.isDead ? UObjectSystem.noneID : aimEnemyObjId;
                if (enemy != null)
                {
                    _bloodBar.Initialize(enemy.maxHp, result.oldHp);
                    _bloodBar.SetValue(enemy.hp);
                    needShowInfo = true;
                }
            }
            else if (enemy != null)
            {
                _bloodBar.SetValue(enemy.hp);
                _enemyId = enemy.isDead ? UObjectSystem.noneID : _enemyId;
                needShowInfo = !enemy.isDead;
            }
            else
            {
                _enemyId = UObjectSystem.noneID;
            }

            if (needShowInfo && !_isShowing)
            {
                _isShowing = true;
                _showTimer = 0f;
                _canvasGroup.DOKill(this);
                _canvasGroup.alpha = 1.0f;
            }
        }
    }
}