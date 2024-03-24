/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/25 2:35:20
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

namespace AliveCell
{
    /// <summary>
    /// DestroyConfig
    /// </summary>
    [Serializable]
    [ActionConfig(typeof(Destroy))]
    public class DestroyConfig : HoldFrames
    {
        public int waitFrameCnt = 0;
    }

    /// <summary>
    /// Destroy
    /// </summary>
    public class Destroy : IActionHandler
    {
        //public class Data
        //{
        //}

        public void Enter(ActionNode node)
        {
            //DestroyConfig config = (DestroyConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //node.data = new Data();
        }

        public void Exit(ActionNode node)
        {
            //DestroyConfig config = (DestroyConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            DestroyConfig config = (DestroyConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;

            if (node.updateCnt + 1 >= config.waitFrameCnt)
            {
                controller.AppendEvent<UDestroyEvent>();
            }
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}