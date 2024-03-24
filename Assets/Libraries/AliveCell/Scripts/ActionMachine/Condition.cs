/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/12/26 0:27:41
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
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    [Flags]
    public enum CheckType : byte
    {
        None = 0b0000,
        FowardWall = 0b0001,
        Ground = 0b0010,
        Air = 0b0100,

        /// <summary>
        /// 部分匹配模式
        /// </summary>
        KeyCode = 0b1000,

        /// <summary>
        /// 全部匹配模式
        /// </summary>
        KeyCodeAll = 0b0001_0000,
    }

    /// <summary>
    /// TSConditionConfig
    /// </summary>
    [ActionConfig(typeof(Condition))]
    [Serializable]
    public class ConditionConfig : HoldFrames
    {
        public string stateName;
        public int priority;

        /// <summary>
        /// 延迟调用跳转，动作最后一帧执行跳转,必须启用EnableBeginEnd，否则无效
        /// </summary>
        [EnableToggle()]
        [EnableToggleItem(nameof(enableBeginEnd))]
        public bool delayInvoke;

        /// <summary>
        /// 立即执行
        /// </summary>
        [EnableToggleItem(nameof(delayInvoke), nameof(enableBeginEnd))]
        public int forceFrameIndex;

        /// <summary>
        /// 每一帧都需要为真，才能在最后跳转
        /// </summary>
        [EnableToggleItem(nameof(delayInvoke), nameof(enableBeginEnd))]
        public bool allFrameCheck;

        [ConditionTypes]
        [SerializeReference]
        public List<Conditions.IItem> checker;

        public override string ToString()
        {
            return $"{GetType().Name} > {stateName} - {priority}";
        }
    }

    /// <summary>
    /// TSCondition
    /// </summary>
    public class Condition : IActionHandler
    {
        public void Enter(ActionNode node)
        {
            ConditionConfig config = (ConditionConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            node.data = 0;

            //校验
            if (config.delayInvoke && !config.EnableBeginEnd())
            {
                throw new RuntimeException($"使用延迟调用(DelayInvoke)，必须启用区间(EnableBeginEnd)\n{node}");
            }
            //
        }

        public void Exit(ActionNode node)
        {
            //TSConditionConfig config = (TSConditionConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            ConditionConfig config = (ConditionConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;

            if (!config.delayInvoke || !config.EnableBeginEnd())
            {
                if (!Checker(config.checker, node))
                {
                    return;
                }
            }
            else
            {
                int successCnt = (int)node.data;
                if (Checker(config.checker, node))
                {//为true时计数+1
                    node.data = ++successCnt;
                }

                if (successCnt != 0
                && ((machine.GetStateFrameIndex() == config.GetEndFrame()) //到达最后一帧
                    || (config.forceFrameIndex > 0 && config.forceFrameIndex < node.updateCnt)) //强制执行
                && (!config.allFrameCheck || successCnt == (node.updateCnt + 1)))//每一帧都必须为true,updateCnt需要+1是因为updateCnt在Update后才会递增
                {
                }
                else
                {
                    return;
                }
            }

            machine.ChangeState(config.stateName, config.priority);
        }

        public static bool Checker(List<Conditions.IItem> checkers, ActionNode node)
        {
            if (checkers == null || checkers.Count == 0)
            {
                return true;
            }

            foreach (var checker in checkers)
            {
                if (!checker.Execute(node))
                {
                    return false;
                }
            }

            return true;
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }

    namespace Conditions
    {
        #region Items

        public interface IItem
        {
            bool Execute(ActionNode node);
        }

        [Serializable]
        public class DatasetChecker : IItem
        {
            public DataTag tag;
            public CompareType compareType;

            [DatasetValueTypes]
            [SerializeReference]
            public object value;

            public bool Execute(ActionNode node)
            {
                IActionController controller = (IActionController)node.actionMachine.controller;
                object rawData = value is IPackageData pData ? pData.RawValue() : value;
                return controller.datas.TryGetValue(tag, rawData.GetType(), out object data) && CompareUtility.Compare(data, rawData, compareType);
            }
        }

        [Serializable]
        public class FrameChecker : IItem
        {
            public DataTag tag;
            public CompareType compareType;
            public int offsetValue;

            public bool Execute(ActionNode node)
            {
                IActionMachine machine = node.actionMachine;
                IActionController controller = (IActionController)node.actionMachine.controller;
                return controller.datas.TryGetValue<int>(tag, out int data) && CompareUtility.Compare(data + offsetValue, machine.frameIndex, compareType);
            }
        }

        [Serializable]
        public class GroundChecker : IItem
        {
            public bool isNot;

            public bool Execute(ActionNode node)
            {
                IActionController controller = (IActionController)node.actionMachine.controller;
                bool result = controller.CheckGround();
                return isNot ? !result : result;
            }
        }

        [Serializable]
        public class AirChecker : IItem
        {
            public bool isNot;

            public bool Execute(ActionNode node)
            {
                IActionController controller = (IActionController)node.actionMachine.controller;
                bool result = controller.CheckAir();
                return isNot ? !result : result;
            }
        }

        [Serializable]
        public class KeyCodeChecker : IItem
        {
            public ActionKeyCode keyCode;
            public bool isNot;
            public bool fullMatch;

            public bool Execute(ActionNode node)
            {
                IActionMachine machine = node.actionMachine;
                IActionController controller = (IActionController)node.actionMachine.controller;
                bool result = controller.HasKey(keyCode, fullMatch);
                return isNot ? !result : result;
            }
        }

        [Serializable]
        public class VeclocityChecker : IItem
        {
            public Single horizontalVelocity;
            public Single verticalVelocity;
            public CompareType horzontalCmp;
            public CompareType verticalCmp;

            public bool Execute(ActionNode node)
            {
                IActionController controller = (IActionController)node.actionMachine.controller;
                if (horzontalCmp != CompareType.none)
                {
                    int result = new Vector2(controller.velocity.x, controller.velocity.z).magnitude.ApproxCompareTo((Single)horizontalVelocity);
                    if (!CompareUtility.CheckResult(result, horzontalCmp))
                    {
                        return false;
                    }
                }
                if (verticalCmp != CompareType.none)
                {
                    int result = controller.velocity.y.ApproxCompareTo((Single)verticalVelocity);
                    if (!CompareUtility.CheckResult(result, verticalCmp))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [Serializable]
        public class TargetDistanceChecker : IItem
        {
            public Single distance;
            public CompareType compare;

            public bool Execute(ActionNode node)
            {
                IActionMachine machine = node.actionMachine;
                IActionController controller = (IActionController)node.actionMachine.controller;
                int lastAttackObjID = controller.datas.GetValue<int>(DataTag.AimObjID, UObjectSystem.noneID);
                TObject target = App.game.uobj.Get<TObject>(lastAttackObjID);
                if (target == null)
                {
                    return false;
                }

                Single distance = Vector3.Distance(controller.position, target.position);
                return CompareUtility.Compare(distance, (Single)this.distance, compare);
            }
        }

        [Serializable]
        public class WaitFrameChecker : IItem
        {
            public int waitFrameCnt;

            public bool Execute(ActionNode node)
            {
                return node.updateCnt + 1 >= waitFrameCnt;
            }
        }

        public class PropertyChecker : IItem
        {
            public PropertyType type;
            public CompareType compare;

            [DatasetValueTypes]
            [SerializeReference]
            public object value;

            public bool Execute(ActionNode node)
            {
                IActionController controller = (IActionController)node.actionMachine.controller;
                object rawData = value is IPackageData pData ? pData.RawValue() : value;
                object propertyValue = GetPropertyValue(type, controller);
                return CompareUtility.Compare(propertyValue, rawData, compare);
            }

            public object GetPropertyValue(PropertyType type, IActionController controller)
            {
                switch (type)
                {
                    case PropertyType.Hp:
                        return controller.hp;

                    case PropertyType.Power:
                        return controller.power;

                    default:
                        throw new RuntimeException($"不支持该属性 {type}");
                }
            }
        }

        #endregion Items
    }
}