/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/20 14:33:01
 */

using System;
using UnityEngine;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// TObject
    /// </summary>
    public class TObject : UObject, IAssetObject
    {
        public virtual Vector3 position { get => _position; set => _position = value; }

        public virtual Quaternion rotation { get => _rotation; set => _rotation = value; }

        public virtual Vector3 scale { get => _scale; set => _scale = value; }

        public virtual Matrix4x4 localToWorldMatrix => Matrix4x4.TRS(_position, _rotation, _scale);

        public int prefabID { get; set; }

        public Single softRotateScale { get; set; } = 1;

        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;

        public virtual void OnViewBind()
        {
        }

        public virtual void OnViewUnbind()
        {
        }

        public override string GetMessage()
        {
            string message = base.GetMessage();
            message += "----T----\n";
            message += $"prefabID:\t{prefabID}\n";
            message += $"softRotateScale:\t{softRotateScale}\n";
            message += string.Format("position:\t({0:N6},{1:N6},{2:N6})\n", position.x, position.y, position.z);
            message += $"rotation:\t{rotation.eulerAngles}\n";
            message += $"scale:\t{scale}\n";
            return message;
        }

        public override void OnReset()
        {
            base.OnReset();

            _position = Vector3.zero;
            _rotation = Quaternion.identity;
            _scale = Vector3.one;
            softRotateScale = 1;

            prefabID = 0;
        }
    }
}