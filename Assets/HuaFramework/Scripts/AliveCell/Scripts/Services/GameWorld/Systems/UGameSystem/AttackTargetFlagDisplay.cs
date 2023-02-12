/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/14 2:09:17
 */

using AliveCell.Commons;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

using Single = FPPhysics.Fix64;


namespace AliveCell
{
    /// <summary>
    /// AttackTargetFlagDisplay
    /// </summary>
    public class AttackTargetFlagDisplay : ResourceItem, IUGameComponent, ISyncLogicUpdate, ICreate, IDestroy
    {
        private ActionMachineObject attackTargetObj;
        private ActionMachineView attackTargetView;

        [SerializeField]
        protected Transform ringTrans;

        [SerializeField]
        protected float scaleDuration = 0.1f;

        [SerializeField]
        protected float scaleStartValue = 1.2f;

        [SerializeField]
        protected DG.Tweening.Ease scaleEase;

        public void OnSyncLogicUpdate(Single deltaTime)
        {
            UpdateDisplay();
            UpdateTransform(deltaTime);
        }

        private void UpdateTransform(Single deltaTime)
        {
            if (attackTargetObj != null && attackTargetView == null)
            {
                attackTargetView = App.game.uview.GetView<ActionMachineView>(attackTargetObj.ID);
            }

            if (attackTargetObj != null && !attackTargetObj.isDead)
            {
                Vector3 position = attackTargetView != null ? attackTargetView.transform.position : attackTargetObj.position;
                position.y = 0f;
                transform.position = position;
            }
        }

        private void UpdateDisplay()
        {
            bool isActive = attackTargetObj != null && !attackTargetObj.isDead;
            if (isActive != ringTrans.gameObject.activeSelf)
            {
                ringTrans.gameObject.SetActive(isActive);
                if (isActive)
                {
                    DoAnimation();
                }
            }
        }

        private void DoAnimation()
        {
            ringTrans.DOKill(false);
            ringTrans.DOScale(1, scaleDuration).ChangeStartValue(scaleStartValue * Vector3.one).SetEase(scaleEase);
        }

        public void OnCreate()
        {
            ringTrans.gameObject.SetActive(false);

            App.On<int, int, int>(EventTypes.Game_AttackTargetChanged, OnAttackTargetChanged).SetFilter((t) =>
            {
                int id = (int)t[0];
                return App.game.ucamera.CheckFollow(id) && App.game.uplayer.IsPlayer(id);
            });
        }

        void IDestroy.OnDestroy()
        {
            attackTargetObj = null;
            attackTargetView = null;
            App.Off(this);

            App.DestroyGO(this);
        }

        private void OnAttackTargetChanged(int id, int oldAttackId, int newAttackId)
        {
            attackTargetObj = App.game.uobj.Get<ActionMachineObject>(newAttackId);
            attackTargetView = null;

            DoAnimation();
        }

        public override void OnPushPool()
        {
            base.OnPushPool();
            ringTrans.DOKill(false);
        }
    }
}