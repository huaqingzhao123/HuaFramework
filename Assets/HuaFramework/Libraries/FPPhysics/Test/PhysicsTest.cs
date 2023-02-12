using FPPhysics;
using System.Collections.Generic;
using UnityEngine;
using FPPhysics.BroadPhaseEntries;
using System;
using FPPhysics.CollisionShapes;
using FPPhysics.CollisionShapes.ConvexShapes;
using FPPhysics.Entities.Prefabs;
using FPPhysics.CollisionRuleManagement;
using AliveCell;
using UnityEngine.InputSystem;
using FPPhysics.Constraints.TwoEntity.Joints;

using Space = FPPhysics.Space;
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
using XMLib;

public class PhysicsTest : MonoBehaviour
{
    private Space _space;

    public Transform groundRoot;

    public Transform checker;

    private CollisionGroup groundGroup = new CollisionGroup();
    private CollisionGroup itemGroup = new CollisionGroup();
    private CollisionGroup bodyGroup = new CollisionGroup();

    private Capsule player;
    private CompoundBody playerBody;

    private void Awake()
    {
        _space = new Space();
        _space.ForceUpdater.Gravity = new Vector3(0, -9.81m, 0);

        CollisionGroup.DefineCollisionRule(bodyGroup, itemGroup, CollisionRule.NoBroadPhase);
        CollisionGroup.DefineCollisionRule(bodyGroup, groundGroup, CollisionRule.NoBroadPhase);

        InitGroundCollision();

        player = new Capsule((Vector3)checker.position, 2, 0.5m, 4);
        player.Tag = new object();

        player.CollisionInformation.CollisionRules.Group = itemGroup;
        player.LocalInertiaTensorInverse = new Matrix3x3();

        playerBody = new CompoundBody(new List<CompoundShapeEntry>() {
         new CompoundShapeEntry(new BoxShape(3,3,3))
        }, Fix64.C0p001);
        playerBody.CollisionInformation.CollisionRules.Group = bodyGroup;
        playerBody.CollisionInformation.CollisionRules.Personal = CollisionRule.NoSolver;
        playerBody.Position = player.Position;

        var joint = new BallSocketJoint(playerBody, player, player.Position) { };
        _space.Add(player);
        _space.Add(playerBody);
        _space.Add(joint);
    }

    private void InitGroundCollision()
    {
        MeshFilter[] meshFilters = groundRoot.GetComponentsInChildren<MeshFilter>();
        foreach (var filter in meshFilters)
        {
            var vertices = Array.ConvertAll(filter.sharedMesh.vertices, t => new Vector3((Fix64)t.x, (Fix64)t.y, (Fix64)t.z));
            var triangles = filter.sharedMesh.triangles;

            Vector3 position = (Vector3)filter.transform.position;
            Vector3 scale = (Vector3)filter.transform.lossyScale;
            Quaternion rotation = (Quaternion)filter.transform.rotation;

            AffineTransform transfrom = new AffineTransform(ref scale, ref rotation, ref position);
            StaticMesh staticMesh = new StaticMesh(vertices, triangles, transfrom);
            staticMesh.CollisionRules.Group = groundGroup;
            _space.Add(staticMesh);
        }
    }

    private void Update()
    {
        _space.Update((Fix64)Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            var box = new Box(new Vector3(0, 5m, 0), 1, 1, 1, 2);
            box.CollisionInformation.CollisionRules.Group = itemGroup;
            var a = Vector3.up;
            box.ApplyLinearImpulse(ref a);
            _space.Add(box);
        }

        int h = Keyboard.current.aKey.IsPressed() ? -1 : Keyboard.current.dKey.IsPressed() ? 1 : 0;
        int v = Keyboard.current.sKey.IsPressed() ? -1 : Keyboard.current.wKey.IsPressed() ? 1 : 0;
        int r = Keyboard.current.qKey.IsPressed() ? -1 : Keyboard.current.eKey.IsPressed() ? 1 : 0;

        if (h != 0 || v != 0)
        {
            Vector3 velocity = new Vector3(h * 4, player.LinearVelocity.y, v * 4);
            player.LinearVelocity = velocity;
        }

        if (r != 0)
        {
            playerBody.Orientation *= Quaternion.CreateFromAxisAngle(Vector3.up, r * 10 * (Single)Time.deltaTime);
        }
    }

    public void UpdateCheck(Vector3 pos, Vector3 dir)
    {
        var ray = new FPPhysics.Ray(pos, dir);
        bool isCast = _space.RayCast(ray, (b) =>
        {
            return b.Tag != null;
        }, out var result);

        DrawUtility.G.DrawLine(ray.Position, ray.Position + ray.Direction * 100, isCast ? Color.red : Color.green);

        if (isCast)
        {
            DrawUtility.G.DrawLine(result.HitData.Location, (result.HitData.Location + result.HitData.Normal) * 2, Color.blue);
        }
    }

    private void OnDrawGizmos()
    {
        if (_space == null)
        {
            return;
        }

        ShapeDrawer.DrawSpace(_space, DrawUtility.G);

        UpdateCheck((Vector3)checker.position, (Vector3)checker.forward);
    }
}