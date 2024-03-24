/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/14 2:09:58
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
    /// AimRingDisplay
    /// </summary>
    public class AimRingDisplay : ResourceItem, IUGameComponent, ICreate, IDestroy, IUpdate
    {
        [SerializeField]
        protected Transform _ringTrans;

        [SerializeField]
        protected float _ringSpeed = 1f;

        [SerializeField]
        protected float scaleDuration = 0.1f;

        [SerializeField]
        protected float scaleStartValue = 1.2f;

        [SerializeField]
        protected DG.Tweening.Ease scaleEase;

        private ActionMachineObject targetObj;
        private ActionMachineView targetView;

        public void OnCreate()
        {
            _ringTrans.gameObject.SetActive(false);
            OnGameFollowTargetChanged(App.game.ucamera.followObj);
            App.On<TObject>(EventTypes.Game_FollowTargetChanged, OnGameFollowTargetChanged);
        }

        void IDestroy.OnDestroy()
        {
            App.Off(this);
            if (_ringTrans != null)
            {
                _ringTrans.DOKill(false);
                _ringTrans.gameObject.SetActive(false);
            }

            App.DestroyGO(this);
        }

        private void OnGameFollowTargetChanged(TObject obj)
        {
            targetObj = obj as ActionMachineObject;
            targetView = null;
        }

        private void DoAnimation()
        {
            _ringTrans.DOKill(false);
            _ringTrans.DOScale(1, scaleDuration).ChangeStartValue(scaleStartValue * Vector3.one).SetEase(scaleEase);
        }


        public void OnUpdate(float deltaTime)
        {
            UpdateAimRing(deltaTime);
        }
        private void UpdateAimRing(float deltaTime)
        {
            if (targetObj == null)
            {
                if (_ringTrans.gameObject.activeSelf)
                {
                    _ringTrans.gameObject.SetActive(false);
                }
                return;
            }
            else if (!_ringTrans.gameObject.activeSelf)
            {
                _ringTrans.gameObject.SetActive(true);
                DoAnimation();
            }

            if (targetObj != null && targetView == null)
            {
                targetView = App.game.uview.GetView<ActionMachineView>(targetObj.ID);
            }

            Vector3 position = targetView != null ? targetView.transform.position : targetObj.position;
            position.y = 0f;
            Quaternion rotation = targetView != null ? targetView.transform.rotation : targetObj.rotation;

            if (targetObj.datas.TryGetValue(DataTag.NearAimObjID, out int aimObjId))
            {
                IAssetView assetView = App.game.uview.GetView(aimObjId);
                if (assetView != null)
                {
                    Vector3 dir = assetView.transform.position - position;
                    if (dir == Vector3.zero)
                    {
                        dir = Vector3.forward;
                    }
                    rotation = Quaternion.LookRotation(dir, Vector3.up);
                    rotation.x = 0f;
                    rotation.z = 0f;
                }
            }

            _ringTrans.rotation = Quaternion.RotateTowards(_ringTrans.rotation, rotation, _ringSpeed * deltaTime);
            transform.position = position;
        }
    }
}