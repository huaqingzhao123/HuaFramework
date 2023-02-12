/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/27 12:17:39
 */

using AliveCell.Commons;
using System;
using System.Collections.Generic;

using UnityEngine;
using XMLib;
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
using Space = FPPhysics.Space;
using System.Linq;
using FPPhysics.BroadPhaseEntries;
using FPPhysics.CollisionRuleManagement;
using FPPhysics.Entities.Prefabs;
using FPPhysics.CollisionShapes.ConvexShapes;
using Ray = FPPhysics.Ray;
using Material = FPPhysics.Materials.Material;

namespace AliveCell
{
    public partial class GameWorld
    {
        public partial class Setting
        {
            [SerializeField]
            private UPhysicSystem.Setting _physic = null;

            public UPhysicSystem.Setting physic => _physic;
        }
    }

    /// <summary>
    /// UPhysicSystem
    /// </summary>
    public class UPhysicSystem : ISystem, ICreate, IDestroy, ILogicUpdate
    {
        [Serializable]
        public class PhysicMaterial
        {
            public Single kineticFriction = 1;
            public Single staticFriction = 1;
            public Single bounciness = 0;

            public static implicit operator Material(PhysicMaterial v)
            {
                return new Material(v.staticFriction, v.kineticFriction, v.bounciness);
            }
        }

        [Serializable]
        public class PlayerPhysic
        {
            public Single mass = 1m;
            public Single Length = 1.4m;
            public Single radius = 0.3m;
            public PhysicMaterial material;
        }

        [Serializable]
        public class Setting
        {
            public Single gravity = -9.81m;
            public PlayerPhysic player;
            public PhysicMaterial wall;
            public PhysicMaterial ground;
        }

        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;
        public Setting setting => GlobalSetting.gameWorld.physic;

        public Space space { get; private set; }
        public CollisionGroup obstacleGroup { get; private set; }
        public CollisionGroup rigidGroup { get; private set; }
        public CollisionGroup bodyGroup { get; private set; }
        public List<StaticMesh> staticMeshs { get; private set; }

        protected LinkedList<int> rigidbodys = new LinkedList<int>();

        public UPhysicSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UPhysic");

            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, setting.gravity, 0);

            obstacleGroup = new CollisionGroup();
            rigidGroup = new CollisionGroup();
            bodyGroup = new CollisionGroup();
            staticMeshs = new List<StaticMesh>();

            CollisionGroup.DefineCollisionRule(bodyGroup, obstacleGroup, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(bodyGroup, rigidGroup, CollisionRule.NoBroadPhase);
        }

        public void OnInitialize(List<UObject> preObjs)
        {
            InitMeshCollider();
            foreach (var obj in preObjs)
            {
                if (obj is IRigidBody rigidbodyObj)
                {
                    Regist(rigidbodyObj);
                }
            }
        }

        private void InitMeshCollider()
        {
            Transform root = world.levelController.colliderMeshRoot;
            IEnumerable<MeshFilter> filters = root.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in filters)
            {
                Material material = filter.CompareTag("Ground") ? setting.ground : setting.wall;
                Mesh mesh = filter.mesh;
                Transform meshTransform = filter.transform;

                var vertices = Array.ConvertAll(mesh.vertices, t => new Vector3((Fix64)t.x, (Fix64)t.y, (Fix64)t.z));
                var triangles = mesh.triangles;

                Vector3 position = (Vector3)meshTransform.position;
                Vector3 scale = (Vector3)meshTransform.lossyScale;
                Quaternion rotation = (Quaternion)meshTransform.rotation;

                AffineTransform transfrom = new AffineTransform(ref scale, ref rotation, ref position);
                StaticMesh staticMesh = new StaticMesh(vertices, triangles, transfrom);
                staticMesh.CollisionRules.Group = obstacleGroup;
                staticMesh.Material = material;

                staticMeshs.Add(staticMesh);
                space.Add(staticMesh);
            }
        }

        public void OnCreate()
        {
            Physics.autoSimulation = false;
            Physics.autoSyncTransforms = false;
        }

        public void OnDestroy()
        {
            Physics.autoSimulation = true;
            Physics.autoSyncTransforms = true;
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            Physics.Simulate(deltaTime);
            Physics.SyncTransforms();

            UpdateItem();

            space.Update(deltaTime);
        }

        protected void UpdateItem()
        {
            foreach (var item in world.uobj.ForeachNew<IRigidBody>())
            {
                Regist(item);
            }

            foreach (var item in world.uobj.ForeachDestroyed<IRigidBody>())
            {
                UnRegist(item);
            }
        }

        protected void Regist(IRigidBody rigid)
        {
            rigidbodys.AddLast(rigid.ID);
            rigid.OnRegistPhysic();
        }

        protected void UnRegist(IRigidBody rigid)
        {
            rigid.OnUnRegistPhysic();
            rigidbodys.Remove(rigid.ID);
        }

        #region Ray

        public bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, IList<RayCastResult> outputCastResults, LayerMask layerMask, CollisionGroup group = null)
        {
            //DrawUtility.D.PushAndSetDuration(0.5f);
            //DrawUtility.D.PushAndSetColor(Color.red);
            ShapeDrawer.DrawShape(startingTransform.Matrix, castShape, DrawUtility.D);
            //DrawUtility.D.PopColor();
            //DrawUtility.D.PopDuration();

            return space.ConvexCast(castShape, ref startingTransform, ref sweep, (target) => OnFilter(target, layerMask, group), outputCastResults);
        }

        public bool RayCast(Ray ray, Single maximumLength, Func<BroadPhaseEntry, bool> filter, out RayCastResult result)
        {
            return space.RayCast(ray, maximumLength, filter, out result);
        }

        private bool OnFilter(BroadPhaseEntry target, LayerMask layerMask, CollisionGroup group)
        {
            RigidBodyObject obj = target.Tag as RigidBodyObject;
            if (obj == null || (group != null && group != target.CollisionRules.Group))
            {
                return false;
            }
            return layerMask.Exist(obj.layer);
        }

        #endregion Ray
    }
}