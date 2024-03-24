/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/23 16:26:17
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
    /// EnemyAIConfig
    /// </summary>
    [Serializable]
    [ActionConfig(typeof(EnemyAI))]
    public class EnemyAIConfig
    {
        public Single trackingDistance = 7;
        public Single keepDistance = 2;
        public bool useTracking = true;
        public bool useAttack = true;
    }

    /// <summary>
    /// EnemyAI,只能用于 EnemyObject
    /// </summary>
    public class EnemyAI : IActionHandler
    {
        public class Data
        {
        }

        public void Enter(ActionNode node)
        {
            EnemyAIConfig config = (EnemyAIConfig)node.config;
            IActionMachine machine = node.actionMachine;
            EnemyObject controller = (EnemyObject)node.actionMachine.controller;
            node.data = new Data();
        }

        public void Exit(ActionNode node)
        {
            EnemyAIConfig config = (EnemyAIConfig)node.config;
            IActionMachine machine = node.actionMachine;
            EnemyObject controller = (EnemyObject)node.actionMachine.controller;
            Data data = (Data)node.data;
        }

        public void Update(ActionNode node, Single deltaTime)
        {
            EnemyAIConfig config = (EnemyAIConfig)node.config;
            IActionMachine machine = node.actionMachine;
            EnemyObject controller = (EnemyObject)node.actionMachine.controller;
            Data data = (Data)node.data;

            int trackingPlayer = UObjectSystem.noneID;
            Single playerDistance = 0;

            if (controller.data.nearPlayerId != UObjectSystem.noneID)
            {
                Single distance = controller.data.pid2dist[controller.data.nearPlayerId];
                if (distance < config.trackingDistance)
                {
                    trackingPlayer = controller.data.nearPlayerId;
                    playerDistance = distance;
                }
            }

            if (config.useTracking && trackingPlayer != UObjectSystem.noneID)
            {
                PlayerObject playerObj = App.game.uplayer.GetPlayer(trackingPlayer);
                if (playerObj.hp <= 0)
                {
                    return;
                }

                Vector3 dir = playerObj.position - controller.position;
                dir.y = 0;
                dir.Normalize();
                if (playerDistance > config.keepDistance)
                {
                    App.game.uinput.SetAxisFromDir(controller.ID, true, dir);
                }
                else if (config.useAttack)
                {
                    App.game.uinput.AddKey(controller.ID, ActionKeyCode.Attack);
                }
            }

            controller.datas[DataTag.AimObjID] = trackingPlayer;
            if (trackingPlayer != UObjectSystem.noneID)
            {
                ActionMachineObject obj = App.game.FindObjWithID<ActionMachineObject>(trackingPlayer);
                if (obj != null)
                {
                    List<int> aimSelfObjIDs = obj.datas.GetOrCreateValue<List<int>>(DataTag.AimSelfObjIDsInFrame);
                    aimSelfObjIDs.Add(controller.ID);
                }
            }
        }

        public void Update(ActionNode node, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}