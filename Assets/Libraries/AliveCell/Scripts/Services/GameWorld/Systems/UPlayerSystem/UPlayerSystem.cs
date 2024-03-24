/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/20 10:37:11
 */

using AliveCell.Commons;
using System;
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
    /// UPlayerSystem
    /// </summary>
    public class UPlayerSystem : ISystem, ICreate, IDestroy, ILogicUpdate
    {
        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        public string selfPid => world.selfPlayerId;

        public int selfUid => Pid2Uid(selfPid);

        private Dictionary<string, int> pid2uid = new Dictionary<string, int>();
        private Dictionary<int, string> uid2pid = new Dictionary<int, string>();
        public LinkedList<int> uids = new LinkedList<int>();

        public int Pid2Uid(string pid) => pid2uid.TryGetValue(pid, out int uid) ? uid : UObjectSystem.noneID;

        public string Uid2Pid(int uid) => uid2pid.TryGetValue(uid, out string pid) ? pid : null;

        public PlayerObject GetSelfPlayer() => GetPlayer(selfPid);

        public PlayerObject GetPlayer(string pid) => pid2uid.TryGetValue(pid, out int uid) ? world.uobj.Get<PlayerObject>(uid) : null;

        public PlayerObject GetPlayer(int uid) => uid2pid.ContainsKey(uid) ? world.uobj.Get<PlayerObject>(uid) : null;

        public bool IsSelfPlayer(int uid) => uid2pid.TryGetValue(uid, out string pid) && string.Compare(selfPid, pid) == 0;

        public bool IsSelfPlayer(string pid) => pid2uid.ContainsKey(pid) && string.Compare(selfPid, pid) == 0;

        public bool IsPlayer(int uid) => uid2pid.ContainsKey(uid);

        public bool IsPlayer(string pid) => pid2uid.ContainsKey(pid);

        public UPlayerSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UPlayer");
        }

        public void OnInitialize(List<UObject> preObjs)
        {
            foreach (var obj in preObjs)
            {
                if (obj is PlayerObject playerObj)
                {
                    pid2uid.Add(playerObj.playerID, playerObj.ID);
                    uid2pid.Add(playerObj.ID, playerObj.playerID);
                    uids.AddLast(playerObj.ID);
                }
            }
        }

        public void OnCreate()
        {
        }

        public void OnDestroy()
        {
            App.Off(this);
        }

        public void OnLogicUpdate(Single deltaTime)
        {
            UpdateItem();
        }

        private void UpdateItem()
        {
            foreach (var item in world.uobj.ForeachNew<PlayerObject>())
            {
                pid2uid.Add(item.playerID, item.ID);
                uid2pid.Add(item.ID, item.playerID);
                uids.AddLast(item.ID);
            }

            foreach (var item in world.uobj.ForeachDestroyed<PlayerObject>())
            {
                pid2uid.Remove(item.playerID);
                uid2pid.Remove(item.ID);
                uids.Remove(item.ID);
            }
        }
    }
}