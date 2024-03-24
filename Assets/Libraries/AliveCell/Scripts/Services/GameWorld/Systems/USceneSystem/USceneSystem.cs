/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/2/18 12:36:29
 */

using AliveCell.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// USceneSystem
    /// </summary>
    public class USceneSystem : ISystem, ICreate, IDestroy
    {
        public readonly SuperLogHandler LogHandler;
        public readonly GameWorld world;

        public USceneSystem(GameWorld world)
        {
            this.world = world;
            this.LogHandler = world.LogHandler.CreateSub("UScene");
        }

        public void OnInitialize(List<UObject> preObjs)
        {
        }

        public void OnCreate()
        {
            App.On<int>(EventTypes.Game_Dead, OnDead);
        }

        public void OnDestroy()
        {
        }

        private void OnDead(int id)
        {
            //LogHandler.Log($"[{id}] 已死亡");
        }
    }
}