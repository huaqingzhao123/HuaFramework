/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/11/5 14:33:25
 */

using System;
using UnityEngine;
using XMLib;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;

namespace AliveCell
{
    /// <summary>
    /// IActionController
    /// </summary>
    public interface IActionController : IRigidBody
    {
        GameWorld world { get; }

        bool isDebug { get; set; }
        Matrix4x4 localToWorldMatrix { get; }

        LayerMask attackMask { get; }
        int groupID { get; }

        void Log(string msg);

        bool isDead { get; }

        int maxHp { get; }
        int hp { get; }
        int maxPower { get; }
        int power { get; }

        void SetHP(int hp);

        void SetPower(int power);

        InjuredResult Injured(InjuredInfo info);

        AttackerInfo GetAttackInfo();

        T AppendEvent<T>() where T : UEvent, new();

        #region input

        ActionKeyCode GetAllKey();

        byte GetAxis();

        bool HasKey(ActionKeyCode keyCode, bool fullMatch = false);

        InputData GetRawInput();

        #endregion input

        #region data operations

        DataDictionary<DataTag, object> datas { get; }

        #endregion data operations

        #region state

        Quaternion rotation { get; set; }
        Single softRotateScale { get; set; }

        bool CheckAir();

        bool CheckGround();

        bool enableTrail { get; set; }

        #endregion state
    }
}