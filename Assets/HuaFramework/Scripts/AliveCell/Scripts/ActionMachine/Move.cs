/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/24 0:02:52
 */

using System;
using UnityEngine;
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
    /// <summary>
    /// SMoveConfig
    /// </summary>
    [ActionConfig(typeof(Move))]
    [Serializable]
    public class MoveConfig
    {
        public Single moveSpeed;
    }

    /// <summary>
    /// SMove
    /// </summary>
    public class Move : IActionHandler
    {
        public void Enter(ActionNode node)
        {
            MoveConfig config = (MoveConfig)node.config;
            IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
        }

        public void Exit(ActionNode node)
        {
            MoveConfig config = (MoveConfig)node.config;
            IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            MoveConfig config = (MoveConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;

            Vector3 velocity = controller.velocity;

            if (controller.HasKey(ActionKeyCode.Axis))
            {
                Single speed = (Single)config.moveSpeed;
                InputData data = controller.GetRawInput();

                Vector3 moveSpeed = data.GetDir() * speed;
                velocity.x = moveSpeed.x;
                velocity.z = moveSpeed.z;
                controller.rotation = data.GetRotation();
            }

            controller.velocity = velocity;
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}