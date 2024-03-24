/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/24 17:31:03
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// ParticleEffectView
    /// </summary>
    public class ParticleEffectView : EffectView
    {
        protected List<ParticleSystem> particles = new List<ParticleSystem>();

        private bool needReset = true;

        protected override void Awake()
        {
            base.Awake();

            GetComponentsInChildren<ParticleSystem>(false, particles);
        }

        public override void OnPushPool()
        {
            base.OnPushPool();

            for (int i = 0; i < particles.Count; i++)
            {
                ParticleSystem ps = particles[i];
                ps.Clear();
            }
            needReset = true;
        }

        protected override void SyncLogicUpdate(Single deltaTime)
        {
            base.SyncLogicUpdate(deltaTime);

            foreach (var item in particles)
            {
                item.Simulate(deltaTime, false, needReset, false);
            }
            needReset = false;
        }
    }
}