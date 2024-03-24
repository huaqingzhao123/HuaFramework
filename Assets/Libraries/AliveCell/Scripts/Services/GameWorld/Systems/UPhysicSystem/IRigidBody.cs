/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/7/19 22:17:09
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
using Material = FPPhysics.Materials.Material;
using FPPhysics.Entities.Prefabs;
using Space = FPPhysics.Space;

namespace AliveCell
{
    /// <summary>
    /// IRigidBody
    /// </summary>
    public interface IRigidBody
    {
        Vector3 velocity { get; set; }
        Vector3 position { get; set; }
        Material material { get; set; }
        Single mass { get; set; }
        Single? gravity { get; set; }
        Single gravityDefault { get; }
        Single staticFriction { get; set; }
        Single bounciness { get; set; }
        Single kineticFriction { get; set; }
        bool isKinematic { get; set; }

        int ID { get; }
        bool isDestroyed { get; set; }

        Capsule bulk { get; }

        void OnRegistPhysic();

        void OnUnRegistPhysic();
    }
}