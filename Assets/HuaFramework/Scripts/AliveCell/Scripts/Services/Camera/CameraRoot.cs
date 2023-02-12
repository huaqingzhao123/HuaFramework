/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/11/10 16:04:00
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// CameraRoot
    /// </summary>
    public class CameraRoot : ResourceItem
    {
        [SerializeField] protected Camera _camera;
        [SerializeField] protected AudioListener _listener;
        [SerializeField] protected PostProcessVolume _postProcess;

        public new Camera camera => _camera;
        public AudioListener listener => _listener;
        public PostProcessVolume postProcess => _postProcess;

        public Transform root => transform;
        public Transform cameraRoot => _camera.transform;
    }
}