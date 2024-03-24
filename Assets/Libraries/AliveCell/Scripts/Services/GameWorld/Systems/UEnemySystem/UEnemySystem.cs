/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/18 17:30:37
 */

using AliveCell.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// UEnemySystem
    /// </summary>
    public class UEnemySystem : ISystem, ICreate, IDestroy, ILogicUpdate
    {
        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        private LinkedList<int> ids = new LinkedList<int>();
        private HashSet<int> idHashs = new HashSet<int>();

        private Dictionary<int, Vector3> id2PlayerPos = new Dictionary<int, Vector3>();

        public EnemyObject GetEnemy(int eid) => idHashs.Contains(eid) ? world.uobj.Get<EnemyObject>(eid) : null;

        public UEnemySystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UEnemy");
        }

        public bool IsEnemy(int id) => idHashs.Contains(id);

        public void OnInitialize(List<UObject> preObjs)
        {
            foreach (var obj in preObjs)
            {
                if (obj is EnemyObject enemyObj)
                {
                    ids.AddLast(enemyObj.ID);
                    idHashs.Add(enemyObj.ID);
                }
            }
        }

        public void OnCreate()
        {
        }

        public void OnDestroy()
        {
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            UpdateItem();
            UpdateDist();
        }

        private void UpdateItem()
        {
            foreach (var item in world.uobj.ForeachNew<EnemyObject>())
            {
                ids.AddLast(item.ID);
                idHashs.Add(item.ID);
            }
            foreach (var item in world.uobj.ForeachDestroyed<EnemyObject>())
            {
                ids.Remove(item.ID);
                idHashs.Remove(item.ID);
            }
        }

        private void UpdateDist()
        {
            id2PlayerPos.Clear();
            foreach (var id in world.uplayer.uids)
            {
                id2PlayerPos[id] = world.uobj.Get<PlayerObject>(id).position;
            }

            foreach (var id in ids)
            {
                EnemyObject obj = world.uobj.Get<EnemyObject>(id);

                obj.data.pid2dist.Clear();

                int nearId = UObjectSystem.noneID;
                Single nearDist = -1;
                foreach (var p in id2PlayerPos)
                {
                    Single dist = obj.data.pid2dist[p.Key] = Vector3.Distance(obj.position, p.Value);
                    if (nearId == UObjectSystem.noneID || dist < nearDist)
                    {
                        nearId = p.Key;
                        nearDist = dist;
                    }
                }

                obj.data.nearPlayerId = nearId;
            }
        }
    }
}