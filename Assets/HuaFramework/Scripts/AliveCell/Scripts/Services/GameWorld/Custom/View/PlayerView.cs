/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/13 15:45:58
 */

using System;
using TrailsFX;
using UnityEngine;

using Single = FPPhysics.Fix64;
namespace AliveCell
{
    /// <summary>
    /// PlayerView
    /// </summary>
    public class PlayerView : ActionMachineView
    {
        private PlayerObject obj = null;

        [SerializeField]
        private TrailEffect trailEffect = null;

        public override void OnViewBind()
        {
            base.OnViewBind();
            obj = GetObj<PlayerObject>();
        }

        public override void OnViewUnbind()
        {
            base.OnViewUnbind();
            obj = null;
        }

        protected override void SyncLogicUpdate(Single deltaTime)
        {
            base.SyncLogicUpdate(deltaTime);

            //if (trailEffect != null)
            //{
            //    trailEffect.UpdateTrail(deltaTime);
            //}
        }

        public override void LogicUpdateView(Single deltaTime)
        {
            base.LogicUpdateView(deltaTime);

            if (trailEffect != null)
            {
                trailEffect.active = obj.enableTrail;
            }
        }

        public override void UpdateView(float deltaTime)
        {
            base.UpdateView(deltaTime);
        }
    }
}