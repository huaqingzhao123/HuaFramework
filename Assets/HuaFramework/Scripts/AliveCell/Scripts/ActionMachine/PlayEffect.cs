/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/3 14:42:14
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
    /// PlayEffectConfig
    /// </summary>
    [ActionConfig(typeof(PlayEffect))]
    [Serializable]
    public class PlayEffectConfig : HoldFrames
    {
        public int effectId;
        public Single lifeTime;
        public Vector3 localPosition;
        public Vector3 localRotation;
        public bool updateTransform;
    }

    /// <summary>
    /// PlayEffect
    /// </summary>
    public class PlayEffect : IActionHandler
    {
        //public class Data
        //{
        //}

        public void Enter(ActionNode node)
        {
            PlayEffectConfig config = (PlayEffectConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            //node.data = new Data();

            Quaternion localRotation = Quaternion.Euler((Vector3)config.localRotation);

            App.game.Create<EffectObject>(config.effectId, t =>
            {
                t.lifeTime = (Single)config.lifeTime;
                t.bindObjId = controller.ID;
                t.localPosition = (Vector3)config.localPosition;
                t.localRotation = localRotation;
                t.updateTransform = config.updateTransform;
            });
        }

        public void Exit(ActionNode node)
        {
            //PlayEffectConfig config = (PlayEffectConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            //PlayEffectConfig config = (PlayEffectConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}