/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/6 16:06:29
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.AM;

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
using FPPhysics.Entities.Prefabs;
using FPPhysics.BroadPhaseEntries.MobileCollidables;
using FPPhysics.Entities;
using XMLib.AM.Ranges;
using FPPhysics.CollisionRuleManagement;
using FPPhysics.Constraints.TwoEntity.Joints;
using Joint = FPPhysics.Constraints.TwoEntity.Joints.Joint;
using Ray = FPPhysics.Ray;

namespace AliveCell
{
    /// <summary>
    /// ActionMachineObject
    /// </summary>
    public abstract class ActionMachineObject : RigidBodyObject, IUObjectLogicUpdate, IActionController
    {
        public virtual string configName { get; set; }
        public IActionMachine machine { get; private set; }
        public LayerMask attackMask { get; set; }
        public int groupID { get; set; }

        public DataDictionary<DataTag, object> datas { get; private set; } = new DataDictionary<DataTag, object>();

        public abstract int maxHp { get; }
        public abstract int maxPower { get; }
        public abstract int hp { get; }
        public abstract int power { get; }
        public virtual bool enableTrail { get; set; }

        public float initAnimTime { get; set; }

        public bool isDead => hp <= 0;

        public override bool isDebug { get => base.isDebug; set { base.isDebug = value; if (machine != null) { machine.isDebug = value; } } }

        public abstract void SetPower(int power);

        public abstract void SetHP(int hp);

        public override void OnReset()
        {
            base.OnReset();

            configName = null;
            machine = null;
            attackMask = default;
            groupID = 0;
            enableTrail = false;
            initAnimTime = 0;
            datas.Clear();

            isOnGround = true;
            body = null;
            _isBodyActive = false;

            frameWaitingVelocity = Vector3.zero;
            isFrameWaiting = false;
        }

        #region ActionController

        public virtual InjuredResult Injured(InjuredInfo info)
        {
            //Debug.Log($"[{ID}]受到来自于[{info.info.id}]的攻击 {info.info.attack}");
            return new InjuredResult()
            {
                id = ID,
                oldHp = hp,
            };
        }

        public virtual AttackerInfo GetAttackInfo()
        {
            AttackerInfo info = new AttackerInfo();

            info.id = ID;

            return info;
        }

        public T AppendEvent<T>() where T : UEvent, new()
        {
            return world.uevt.Append<T>(ID);
        }

        public bool CheckAir() => !isOnGround;

        public bool CheckGround() => isOnGround;

        public ActionKeyCode GetAllKey() => world.uinput.GetAllKey(ID);

        public byte GetAxis() => world.uinput.GetAxis(ID);

        public bool HasKey(ActionKeyCode keyCode, bool fullMatch = false) => world.uinput.HasKey(ID, keyCode, fullMatch);

        public InputData GetRawInput() => world.uinput.GetRawInput(ID);

        #endregion ActionController

        public override void OnPushPool()
        {
            base.OnPushPool();
            configName = null;
        }

        public override void OnInitialized()
        {
            machine = new ActionMachine() { isDebug = isDebug };
            machine.Initialize(configName, this);

            base.OnInitialized();

            CreateBody();
            UpdateBody(machine.GetStateConfig().frames[0].bodyRanges);
        }

        public override void OnDestroyed()
        {
            machine.Destroy();
            DestoryBody();
            base.OnDestroyed();
        }

        public bool CheckCombatMode()
        {
            int aimObjId = datas.GetValue(DataTag.AimObjID, UObjectSystem.noneID);
            if (aimObjId != UObjectSystem.noneID)
            {
                return true;
            }
            //bool isInjuredInFrame = datas.GetValue(DataTag.InjuredInFrame, false);
            //if (isInjuredInFrame)
            //{
            //    return true;
            //}

            int attackCount = datas.GetValue(DataTag.AttackCountInFrame, 0);
            if (attackCount > 0)
            {
                return true;
            }
            List<int> aimSelfObjIDs = datas.GetValue<List<int>>(DataTag.AimSelfObjIDsInFrame, null);
            if (aimSelfObjIDs != null && aimSelfObjIDs.Count > 0)
            {
                return true;
            }
            List<int> lastAimSelfObjIDs = datas.GetValue<List<int>>(DataTag.LastAimSelfObjIDsInFrame, null);
            if (lastAimSelfObjIDs != null && lastAimSelfObjIDs.Count > 0)
            {
                return true;
            }

            return false;
        }

        public virtual void OnLogicUpdate(Single deltaTime)
        {
            OnLogicBegin();

            //执行
            machine.LogicUpdate(deltaTime);

            OnLogicEnd();
        }

        public virtual void OnLogicBegin()
        {
            //执行前清理
            datas[DataTag.AttackCountInFrame] = 0;

            List<int> aimSelfs = datas.GetOrCreateValue<List<int>>(DataTag.AimSelfObjIDsInFrame);
            List<int> lastAimSelfs = datas.GetOrCreateValue<List<int>>(DataTag.LastAimSelfObjIDsInFrame);
            lastAimSelfs.Clear();
            lastAimSelfs.AddRange(aimSelfs);
            aimSelfs.Clear();

            if (machine.waitFrameCnt > 0)
            {
                if (!isKinematic)
                {
                    frameWaitingVelocity = velocity;
                    velocity = Vector3.zero;
                    isKinematic = true;
                    isFrameWaiting = true;
                }
            }
        }

        public virtual void OnLogicEnd()
        {
            //更新身体范围
            UpdateBody(machine.GetBodyRanges());
            UpdateGround();

            if (isFrameWaiting && machine.waitFrameCnt == 0)
            {
                isFrameWaiting = false;
                isKinematic = false;
                velocity = frameWaitingVelocity;
            }
        }

        #region frame wait

        protected Vector3 frameWaitingVelocity;
        protected bool isFrameWaiting;

        #endregion frame wait

        #region body

        protected bool _isBodyActive;

        public Box body { get; protected set; }
        public bool isOnGround { get; protected set; }

        public bool isBodyActive
        {
            get => _isBodyActive;
            set
            {
                if (_isBodyActive == value)
                {
                    return;
                }
                _isBodyActive = value;

                if (_isBodyActive)
                {
                    body.Position = bulk.Position;
                    body.Orientation = bulk.Orientation;
                    space.Add(body);
                }
                else
                {
                    space.Remove(body);
                }
            }
        }

        private void UpdateGround()
        {
            if (bulk.LinearVelocity.y > Fix64.C0p15)
            {
                isOnGround = false;
                return;
            }

            isOnGround = uphysic.RayCast(new Ray(bulk.Position + Vector3.up * Fix64.C0p15, Vector3.down), Fix64.C0p15 * 2,
                t => t.CollisionRules.Group == uphysic.obstacleGroup, out RayCastResult result);
            DrawUtility.D.DrawLine(bulk.Position, bulk.Position + Vector3.down * Fix64.C1);
        }

        private void CreateBody()
        {
            body = new Box(bulk.Position, bulk.Radius, bulk.Length + 2 * bulk.Radius, bulk.Radius);
            body.CollisionInformation.CollisionRules.Group = uphysic.bodyGroup;
            body.CollisionInformation.CollisionRules.Personal = CollisionRule.NoSolver;
            body.CollisionInformation.LocalPosition = new Vector3(0, body.HalfHeight, 0);
            body.Orientation = bulk.Orientation;
            body.Tag = this;
            body.CollisionInformation.Tag = this;
            //space.Add(_body);
        }

        private void DestoryBody()
        {
            isBodyActive = false;
        }

        private void UpdateBody(List<RangeConfig> bodyRanges)
        {
            isBodyActive = bodyRanges != null ? bodyRanges.Count > 0 : false;
            if (isBodyActive)
            {
                foreach (var body in bodyRanges)
                {
                    switch (body.value)
                    {
                        case BoxItem v:
                            UpdateBoxBody(v);
                            break;
                    }
                }
            }

            if (isBodyActive)
            {//更新位置
                body.Position = bulk.Position;
                body.Orientation = bulk.Orientation;
            }
        }

        private void UpdateBoxBody(BoxItem v)
        {
            body.CollisionInformation.Shape.Resize((Single)v.size.x, (Single)v.size.z, (Single)v.size.y);
            body.CollisionInformation.LocalPosition = new Vector3((Single)v.offset.x, (Single)v.offset.y, (Single)v.offset.z);
        }

        #endregion body

        public override string GetMessage()
        {
            string message = base.GetMessage();

            message += "----ActionMachine----\n" +
                $"BFI/SLC/SFI:\t{machine.stateBeginFrameIndex}/{machine.GetStateLoopCnt()}/{machine.GetStateFrameIndex()}\n" +
                //$"BeginFrameIndex/StateLoopCnt/StateFrameIndex:\t{machine.stateBeginFrameIndex}/{machine.GetStateLoopCnt()}/{machine.GetStateFrameIndex()}\n" +
                $"stateName:\t{machine.stateName}\n" +
                $"frameIndex:\t{machine.frameIndex}\n" +
                $"CheckGround:\t{CheckGround()}\n" +
                $"CheckAir:\t{CheckAir()}\n" +
                $"animIndex:\t{machine.animIndex}\n" +
                $"GetAnimName:\t{machine.GetAnimName()}\n" +
                $"eventTypes:\t{machine.eventTypes}\n" +
                $"CombatMode:{CheckCombatMode()}\n";

            string dataStr = string.Empty;
            foreach (var item in datas)
            {
                dataStr += $"\t{item.Key}\t=\t{item.Value}\n";
            }
            message += "Datas:\n";
            message += dataStr;

            return message;
        }
    }
}