/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/14 1:30:06
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

using Single = FPPhysics.Fix64;

namespace AliveCell.Commons
{
    public interface IUpdate
    {
        void OnUpdate(float deltaTime);
    }

    public interface ISyncLogicUpdate
    {
        void OnSyncLogicUpdate(Single deltaTime);
    }

    public interface ILateUpdate
    {
        void OnLateUpdate(float deltaTime);
    }

    public interface ILogicUpdate
    {
        void OnLogicUpdate(Single deltaTime);
    }

    public interface ILogicUpdateBegin
    {
        void OnLogicUpdateBegin();
    }

    public interface ILogicUpdateEnd
    {
        void OnLogicUpdateEnd();
    }

    public interface ICreate
    {
        void OnCreate();
    }

    public interface IDestroy
    {
        void OnDestroy();
    }
}