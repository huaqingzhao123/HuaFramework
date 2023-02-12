using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
using ByteAngle = FPPhysics.FPUtility;
using XMLib;

[ExecuteAlways]
public class FPTest : MonoBehaviour
{
    public UVector3 v3_01;
    public UVector3 v3_02;
    public UVector3 v3_03;
    public UVector3 v3_01_;
    public UVector3 v3_02_;
    public UVector3 v3_03_;

    public UQuaternion q_01;
    public UQuaternion q_02;
    public UQuaternion q_03;
    public UQuaternion q_01_;
    public UQuaternion q_02_;
    public UQuaternion q_03_;

    public UMatrix4x4 m_01;
    public UMatrix4x4 m_02;
    public UMatrix4x4 m_03;

    /*
        Vector2.Angle
        Vector2.SignedAngle
        Vector3.Angle
        Vector3.SignedAngle
        Quaternion * Vector3
        Quaternion.eulerAngles
        Quaternion.FromToRotation
        Quaternion.Euler
        Matrix4x4 * Vector3
    */

    public Transform point01;
    public Transform point02;
    public Transform point03;
    public float size;

    private void OnDrawGizmos()
    {
        v3_01 = transform.rotation.eulerAngles;
        v3_02 = v3_01;

        q_01 = UQuaternion.Euler(v3_01);
        q_02 = Quaternion.Euler((Vector3)v3_02);

        v3_01_ = q_01.eulerAngles;
        v3_02_ =((Quaternion)q_02).eulerAngles;

        point01.rotation = UQuaternion.Euler(v3_01_);
        point02.rotation = Quaternion.Euler((Vector3)v3_02_);
    }
}
