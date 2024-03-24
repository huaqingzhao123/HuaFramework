/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/4/2 16:51:55
 */

using System;
using System.Collections.Generic;
using XMLib.AM;
using UnityEngine;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using Mathf = FPPhysics.FPUtility;

using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// SDatasetConfig
    /// </summary>
    [ActionConfig(typeof(Dataset))]
    [Serializable]
    public class DatasetConfig : HoldFrames
    {
        [DatasetObjectTypes]
        [SerializeReference]
        public List<DataSets.IItem> enterItems;

        [DatasetObjectTypes]
        [SerializeReference]
        public List<DataSets.IItem> updateItems;

        [DatasetObjectTypes]
        [SerializeReference]
        public List<DataSets.IItem> exitItems;
    }

    /// <summary>
    /// SDataset
    /// </summary>
    public class Dataset : IActionHandler
    {
        public void Enter(ActionNode node)
        {
            DatasetConfig config = (DatasetConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;

            if (config.enterItems == null || config.enterItems.Count == 0)
            {
                return;
            }

            foreach (var item in config.enterItems)
            {
                item.Execute(node);
            }
        }

        public void Exit(ActionNode node)
        {
            DatasetConfig config = (DatasetConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;

            if (config.exitItems == null || config.exitItems.Count == 0)
            {
                return;
            }
            foreach (var item in config.exitItems)
            {
                item.Execute(node);
            }
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            DatasetConfig config = (DatasetConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;

            if (config.updateItems == null || config.updateItems.Count == 0)
            {
                return;
            }
            foreach (var item in config.updateItems)
            {
                item.Execute(node);
            }
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }

    namespace DataSets
    {
        #region Items

        public interface IItem
        {
            void Execute(ActionNode node);
        }

        [Serializable]
        public class SetCurrentFrameIndex : IItem
        {
            public DataTag tag;

            public void Execute(ActionNode node)
            {
                IActionMachine machine = node.actionMachine;
                IActionController controller = (IActionController)node.actionMachine.controller;
                controller.datas[tag] = machine.frameIndex;
            }
        }

        [Serializable]
        public class Set : IItem
        {
            public DataTag tag;

            [DatasetValueTypes]
            [SerializeReference]
            public object value;

            public void Execute(ActionNode node)
            {
                IActionController controller = (IActionController)node.actionMachine.controller;
                controller.datas[tag] = value;
            }
        }

        [Serializable]
        public class ConditionSet : IItem
        {
            [ConditionTypes]
            [SerializeReference]
            public List<Conditions.IItem> checker;

            public DataTag tag;

            [DatasetValueTypes]
            [SerializeReference]
            public object value;

            public void Execute(ActionNode node)
            {
                if (!Condition.Checker(checker, node))
                {
                    return;
                }

                IActionController controller = (IActionController)node.actionMachine.controller;
                controller.datas[tag] = value;
            }
        }

        [Serializable]
        public class Remove : IItem
        {
            public DataTag tag;

            public void Execute(ActionNode node)
            {
                IActionController controller = (IActionController)node.actionMachine.controller;
                controller.datas.Remove(tag);
            }
        }

        #endregion Items
    }
}