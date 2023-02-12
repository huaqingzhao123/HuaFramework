/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/26 18:04:45
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// CameraShakeSetting
    /// </summary>
    [CreateAssetMenu(menuName = "AliveCell/CameraShakeSetting")]
    [System.Serializable]
    public class CameraShakeSetting : ScriptableObject
    {
        [SerializeField]
        public List<CameraShakeInfo> shakeInfos;
    }

    [Serializable]
    public class CameraShakeInfo
    {
        public string name;
        public Vector3 strength;
        public float duration;
        public int vibrato;
        public float elasticity;
        public DG.Tweening.Ease curve;
        public bool isPunch;
        public bool correctDirection;
        public string hapticName;

        public override string ToString()
        {
            return $"Shake({name}):strength={strength}, duration={duration}, isPunch={isPunch}, hapticName={hapticName}";
        }
    }
}