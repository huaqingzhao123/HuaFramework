/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/5/3 2:27:59
 */

using System;
using XMLib;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;
using Space = FPPhysics.Space;

namespace AliveCell
{
    public class ObjectTypesPrimaryAttribute : ObjectTypesAttribute
    {
        public static readonly Type[] primaryTypes = {
            typeof(P<int>),
            typeof(P<Single>),
            typeof(P<bool>),
            typeof(P<string>),
            typeof(P<Vector2>),
            typeof(P<Vector3>),
        };

        public ObjectTypesPrimaryAttribute(params Type[] extraTypes) :
            base(ArrayUtility.Combine(extraTypes, primaryTypes))
        {
        }
    }

    public class DatasetValueTypesAttribute : ObjectTypesPrimaryAttribute
    {
        public DatasetValueTypesAttribute() :
            base(typeof(P<BindPointType>))
        {
        }
    }

    public class DatasetObjectTypesAttribute : ObjectTypesAttribute
    {
        public override Type baseType => typeof(DataSets.IItem);
    }

    public class ConditionTypesAttribute : ObjectTypesAttribute
    {
        public override Type baseType => typeof(Conditions.IItem);
    }

    public class AttackTypesAttribute : ObjectTypesAttribute
    {
        public AttackTypesAttribute() :
            base(typeof(Attacks.AudioConfig),
                typeof(Attacks.ShakeConfig),
                typeof(Attacks.VelocityConfig),
                typeof(Attacks.EffectConfig),
                typeof(Attacks.LevitateConfig),
                typeof(Attacks.AddHpPowerConfig)
                )
        {
        }
    }
}