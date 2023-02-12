/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/13 22:35:29
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// PopTextStyleInfos
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(menuName = "AliveCell/PopTextStyleInfos")]
    public class PopTextStyleInfos : ScriptableObject
    {
        public List<PopTextStyleInfo> styles;
    }

    [System.Serializable]
    public class PopTextStyleInfo
    {
        public string name;
        public Single lifeTime;

        public Gradient colorRange;
        public AnimationCurve sizeRange;

        public bool useAlphaOverLifeTime;
        public AnimationCurve alphaOverLifeTime;

        public bool useSizeOverLifeTime;
        public AnimationCurve sizeOverLifeTime;

        public Vector3 velocity;
        public Vector3 gravity;
        public Single damping;
    }
}