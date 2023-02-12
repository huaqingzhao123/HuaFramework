/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/17 13:27:11
 */

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// UIControl
    /// </summary>
    public abstract class UIControl : MonoBehaviour, IResourcePoolCallback
    {
        public UIControl parent { get; private set; } = null;
        public List<UIControl> childs { get; private set; } = new List<UIControl>();
        public List<UIAnimControl> anims { get; protected set; } = new List<UIAnimControl>();
        public List<UIAudioControl> audios { get; protected set; } = new List<UIAudioControl>();
        public int playAnimCount { get; private set; } = 0;

        public SuperLogHandler LogHandler { get; private set; } = null;

        protected virtual void Awake()
        {
            LogHandler = SuperLogHandler.Create(GetType().Name);
            UpdateParent();
        }

        public void OnTransformParentChanged()
        {
            UpdateParent();
        }

        public virtual void OnInitialize()
        {
        }

        public void UpdateParent()
        {
            //获取组件时，排除自己，否则会造成死循环
            UIControl newParent = transform.parent != null ? transform.parent.GetComponentInParent<UIControl>() : null;
            SetParent(newParent);
        }

        public void SetParent(UIControl control)
        {
            if (parent == control)
            {
                return;
            }

            if (parent != null)
            {
                parent.RemoveChild(this);
            }
            parent = control;
            if (parent != null)
            {
                parent.AddChild(this);
            }
        }

        public void AddChild(UIControl child)
        {
            childs.Add(child);
            child.OnInitialize();
        }

        public void RemoveChild(UIControl child)
        {
            childs.Remove(child);
        }

        #region tween

        public bool isAnimPlaying
        {
            get => playAnimCount != 0;
            set
            {
                playAnimCount += (value ? 1 : -1);
                LogHandler.Assert(playAnimCount >= 0, $"操作异常，值({playAnimCount})不应小于0");
            }
        }

        public bool isAllAnimCompleted
        {
            get
            {
                if (isAnimPlaying)
                {
                    return false;
                }

                foreach (var child in childs)
                {
                    if (!child.isAllAnimCompleted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void AppendAnim(UIAnimControl anim)
        {
            anims.Add(anim);
            anim.OnInitialize();
        }

        public void RemoveAnim(UIAnimControl anim)
        {
            anims.Remove(anim);
        }

        public virtual void CompleteAllTween()
        {
            foreach (var child in childs)
            {
                child.CompleteAllTween();
            }
            DOTween.Kill(this, true);
        }

        protected Sequence BeginTween()
        {
            return DOTween.Sequence();
        }

        protected void EndTween(Sequence seq)
        {
            isAnimPlaying = true;
            seq.onComplete += () => isAnimPlaying = false;
            seq
                .SetId(this)
                .Play();
        }

        #endregion tween

        #region audio

        public void AppendAudio(UIAudioControl audio)
        {
            audios.Add(audio);
            audio.OnInitialize();
        }

        public void RemoveAudio(UIAudioControl audio)
        {
            audios.Remove(audio);
        }

        #endregion audio

        public virtual void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            foreach (var child in childs)
            {
                child.OnStateChange(status, enterOrExit);
            }

            if (anims.Count > 0)
            {
                Sequence seq = BeginTween();
                foreach (var item in anims)
                {
                    Tween tween = item.GetTween(status, enterOrExit);
                    if (tween != null)
                    {
                        seq.Join(tween);
                    }
                }
                EndTween(seq);
            }

            if (audios.Count > 0)
            {
                foreach (var audio in audios)
                {
                    audio.OnStateChange(status, enterOrExit);
                }
            }
        }

        public virtual void OnUpdate()
        {
            foreach (var child in childs)
            {
                child.OnUpdate();
            }
        }

        public void OnPushPool()
        {
        }

        public void OnPopPool()
        {
        }
    }
}