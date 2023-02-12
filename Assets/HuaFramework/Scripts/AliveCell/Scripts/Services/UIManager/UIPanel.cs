/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/8 10:44:49
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace AliveCell
{
    public enum UIPanelStatus
    {
        None,
        Enter,
        Exit,
        Pause,
        Resume,
    }

    /// <summary>
    /// UIPanel
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : UIControl, IDisposable
    {
        public UIManager manager { get; private set; }
        public virtual string panelName => GetType().Name;
        public virtual int sortWeight { get; } = 0;
        public UIPanelStatus status { get; protected set; }
        public float enterTime { get; protected set; } = 0f;
        public CanvasGroup canvasGroup => _canvasGroup;
        public bool enableSafeArea => _enableSafeArea;

        public virtual bool isRunning => !isAllAnimCompleted || (status != UIPanelStatus.None && status != UIPanelStatus.Exit);

        [SerializeField]
        protected CanvasGroup _canvasGroup;

        [SerializeField]
        protected bool _enableSafeArea;

        public void Initialize(UIManager manager)
        {
            this.manager = manager;
            UpdateSafeArea();
            OnCreateSubControl();//在获取子控件前，创建子控件到节点下

            OnInitialize();
        }

        protected void UpdateSafeArea()
        {
            if (!_enableSafeArea)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            RectTransform trans = transform as RectTransform;

            Vector2 max = safeArea.max;
            Vector2 min = safeArea.min;

            max.x /= Screen.width;
            //max.y /= Screen.height;
            max.y = 1f;
            min.x /= Screen.width;
            //min.y /= Screen.height;
            min.y = 0f;

            trans.anchorMax = max;
            trans.anchorMin = min;
        }

        protected virtual void OnCreateSubControl()
        {
        }

        public void ChangeState(UIPanelStatus status)
        {
            //LogHandler.Log($"ChangeState {this.status}=>{status}");
            CompleteAllTween();
            OnStateChange(this.status, false);
            this.status = status;
            OnStateChange(this.status, true);
        }

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);
            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;
                    break;

                case UIPanelStatus.Pause when enterOrExit:
                    canvasGroup.interactable = false;
                    break;

                case UIPanelStatus.Resume when enterOrExit:
                    canvasGroup.interactable = true;
                    break;
            }
        }

        public virtual void Dispose()
        {
        }

#if UNITY_EDITOR

        protected virtual void OnValidate()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

#endif
    }
}