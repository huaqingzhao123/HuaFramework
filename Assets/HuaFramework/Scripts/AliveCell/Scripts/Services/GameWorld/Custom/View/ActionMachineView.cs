/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/6 16:44:43
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using XMLib.AM;

using Single = FPPhysics.Fix64;

namespace AliveCell
{
    public enum BindPointType
    {
        None = 0,
        Weapon,
        Back,
        pit,
        Arrow
    }

    [System.Serializable]
    public class BindPoint
    {
        public BindPointType type;
        public Transform target;
    }

    /// <summary>
    /// ActionMachineView
    /// </summary>
    public class ActionMachineView : RigidBodyView
    {
        [SerializeField]
        private Animator _animator = null;

        [SerializeField]
        private List<BindPoint> bindPoints = null;

        public Animator animator => _animator;

        private Dictionary<BindPointType, Transform> name2bind;

        [SerializeField]
        protected List<Renderer> renderers;

        [SerializeField]
        protected Color rimColor = new Color(1, 0, 0, 0.3f);

        [SerializeField]
        protected float rimTime = 0.3f;

        protected float _rimTimer = 0f;

        private ActionMachineObject obj = null;

        public override bool canSyncLogic => (obj.machine.eventTypes & ActionMachineEvent.FrameChanged) != 0;

        protected override void Awake()
        {
            base.Awake();

            name2bind = new Dictionary<BindPointType, Transform>();
            foreach (var point in bindPoints)
            {
                name2bind.Add(point.type, point.target);
            }
        }

        public Transform FindBind(BindPointType type)
        {
            Transform bind = name2bind.TryGetValue(type, out Transform target) ? target : null;
            return bind ?? transform;
        }

        public override void OnPushPool()
        {
            base.OnPushPool();

            _animator.enabled = false;
            _rimTimer = 0f;
            SetRimColor(Color.clear);
        }

        public override void OnViewBind()
        {
            base.OnViewBind();
            obj = GetObj<ActionMachineObject>();

            InitAnimation();
        }

        private void InitAnimation()
        {
            if (animator == null)
            {
                return;
            }

            string animName = obj.machine.GetAnimName();
            animator.Play(animName, 0, obj.initAnimTime);
            animator.Update(0);
        }

        public override void OnViewUnbind()
        {
            base.OnViewUnbind();
            obj = null;
        }

        protected override void SyncLogicUpdate(Single deltaTime)
        {
            base.SyncLogicUpdate(deltaTime);
            UpdateRim(deltaTime);
            UpdateAnimation(deltaTime);
        }

        private void UpdateAnimation(Single deltaTime)
        {
            if (animator != null)
            {
                animator.Update(deltaTime);
            }
        }

        public override void LogicUpdateView(Single deltaTime)
        {
            base.LogicUpdateView(deltaTime);

            ActionMachineEvent eventTypes = obj.machine.eventTypes;
            if ((eventTypes & ActionMachineEvent.FrameChanged) != 0)
            {
                if (obj.datas.TryGetValue(DataTag.InjuredInFrame, out bool isInjured) && isInjured)
                {
                    PlayRim();
                }
            }

            if (animator != null && (eventTypes & ActionMachineEvent.AnimChanged) != 0)
            {
                StateConfig config = obj.machine.GetStateConfig();

                float fixedTimeOffset = obj.machine.animStartTime;
                float fadeTime = config.fadeTime;
                string animName = obj.machine.GetAnimName();

                if ((eventTypes & ActionMachineEvent.HoldAnimDuration) != 0)
                {
                    fixedTimeOffset = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                }

                animator.CrossFadeInFixedTime(animName, fadeTime, 0, fixedTimeOffset);
                animator.Update(0);
            }
        }

        protected void PlayRim()
        {
            _rimTimer = rimTime;
        }

        private void UpdateRim(Single deltaTime)
        {
            if (_rimTimer == 0)
            {
                return;
            }

            _rimTimer = Mathf.Max(0, _rimTimer - deltaTime);
            float alpha = Mathf.Clamp01(_rimTimer / rimTime) * rimColor.a;

            Color color = new Color(rimColor.r, rimColor.g, rimColor.b, alpha);
            SetRimColor(color);
        }

        private void SetRimColor(Color color)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor(ShaderIDs.AimColor, color);

            foreach (var item in renderers)
            {
                item.SetPropertyBlock(block);
            }
        }

        public static class ShaderIDs
        {
            public readonly static int AimColor = Shader.PropertyToID("_RimColor");
        }

#if UNITY_EDITOR

        protected virtual void OnValidate()
        {
            if (renderers == null || renderers.Count == 0)
            {
                GetComponentsInChildren<Renderer>(renderers);
            }
        }

#endif
    }
}