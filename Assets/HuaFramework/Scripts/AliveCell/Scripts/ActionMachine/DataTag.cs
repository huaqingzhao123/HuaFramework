/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/4/28 15:55:30
 */

using UnityEngine;

namespace AliveCell
{
    /// <summary>
    /// DataTag
    /// </summary>
    public enum DataTag
    {
        None = 0,

        WeaponBindPointType = 1,

        /// <summary>
        /// InjuredInfos
        /// </summary>
        InjuredInfos = 2,

        /// <summary>
        /// 攻击目标
        /// </summary>
        AimObjID = 3,

        /// <summary>
        /// 攻击数量
        /// </summary>
        AttackCountInFrame = 4,

        /// <summary>
        /// 当前帧受伤
        /// </summary>
        InjuredInFrame = 5,

        /// <summary>
        /// 瞄准自己
        /// </summary>
        AimSelfObjIDsInFrame = 6,

        /// <summary>
        /// 最近的可攻击的目标
        /// </summary>
        NearAimObjID = 7,

        /// <summary>
        /// 上一帧瞄准自己
        /// </summary>
        LastAimSelfObjIDsInFrame = 8,
    }
}