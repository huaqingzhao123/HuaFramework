/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/22 12:02:44
 */

using System;
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
    /// <summary>
    /// SVelocityConfig
    /// </summary>
    [ActionConfig(typeof(Velocity))]
    [Serializable]
    public class VelocityConfig : HoldFrames
    {
        public Vector3 velocity;

        [EnableToggle()]
        public bool fixDir;

        [EnableToggleItem(nameof(fixDir))]
        public bool useInput;

        public bool useHeight;
    }

    /// <summary>
    /// SVelocity
    /// </summary>
    public class Velocity : IActionHandler
    {
        public void Enter(ActionNode node)
        {
            VelocityConfig config = (VelocityConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;

            Vector3 velocity;
            if (config.fixDir)
            {
                if (config.useInput)
                {
                    var inputData = controller.GetRawInput();
                    velocity = (Quaternion)inputData.GetRotation() * (Vector3)config.velocity;
                }
                else
                {
                    velocity = (Quaternion)controller.rotation * (Vector3)config.velocity;
                }
            }
            else
            {
                velocity = (Vector3)config.velocity;
            }

            if (config.useHeight)
            {
                velocity.y = Mathf.JumpSpeed(controller.mass, (Single)config.velocity.y);
            }

            controller.velocity = velocity;
        }

        public void Exit(ActionNode node)
        {
            //SApplyForceConfig config = (SApplyForceConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            //SApplyForceConfig config = (SApplyForceConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}