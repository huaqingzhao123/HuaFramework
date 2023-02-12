/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/13 15:45:58
 */

using UnityEngine;

namespace AliveCell
{
    /// <summary>
    /// PlayerView
    /// </summary>
    public class EnemyView : ActionMachineView
    {
        private EnemyObject obj = null;

        public override void OnViewBind()
        {
            base.OnViewBind();
            obj = GetObj<EnemyObject>();
        }

        public override void OnViewUnbind()
        {
            base.OnViewUnbind();
            obj = null;
        }
    }
}