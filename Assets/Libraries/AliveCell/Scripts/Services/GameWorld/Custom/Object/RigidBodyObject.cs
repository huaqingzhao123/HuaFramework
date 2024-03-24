/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/5/20 20:44:10
 */

using System;
using UnityEngine;
using XMLib.Extensions;

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
using FPPhysics.EntityStateManagement;
using Space = FPPhysics.Space;

namespace AliveCell
{
    public struct RigidBodyInitData
    {
        public Vector3 Position;
        public Quaternion Orientation;
        public UPhysicSystem.PlayerPhysic physic;
    }

    /// <summary>
    /// RigidBodyObject
    /// </summary>
    public abstract class RigidBodyObject : TObject, IRigidBody
    {
        #region setting

        public int layer { get; set; }
        public override Matrix4x4 localToWorldMatrix => bulk.WorldTransform;
        public override Vector3 position { get => bulk.Position; set => bulk.Position = value; }
        public override Quaternion rotation { get => bulk.Orientation; set => bulk.Orientation = value; }
        public Material material { get => bulk.Material; set => bulk.Material = value; }

        public Single mass
        {
            get => bulk.Mass;
            set
            {
                bulk.Mass = value;
                bulk.LocalInertiaTensorInverse = new Matrix3x3();
            }
        }

        public Single? gravity
        {//纵向
            get => bulk.Gravity?.y;
            set
            {
                bulk.Gravity = value.HasValue ? new Vector3(0, value.Value, 0) : (Vector3?)null;
            }
        }

        public Single gravityDefault => space.ForceUpdater.Gravity.y;

        public Vector3 velocity
        {
            get => bulk.LinearVelocity;
            set
            {
                bulk.LinearVelocity = value;
            }
        }

        public bool isKinematic
        {
            get => !bulk.IsDynamic;
            set
            {
                if (value)
                {
                    bulk.BecomeKinematic();
                    bulk.LinearVelocity = Vector3.zero;
                }
                else
                {
                    bulk.BecomeDynamic(initData.physic.mass);
                    bulk.LocalInertiaTensorInverse = new Matrix3x3();
                }
            }
        }

        public Single kineticFriction
        {
            get => material.KineticFriction;
            set
            {
                Material material = this.material;
                material.KineticFriction = value;
                this.material = material;
            }
        }

        public Single staticFriction
        {
            get => material.StaticFriction;
            set
            {
                Material material = this.material;
                material.StaticFriction = value;
                this.material = material;
            }
        }

        public Single bounciness
        {
            get => material.Bounciness;
            set
            {
                Material material = this.material;
                material.Bounciness = value;
                this.material = material;
            }
        }

        #endregion setting

        public Space space => world.uphysic.space;
        public UPhysicSystem uphysic => world.uphysic;

        public RigidBodyInitData initData;
        public Capsule bulk { get; protected set; }

        public override void OnInitialized()
        {
            base.OnInitialized();

            bulk = new Capsule(initData.Position, initData.physic.Length, initData.physic.radius, initData.physic.mass);
            bulk.Orientation = initData.Orientation;
            bulk.CollisionInformation.LocalPosition = new Vector3(0, initData.physic.Length / 2 + initData.physic.radius, 0);
            bulk.CollisionInformation.CollisionRules.Group = world.uphysic.rigidGroup;
            bulk.LocalInertiaTensorInverse = new Matrix3x3();
            bulk.Material = initData.physic.material ?? bulk.Material;
            bulk.Tag = this;
            bulk.CollisionInformation.Tag = this;
        }

        public virtual void OnRegistPhysic()
        {
            space.Add(bulk);
        }

        public virtual void OnUnRegistPhysic()
        {
            space.Remove(bulk);
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
        }

        public override void OnReset()
        {
            base.OnReset();
            initData = default;
        }

        public override string GetMessage()
        {
            string message = base.GetMessage();

            message += "----RigidBodyObject----\n" +
                string.Format("velocity:\t({0:N6},{1:N6},{2:N6})\n", velocity.x, velocity.y, velocity.z) +
                $"gravity:\t{gravity}\n" +
                $"mass:\t{mass}\n" +
                $"kineticFriction:\t{kineticFriction}\n" +
                $"staticFriction:\t{staticFriction}\n" +
                $"bounciness:\t{bounciness}\n" +
                $"isKinematic:\t{isKinematic}\n";

            return message;
        }
    }
}