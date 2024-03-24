/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/8 10:41:07
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace AliveCell
{
    /// <summary>
    /// UIAudio
    /// </summary>
    public class UIAudioControl : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IResourcePoolCallback
    {
        [Flags]
        public enum PlayType
        {
            Panel_Enter = 0b0000_0001,
            Panel_Exit = 0b0000_0010,
            Panel_Pause = 0b0000_0100,
            Panel_Resume = 0b0000_1000,

            Pointer_Click = 0b0001_0000,
            Pointer_Down = 0b0010_0000,
            Pointer_Up = 0b0100_0000,
        }

        [Serializable]
        public class Data
        {
            public PlayType type;
            public int id;
        }

        [SerializeField]
        protected List<Data> audios;

        public UIControl parent { get; protected set; }

        protected virtual void Awake()
        {
            UpdateParent();
        }

        public void Play(PlayType type)
        {
            foreach (var audio in audios)
            {
                if ((audio.type & type) == 0)
                {
                    continue;
                }

                //App.audio.Play(audio.id);
            }
        }

        public void OnInitialize()
        {
        }

        public void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    Play(PlayType.Panel_Enter);
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    Play(PlayType.Panel_Exit);
                    break;

                case UIPanelStatus.Pause when enterOrExit:
                    Play(PlayType.Panel_Pause);
                    break;

                case UIPanelStatus.Resume when enterOrExit:
                    Play(PlayType.Panel_Resume);
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Play(PlayType.Pointer_Click);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Play(PlayType.Pointer_Down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Play(PlayType.Pointer_Up);
        }

        public void SetParent(UIControl control)
        {
            if (control == parent)
            {
                return;
            }

            if (parent != null)
            {
                parent.RemoveAudio(this);
                parent = null;
            }
            parent = control;
            if (parent != null)
            {
                parent.AppendAudio(this);
            }
        }

        public void UpdateParent()
        {
            UIControl newParent = transform.GetComponentInParent<UIControl>();
            SetParent(newParent);
        }

        public void OnTransformParentChanged()
        {
            UpdateParent();
        }

        public void OnPushPool()
        {
        }

        public void OnPopPool()
        {
        }
    }
}