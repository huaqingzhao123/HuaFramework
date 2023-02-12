using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// BloodEffectView
    /// </summary>
    public class BloodEffectView : EffectView
    {
        //protected List<UVTextureAnimator> animators = new List<UVTextureAnimator>();

        protected override void Awake()
        {
            base.Awake();

            //GetComponentsInChildren<UVTextureAnimator>(false, animators);
        }

        public override void OnPushPool()
        {
            base.OnPushPool();
        }

        public override void UpdateView(float deltaTime)
        {
            base.UpdateView(deltaTime);
        }

        protected override void SyncLogicUpdate(Single deltaTime)
        {
            base.SyncLogicUpdate(deltaTime);

            // foreach (var item in animators)
            // {
            //     item.ManualUpdate(deltaTime);
            // }
        }
    }
}