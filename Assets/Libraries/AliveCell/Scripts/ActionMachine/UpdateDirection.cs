/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/24 13:23:39
*/

using System;
using System.Collections;
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

using MathUtility = FPPhysics.FPUtility;

namespace AliveCell
{
    /// <summary>
    /// UpdateDirectionConfig
    /// </summary>
    [ActionConfig(typeof(UpdateDirection))]
    [Serializable]
    public class UpdateDirectionConfig : HoldFrames
    {
    }

    /// <summary>
    /// UpdateDirection
    /// </summary>
    public class UpdateDirection : IActionHandler
    {
        //public class Data
        //{
        //}

        public void Enter(ActionNode node)
        {
            //UpdateDirectionConfig config = (UpdateDirectionConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //node.data = new Data();
        }

        public void Exit(ActionNode node)
        {
            //UpdateDirectionConfig config = (UpdateDirectionConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            //UpdateDirectionConfig config = (UpdateDirectionConfig)node.config;
            IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;

            int id = controller.datas.GetValue(DataTag.AimObjID, UObjectSystem.noneID);
            TObject target = App.game.uobj.Get<TObject>(id);
            if (target != null)
            {
                Vector3 dir = target.position - controller.position;
                Single angle = MathUtility.FixedByteAngleYFromDir(dir);
                controller.rotation = Quaternion.AngleAxis(angle, Vector3.up);
                return;
            }

            InputData data = controller.GetRawInput();
            if ((data.GetKeyCode() & ActionKeyCode.Axis) != 0)
            {
                controller.rotation = data.GetRotation();
            }
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}