/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/26 19:04:40
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// CameraFollowSetting
    /// </summary>
    [CreateAssetMenu(menuName = "AliveCell/CameraFollowSetting")]
    [Serializable]
    public class CameraFollowSetting : ScriptableObject
    {
        public List<CameraFollowInfo> followInfos;
        public CameraStates.LookUpDownSetting lookUpDown;
    }

    [Serializable]
    public class CameraFollowInfo
    {
        public string name;
        public ViewData view;
    }

    [Serializable]
    public struct ViewData
    {
        public float distance;
        public float angle;
        public float cameraAngle;
        public float height;

        public static ViewData Lerp(in ViewData from, in ViewData to, float value)
        {
            ViewData result;

            result.distance = Mathf.Lerp(from.distance, to.distance, value);
            result.height = Mathf.Lerp(from.height, to.height, value);
            result.cameraAngle = Mathf.LerpAngle(from.cameraAngle, to.cameraAngle, value);
            result.angle = Mathf.LerpAngle(from.angle, to.angle, value);

            return result;
        }
    }
}