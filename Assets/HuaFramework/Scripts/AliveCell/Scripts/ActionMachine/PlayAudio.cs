/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/8/28 14:12:35
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
    /// PlayAudioConfig
    /// </summary>
    [ActionConfig(typeof(PlayAudio))]
    [Serializable]
    public class PlayAudioConfig : HoldFrames
    {
        [EnableToggle()]
        public bool isRandom;

        [EnableToggleItem(false, nameof(isRandom))]
        public int audioId;

        [EnableToggleItem(nameof(isRandom))]
        public List<int> audioIds;
    }

    /// <summary>
    /// PlayAudio
    /// </summary>
    public class PlayAudio : IActionHandler
    {
        //public class Data
        //{
        //}

        public void Enter(ActionNode node)
        {
            PlayAudioConfig config = (PlayAudioConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            IActionController controller = (IActionController)node.actionMachine.controller;
            //node.data = new Data();

            int audioId = config.isRandom ? 0 : config.audioId;

            if (config.isRandom && config.audioIds?.Count > 0)
            {
                audioId = config.audioIds[UnityEngine.Random.Range(0, config.audioIds.Count)];
            }

            if (audioId > 0)
            {
                UAudioEvent evt = controller.AppendEvent<UAudioEvent>();
                evt.audioId = audioId;
                evt.position = controller.position;
            }
        }

        public void Exit(ActionNode node)
        {
            //PlayAudioConfig config = (PlayAudioConfig)node.config;
            //IActionMachine machine = node.actionMachine;
            //IActionController controller = (IActionController)node.actionMachine.controller;
            //Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            //PlayAudioConfig config = (PlayAudioConfig)node.config;
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