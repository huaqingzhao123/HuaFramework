using System;

namespace FPPhysics
{
    /// <summary>
    /// Provides XNA-like quaternion support.
    /// </summary>
    public struct Quaternion : IEquatable<Quaternion>
    {
        /// <summary>
        /// X component of the quaternion.
        /// </summary>
        public Fix64 x;

        /// <summary>
        /// Y component of the quaternion.
        /// </summary>
        public Fix64 y;

        /// <summary>
        /// Z component of the quaternion.
        /// </summary>
        public Fix64 z;

        /// <summary>
        /// W component of the quaternion.
        /// </summary>
        public Fix64 w;

        private static readonly Quaternion identityQuaternion = new Quaternion(0, 0, 0, 1);

        /// <summary>
        ///   <para>The identity rotation (Read Only).</para>
        /// </summary>
        public static Quaternion identity => identityQuaternion;

        /// <summary>
        /// Constructs a new Quaternion.
        /// </summary>
        /// <param name="x">X component of the quaternion.</param>
        /// <param name="y">Y component of the quaternion.</param>
        /// <param name="z">Z component of the quaternion.</param>
        /// <param name="w">W component of the quaternion.</param>
        public Quaternion(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector3 eulerAngles => MakePositive(ToEuler(this) * Fix64.Rad2Deg);

        private static Vector3 ToEuler(Quaternion q)
        {
            q.Normalize();
            var mat = Matrix3x3.CreateFromQuaternion(q);
            Vector3 output = MatrixToEulerZXY(mat);
            return output;
        }

        private static Vector3 MatrixToEulerZXY(Matrix3x3 matrix)
        {
            Vector3 v;
            // from http://www.geometrictools.com/Documentation/EulerAngles.pdf
            // YXZ order
            if (matrix.M32 < 1)
            {
                if (matrix.M32 > -1)
                {
                    v.x = Fix64.Asin(matrix.M32);
                    v.z = Fix64.Atan2(-matrix.M12, matrix.M22);
                    v.y = Fix64.Atan2(-matrix.M31, matrix.M33);
                }
                else
                {
                    v.x = -Fix64.Pi * Fix64.C0p5;
                    v.z = -Fix64.Atan2(-matrix.M13, matrix.M11);
                    v.y = 0;
                }
            }
            else
            {
                v.x = Fix64.Pi * Fix64.C0p5;
                v.z = Fix64.Atan2(matrix.M13, matrix.M11);
                v.y = 0;
            }

            return -v;
        }

        private static Vector3 MakePositive(Vector3 euler)
        {
            Fix64 negativeFlip = -0.0001m * Fix64.Rad2Deg;
            Fix64 positiveFlip = 360 + negativeFlip;

            if (euler.x < negativeFlip)
                euler.x += 360;
            else if (euler.x > positiveFlip)
                euler.x -= 360;

            if (euler.y < negativeFlip)
                euler.y += 360;
            else if (euler.y > positiveFlip)
                euler.y -= 360;

            if (euler.z < negativeFlip)
                euler.z += 360;
            else if (euler.z > positiveFlip)
                euler.z -= 360;

            return euler;
        }

        public static Quaternion FromToRotation(Vector3 fromVector, Vector3 toVector)
        {
            fromVector.Normalize();
            toVector.Normalize();
            GetQuaternionBetweenNormalizedVectors(ref fromVector, ref toVector, out Quaternion result);
            return result;
        }

        public static Quaternion Euler(Fix64 x, Fix64 y, Fix64 z)
        {
            x *= Fix64.Deg2Rad;
            y *= Fix64.Deg2Rad;
            z *= Fix64.Deg2Rad;
            return CreateFromYawPitchRoll(y, x, z);
        }

        public static Quaternion Euler(Vector3 eulerAngles)
        {
            return Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        }

        public static Fix64 Angle(Quaternion a, Quaternion b)
        {
            Quaternion aInv = Quaternion.Inverse(a);
            Quaternion f = b * aInv;

            Fix64 angle = Fix64.Acos(f.w) * 2 * Fix64.Rad2Deg;

            if (angle > 180)
            {
                angle = 360 - angle;
            }

            return angle;
        }

        /// <summary>
        ///   <para>The dot product between two rotations.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static Fix64 Dot(Quaternion a, Quaternion b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public static Quaternion AngleAxis(Fix64 angle, Vector3 axis)
        {
            axis.Normalize();
            angle *= Fix64.Deg2Rad;

            CreateFromAxisAngle(ref axis, angle, out var result);
            return result;
        }


        public static Quaternion LookRotation(Vector3 forward)
        {
            return LookRotation(forward, Vector3.up);
        }

        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards)
        {
            Vector3 a = forward.normalized;
            Vector3 b = Vector3.up;
            GetQuaternionBetweenNormalizedVectors(ref a, ref b, out var result);
            return result;
        }

        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            Fix64 num = rotation.x * 2;
            Fix64 num2 = rotation.y * 2;
            Fix64 num3 = rotation.z * 2;
            Fix64 num4 = rotation.x * num;
            Fix64 num5 = rotation.y * num2;
            Fix64 num6 = rotation.z * num3;
            Fix64 num7 = rotation.x * num2;
            Fix64 num8 = rotation.x * num3;
            Fix64 num9 = rotation.y * num3;
            Fix64 num10 = rotation.w * num;
            Fix64 num11 = rotation.w * num2;
            Fix64 num12 = rotation.w * num3;
            Vector3 result = default(Vector3);
            result.x = (1 - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
            result.y = (num7 + num12) * point.x + (1 - (num4 + num6)) * point.y + (num9 - num10) * point.z;
            result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1 - (num4 + num5)) * point.z;
            return result;
        }


        public static implicit operator UnityEngine.Quaternion(Quaternion value) => new UnityEngine.Quaternion(value.x, value.y, value.z, value.w);
        public static explicit operator Quaternion(UnityEngine.Quaternion value) => new Quaternion((Fix64)value.x, (Fix64)value.y, (Fix64)value.z, (Fix64)value.w);

        /// <summary>
        /// Gets a string representation of the quaternion.
        /// </summary>
        /// <returns>String representing the quaternion.</returns>
        public override string ToString()
        {
            return "{ X: " + x + ", Y: " + y + ", Z: " + z + ", W: " + w + "}";
        }

        /// <summary>
        /// Adds two quaternions together.
        /// </summary>
        /// <param name="a">First quaternion to add.</param>
        /// <param name="b">Second quaternion to add.</param>
        /// <param name="result">Sum of the addition.</param>
        public static void Add(ref Quaternion a, ref Quaternion b, out Quaternion result)
        {
            result.x = a.x + b.x;
            result.y = a.y + b.y;
            result.z = a.z + b.z;
            result.w = a.w + b.w;
        }

        /// <summary>
        /// Multiplies two quaternions.
        /// </summary>
        /// <param name="a">First quaternion to multiply.</param>
        /// <param name="b">Second quaternion to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Multiply(ref Quaternion a, ref Quaternion b, out Quaternion result)
        {
            Fix64 x = a.x;
            Fix64 y = a.y;
            Fix64 z = a.z;
            Fix64 w = a.w;
            Fix64 bX = b.x;
            Fix64 bY = b.y;
            Fix64 bZ = b.z;
            Fix64 bW = b.w;
            result.x = x * bW + bX * w + y * bZ - z * bY;
            result.y = y * bW + bY * w + z * bX - x * bZ;
            result.z = z * bW + bZ * w + x * bY - y * bX;
            result.w = w * bW - x * bX - y * bY - z * bZ;
        }

        /// <summary>
        /// Scales a quaternion.
        /// </summary>
        /// <param name="q">Quaternion to multiply.</param>
        /// <param name="scale">Amount to multiply each component of the quaternion by.</param>
        /// <param name="result">Scaled quaternion.</param>
        public static void Multiply(ref Quaternion q, Fix64 scale, out Quaternion result)
        {
            result.x = q.x * scale;
            result.y = q.y * scale;
            result.z = q.z * scale;
            result.w = q.w * scale;
        }

        /// <summary>
        /// Multiplies two quaternions together in opposite order.
        /// </summary>
        /// <param name="a">First quaternion to multiply.</param>
        /// <param name="b">Second quaternion to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Concatenate(ref Quaternion a, ref Quaternion b, out Quaternion result)
        {
            Fix64 aX = a.x;
            Fix64 aY = a.y;
            Fix64 aZ = a.z;
            Fix64 aW = a.w;
            Fix64 bX = b.x;
            Fix64 bY = b.y;
            Fix64 bZ = b.z;
            Fix64 bW = b.w;

            result.x = aW * bX + aX * bW + aZ * bY - aY * bZ;
            result.y = aW * bY + aY * bW + aX * bZ - aZ * bX;
            result.z = aW * bZ + aZ * bW + aY * bX - aX * bY;
            result.w = aW * bW - aX * bX - aY * bY - aZ * bZ;
        }

        /// <summary>
        /// Multiplies two quaternions together in opposite order.
        /// </summary>
        /// <param name="a">First quaternion to multiply.</param>
        /// <param name="b">Second quaternion to multiply.</param>
        /// <returns>Product of the multiplication.</returns>
        public static Quaternion Concatenate(Quaternion a, Quaternion b)
        {
            Quaternion result;
            Concatenate(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// Quaternion representing the identity transform.
        /// </summary>
        public static Quaternion Identity
        {
            get
            {
                return new Quaternion(Fix64.C0, Fix64.C0, Fix64.C0, Fix64.C1);
            }
        }

        /// <summary>
        /// Constructs a quaternion from a rotation matrix.
        /// </summary>
        /// <param name="r">Rotation matrix to create the quaternion from.</param>
        /// <param name="q">Quaternion based on the rotation matrix.</param>
        public static void CreateFromRotationMatrix(ref Matrix3x3 r, out Quaternion q)
        {
            Fix64 trace = r.M11 + r.M22 + r.M33;
#if !WINDOWS
            q = new Quaternion();
#endif
            if (trace >= Fix64.C0)
            {
                var S = Fix64.Sqrt(trace + Fix64.C1) * Fix64.C2; // S=4*qw
                var inverseS = Fix64.C1 / S;
                q.w = Fix64.C0p25 * S;
                q.x = (r.M23 - r.M32) * inverseS;
                q.y = (r.M31 - r.M13) * inverseS;
                q.z = (r.M12 - r.M21) * inverseS;
            }
            else if ((r.M11 > r.M22) & (r.M11 > r.M33))
            {
                var S = Fix64.Sqrt(Fix64.C1 + r.M11 - r.M22 - r.M33) * Fix64.C2; // S=4*qx
                var inverseS = Fix64.C1 / S;
                q.w = (r.M23 - r.M32) * inverseS;
                q.x = Fix64.C0p25 * S;
                q.y = (r.M21 + r.M12) * inverseS;
                q.z = (r.M31 + r.M13) * inverseS;
            }
            else if (r.M22 > r.M33)
            {
                var S = Fix64.Sqrt(Fix64.C1 + r.M22 - r.M11 - r.M33) * Fix64.C2; // S=4*qy
                var inverseS = Fix64.C1 / S;
                q.w = (r.M31 - r.M13) * inverseS;
                q.x = (r.M21 + r.M12) * inverseS;
                q.y = Fix64.C0p25 * S;
                q.z = (r.M32 + r.M23) * inverseS;
            }
            else
            {
                var S = Fix64.Sqrt(Fix64.C1 + r.M33 - r.M11 - r.M22) * Fix64.C2; // S=4*qz
                var inverseS = Fix64.C1 / S;
                q.w = (r.M12 - r.M21) * inverseS;
                q.x = (r.M31 + r.M13) * inverseS;
                q.y = (r.M32 + r.M23) * inverseS;
                q.z = Fix64.C0p25 * S;
            }
        }

        /// <summary>
        /// Creates a quaternion from a rotation matrix.
        /// </summary>
        /// <param name="r">Rotation matrix used to create a new quaternion.</param>
        /// <returns>Quaternion representing the same rotation as the matrix.</returns>
        public static Quaternion CreateFromRotationMatrix(Matrix3x3 r)
        {
            Quaternion toReturn;
            CreateFromRotationMatrix(ref r, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Constructs a quaternion from a rotation matrix.
        /// </summary>
        /// <param name="r">Rotation matrix to create the quaternion from.</param>
        /// <param name="q">Quaternion based on the rotation matrix.</param>
        public static void CreateFromRotationMatrix(ref Matrix4x4 r, out Quaternion q)
        {
            Matrix3x3 downsizedMatrix;
            Matrix3x3.CreateFromMatrix(ref r, out downsizedMatrix);
            CreateFromRotationMatrix(ref downsizedMatrix, out q);
        }

        /// <summary>
        /// Creates a quaternion from a rotation matrix.
        /// </summary>
        /// <param name="r">Rotation matrix used to create a new quaternion.</param>
        /// <returns>Quaternion representing the same rotation as the matrix.</returns>
        public static Quaternion CreateFromRotationMatrix(Matrix4x4 r)
        {
            Quaternion toReturn;
            CreateFromRotationMatrix(ref r, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Ensures the quaternion has unit length.
        /// </summary>
        /// <param name="quaternion">Quaternion to normalize.</param>
        /// <returns>Normalized quaternion.</returns>
        public static Quaternion Normalize(Quaternion quaternion)
        {
            Quaternion toReturn;
            Normalize(ref quaternion, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Ensures the quaternion has unit length.
        /// </summary>
        /// <param name="quaternion">Quaternion to normalize.</param>
        /// <param name="toReturn">Normalized quaternion.</param>
        public static void Normalize(ref Quaternion quaternion, out Quaternion toReturn)
        {
            Fix64 num = Fix64.Sqrt(Dot(quaternion, quaternion));
            toReturn = num < Fix64.Epsilon ? Identity : new Quaternion(quaternion.x / num, quaternion.y / num, quaternion.z / num, quaternion.w / num);
        }

        /// <summary>
        /// Scales the quaternion such that it has unit length.
        /// </summary>
        public void Normalize()
        {
            Fix64 inverse = Fix64.C1 / Fix64.Sqrt(x * x + y * y + z * z + w * w);
            x *= inverse;
            y *= inverse;
            z *= inverse;
            w *= inverse;
        }

        /// <summary>
        /// Computes the squared length of the quaternion.
        /// </summary>
        /// <returns>Squared length of the quaternion.</returns>
        public Fix64 LengthSquared()
        {
            return x * x + y * y + z * z + w * w;
        }

        /// <summary>
        /// Computes the length of the quaternion.
        /// </summary>
        /// <returns>Length of the quaternion.</returns>
        public Fix64 Length()
        {
            return Fix64.Sqrt(x * x + y * y + z * z + w * w);
        }

        /// <summary>
        /// Blends two quaternions together to get an intermediate state.
        /// </summary>
        /// <param name="start">Starting point of the interpolation.</param>
        /// <param name="end">Ending point of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end point to use.</param>
        /// <param name="result">Interpolated intermediate quaternion.</param>
        public static void Slerp(ref Quaternion start, ref Quaternion end, Fix64 interpolationAmount, out Quaternion result)
        {
            Fix64 cosHalfTheta = start.w * end.w + start.x * end.x + start.y * end.y + start.z * end.z;
            if (cosHalfTheta < Fix64.C0)
            {
                //Negating a quaternion results in the same orientation,
                //but we need cosHalfTheta to be positive to get the shortest path.
                end.x = -end.x;
                end.y = -end.y;
                end.z = -end.z;
                end.w = -end.w;
                cosHalfTheta = -cosHalfTheta;
            }
            // If the orientations are similar enough, then just pick one of the inputs.
            if (cosHalfTheta > Fix64.C1m1em12)
            {
                result.w = start.w;
                result.x = start.x;
                result.y = start.y;
                result.z = start.z;
                return;
            }
            // Calculate temporary values.
            Fix64 halfTheta = Fix64.Acos(cosHalfTheta);
            Fix64 sinHalfTheta = Fix64.Sqrt(Fix64.C1 - cosHalfTheta * cosHalfTheta);

            Fix64 aFraction = Fix64.Sin((Fix64.C1 - interpolationAmount) * halfTheta) / sinHalfTheta;
            Fix64 bFraction = Fix64.Sin(interpolationAmount * halfTheta) / sinHalfTheta;

            //Blend the two quaternions to get the result!
            result.x = (Fix64)(start.x * aFraction + end.x * bFraction);
            result.y = (Fix64)(start.y * aFraction + end.y * bFraction);
            result.z = (Fix64)(start.z * aFraction + end.z * bFraction);
            result.w = (Fix64)(start.w * aFraction + end.w * bFraction);
        }

        /// <summary>
        /// Blends two quaternions together to get an intermediate state.
        /// </summary>
        /// <param name="start">Starting point of the interpolation.</param>
        /// <param name="end">Ending point of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end point to use.</param>
        /// <returns>Interpolated intermediate quaternion.</returns>
        public static Quaternion Slerp(Quaternion start, Quaternion end, Fix64 interpolationAmount)
        {
            Quaternion toReturn;
            Slerp(ref start, ref end, interpolationAmount, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Computes the conjugate of the quaternion.
        /// </summary>
        /// <param name="quaternion">Quaternion to conjugate.</param>
        /// <param name="result">Conjugated quaternion.</param>
        public static void Conjugate(ref Quaternion quaternion, out Quaternion result)
        {
            result.x = -quaternion.x;
            result.y = -quaternion.y;
            result.z = -quaternion.z;
            result.w = quaternion.w;
        }

        /// <summary>
        /// Computes the conjugate of the quaternion.
        /// </summary>
        /// <param name="quaternion">Quaternion to conjugate.</param>
        /// <returns>Conjugated quaternion.</returns>
        public static Quaternion Conjugate(Quaternion quaternion)
        {
            Quaternion toReturn;
            Conjugate(ref quaternion, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Computes the inverse of the quaternion.
        /// </summary>
        /// <param name="quaternion">Quaternion to invert.</param>
        /// <param name="result">Result of the inversion.</param>
        public static void Inverse(ref Quaternion quaternion, out Quaternion result)
        {
            Fix64 inverseSquaredNorm = quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w;
            result.x = -quaternion.x * inverseSquaredNorm;
            result.y = -quaternion.y * inverseSquaredNorm;
            result.z = -quaternion.z * inverseSquaredNorm;
            result.w = quaternion.w * inverseSquaredNorm;
        }

        /// <summary>
        /// Computes the inverse of the quaternion.
        /// </summary>
        /// <param name="quaternion">Quaternion to invert.</param>
        /// <returns>Result of the inversion.</returns>
        public static Quaternion Inverse(Quaternion quaternion)
        {
            Quaternion result;
            Inverse(ref quaternion, out result);
            return result;
        }

        /// <summary>
        /// Tests components for equality.
        /// </summary>
        /// <param name="a">First quaternion to test for equivalence.</param>
        /// <param name="b">Second quaternion to test for equivalence.</param>
        /// <returns>Whether or not the quaternions' components were equal.</returns>
        public static bool operator ==(Quaternion a, Quaternion b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }

        /// <summary>
        /// Tests components for inequality.
        /// </summary>
        /// <param name="a">First quaternion to test for equivalence.</param>
        /// <param name="b">Second quaternion to test for equivalence.</param>
        /// <returns>Whether the quaternions' components were not equal.</returns>
        public static bool operator !=(Quaternion a, Quaternion b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
        }

        /// <summary>
        /// Negates the components of a quaternion.
        /// </summary>
        /// <param name="a">Quaternion to negate.</param>
        /// <param name="b">Negated result.</param>
        public static void Negate(ref Quaternion a, out Quaternion b)
        {
            b.x = -a.x;
            b.y = -a.y;
            b.z = -a.z;
            b.w = -a.w;
        }

        /// <summary>
        /// Negates the components of a quaternion.
        /// </summary>
        /// <param name="q">Quaternion to negate.</param>
        /// <returns>Negated result.</returns>
        public static Quaternion Negate(Quaternion q)
        {
            Negate(ref q, out var result);
            return result;
        }

        /// <summary>
        /// Negates the components of a quaternion.
        /// </summary>
        /// <param name="q">Quaternion to negate.</param>
        /// <returns>Negated result.</returns>
        public static Quaternion operator -(Quaternion q)
        {
            Negate(ref q, out var result);
            return result;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Quaternion other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj is Quaternion)
            {
                return Equals((Quaternion)obj);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode() + w.GetHashCode();
        }

        /// <summary>
        /// Transforms the vector using a quaternion.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void Transform(ref Vector3 v, ref Quaternion rotation, out Vector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            Fix64 x2 = rotation.x + rotation.x;
            Fix64 y2 = rotation.y + rotation.y;
            Fix64 z2 = rotation.z + rotation.z;
            Fix64 xx2 = rotation.x * x2;
            Fix64 xy2 = rotation.x * y2;
            Fix64 xz2 = rotation.x * z2;
            Fix64 yy2 = rotation.y * y2;
            Fix64 yz2 = rotation.y * z2;
            Fix64 zz2 = rotation.z * z2;
            Fix64 wx2 = rotation.w * x2;
            Fix64 wy2 = rotation.w * y2;
            Fix64 wz2 = rotation.w * z2;
            //Defer the component setting since they're used in computation.
            Fix64 transformedX = v.x * (Fix64.C1 - yy2 - zz2) + v.y * (xy2 - wz2) + v.z * (xz2 + wy2);
            Fix64 transformedY = v.x * (xy2 + wz2) + v.y * (Fix64.C1 - xx2 - zz2) + v.z * (yz2 - wx2);
            Fix64 transformedZ = v.x * (xz2 - wy2) + v.y * (yz2 + wx2) + v.z * (Fix64.C1 - xx2 - yy2);
            result.x = transformedX;
            result.y = transformedY;
            result.z = transformedZ;
        }

        /// <summary>
        /// Transforms the vector using a quaternion.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <returns>Transformed vector.</returns>
        public static Vector3 Transform(Vector3 v, Quaternion rotation)
        {
            Vector3 toReturn;
            Transform(ref v, ref rotation, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Transforms a vector using a quaternion. Specialized for x,0,0 vectors.
        /// </summary>
        /// <param name="x">X component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformX(Fix64 x, ref Quaternion rotation, out Vector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            Fix64 y2 = rotation.y + rotation.y;
            Fix64 z2 = rotation.z + rotation.z;
            Fix64 xy2 = rotation.x * y2;
            Fix64 xz2 = rotation.x * z2;
            Fix64 yy2 = rotation.y * y2;
            Fix64 zz2 = rotation.z * z2;
            Fix64 wy2 = rotation.w * y2;
            Fix64 wz2 = rotation.w * z2;
            //Defer the component setting since they're used in computation.
            Fix64 transformedX = x * (Fix64.C1 - yy2 - zz2);
            Fix64 transformedY = x * (xy2 + wz2);
            Fix64 transformedZ = x * (xz2 - wy2);
            result.x = transformedX;
            result.y = transformedY;
            result.z = transformedZ;
        }

        /// <summary>
        /// Transforms a vector using a quaternion. Specialized for 0,y,0 vectors.
        /// </summary>
        /// <param name="y">Y component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformY(Fix64 y, ref Quaternion rotation, out Vector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            Fix64 x2 = rotation.x + rotation.x;
            Fix64 y2 = rotation.y + rotation.y;
            Fix64 z2 = rotation.z + rotation.z;
            Fix64 xx2 = rotation.x * x2;
            Fix64 xy2 = rotation.x * y2;
            Fix64 yz2 = rotation.y * z2;
            Fix64 zz2 = rotation.z * z2;
            Fix64 wx2 = rotation.w * x2;
            Fix64 wz2 = rotation.w * z2;
            //Defer the component setting since they're used in computation.
            Fix64 transformedX = y * (xy2 - wz2);
            Fix64 transformedY = y * (Fix64.C1 - xx2 - zz2);
            Fix64 transformedZ = y * (yz2 + wx2);
            result.x = transformedX;
            result.y = transformedY;
            result.z = transformedZ;
        }

        /// <summary>
        /// Transforms a vector using a quaternion. Specialized for 0,0,z vectors.
        /// </summary>
        /// <param name="z">Z component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformZ(Fix64 z, ref Quaternion rotation, out Vector3 result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            Fix64 x2 = rotation.x + rotation.x;
            Fix64 y2 = rotation.y + rotation.y;
            Fix64 z2 = rotation.z + rotation.z;
            Fix64 xx2 = rotation.x * x2;
            Fix64 xz2 = rotation.x * z2;
            Fix64 yy2 = rotation.y * y2;
            Fix64 yz2 = rotation.y * z2;
            Fix64 wx2 = rotation.w * x2;
            Fix64 wy2 = rotation.w * y2;
            //Defer the component setting since they're used in computation.
            Fix64 transformedX = z * (xz2 + wy2);
            Fix64 transformedY = z * (yz2 - wx2);
            Fix64 transformedZ = z * (Fix64.C1 - xx2 - yy2);
            result.x = transformedX;
            result.y = transformedY;
            result.z = transformedZ;
        }

        /// <summary>
        /// Multiplies two quaternions.
        /// </summary>
        /// <param name="a">First quaternion to multiply.</param>
        /// <param name="b">Second quaternion to multiply.</param>
        /// <returns>Product of the multiplication.</returns>
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            Quaternion toReturn;
            Multiply(ref a, ref b, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Creates a quaternion from an axis and angle.
        /// </summary>
        /// <param name="axis">Axis of rotation.</param>
        /// <param name="angle">Angle to rotate around the axis.</param>
        /// <returns>Quaternion representing the axis and angle rotation.</returns>
        public static Quaternion CreateFromAxisAngle(Vector3 axis, Fix64 angle)
        {
            CreateFromAxisAngle(ref axis, angle, out Quaternion q);
            return q;
        }

        /// <summary>
        /// Creates a quaternion from an axis and angle.
        /// </summary>
        /// <param name="axis">Axis of rotation.</param>
        /// <param name="angle">Angle to rotate around the axis.</param>
        /// <param name="q">Quaternion representing the axis and angle rotation.</param>
        public static void CreateFromAxisAngle(ref Vector3 axis, Fix64 angle, out Quaternion q)
        {
            Fix64 halfAngle = angle * Fix64.C0p5;
            Fix64 s = Fix64.Sin(halfAngle);
            q.x = axis.x * s;
            q.y = axis.y * s;
            q.z = axis.z * s;
            q.w = Fix64.Cos(halfAngle);
        }

        /// <summary>
        /// Constructs a quaternion from yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw of the rotation.</param>
        /// <param name="pitch">Pitch of the rotation.</param>
        /// <param name="roll">Roll of the rotation.</param>
        /// <returns>Quaternion representing the yaw, pitch, and roll.</returns>
        public static Quaternion CreateFromYawPitchRoll(Fix64 yaw, Fix64 pitch, Fix64 roll)
        {
            Quaternion toReturn;
            CreateFromYawPitchRoll(yaw, pitch, roll, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Constructs a quaternion from yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw of the rotation.</param>
        /// <param name="pitch">Pitch of the rotation.</param>
        /// <param name="roll">Roll of the rotation.</param>
        /// <param name="q">Quaternion representing the yaw, pitch, and roll.</param>
        public static void CreateFromYawPitchRoll(Fix64 yaw, Fix64 pitch, Fix64 roll, out Quaternion q)
        {
            Fix64 halfRoll = roll * Fix64.C0p5;
            Fix64 halfPitch = pitch * Fix64.C0p5;
            Fix64 halfYaw = yaw * Fix64.C0p5;

            Fix64 sinRoll = Fix64.Sin(halfRoll);
            Fix64 sinPitch = Fix64.Sin(halfPitch);
            Fix64 sinYaw = Fix64.Sin(halfYaw);

            Fix64 cosRoll = Fix64.Cos(halfRoll);
            Fix64 cosPitch = Fix64.Cos(halfPitch);
            Fix64 cosYaw = Fix64.Cos(halfYaw);

            Fix64 cosYawCosPitch = cosYaw * cosPitch;
            Fix64 cosYawSinPitch = cosYaw * sinPitch;
            Fix64 sinYawCosPitch = sinYaw * cosPitch;
            Fix64 sinYawSinPitch = sinYaw * sinPitch;

            q.x = cosYawSinPitch * cosRoll + sinYawCosPitch * sinRoll;
            q.y = sinYawCosPitch * cosRoll - cosYawSinPitch * sinRoll;
            q.z = cosYawCosPitch * sinRoll - sinYawSinPitch * cosRoll;
            q.w = cosYawCosPitch * cosRoll + sinYawSinPitch * sinRoll;
        }

        /// <summary>
        /// Computes the angle change represented by a normalized quaternion.
        /// </summary>
        /// <param name="q">Quaternion to be converted.</param>
        /// <returns>Angle around the axis represented by the quaternion.</returns>
        public static Fix64 GetAngleFromQuaternion(ref Quaternion q)
        {
            Fix64 qw = Fix64.Abs(q.w);
            if (qw > Fix64.C1)
                return Fix64.C0;
            return Fix64.C2 * Fix64.Acos(qw);
        }

        /// <summary>
        /// Computes the axis angle representation of a normalized quaternion.
        /// </summary>
        /// <param name="q">Quaternion to be converted.</param>
        /// <param name="axis">Axis represented by the quaternion.</param>
        /// <param name="angle">Angle around the axis represented by the quaternion.</param>
        public static void GetAxisAngleFromQuaternion(ref Quaternion q, out Vector3 axis, out Fix64 angle)
        {
#if !WINDOWS
            axis = new Vector3();
#endif
            Fix64 qw = q.w;
            if (qw > Fix64.C0)
            {
                axis.x = q.x;
                axis.y = q.y;
                axis.z = q.z;
            }
            else
            {
                axis.x = -q.x;
                axis.y = -q.y;
                axis.z = -q.z;
                qw = -qw;
            }

            Fix64 lengthSquared = axis.LengthSquared();
            if (lengthSquared > Fix64.C1em14)
            {
                Vector3.Divide(ref axis, Fix64.Sqrt(lengthSquared), out axis);
                angle = Fix64.C2 * Fix64.Acos(MathHelper.Clamp(qw, -1, Fix64.C1));
            }
            else
            {
                axis = Toolbox.UpVector;
                angle = Fix64.C0;
            }
        }

        /// <summary>
        /// Computes the quaternion rotation between two normalized vectors.
        /// </summary>
        /// <param name="v1">First unit-length vector.</param>
        /// <param name="v2">Second unit-length vector.</param>
        /// <param name="q">Quaternion representing the rotation from v1 to v2.</param>
        public static void GetQuaternionBetweenNormalizedVectors(ref Vector3 v1, ref Vector3 v2, out Quaternion q)
        {
            Fix64 dot;
            Vector3.Dot(ref v1, ref v2, out dot);
            //For non-normal vectors, the multiplying the axes length squared would be necessary:
            //Fix64 w = dot + (Fix64)Math.Sqrt(v1.LengthSquared() * v2.LengthSquared());
            if (dot < Fix64.Cm0p9999) //parallel, opposing direction
            {
                //If this occurs, the rotation required is ~180 degrees.
                //The problem is that we could choose any perpendicular axis for the rotation. It's not uniquely defined.
                //The solution is to pick an arbitrary perpendicular axis.
                //Project onto the plane which has the lowest component magnitude.
                //On that 2d plane, perform a 90 degree rotation.
                Fix64 absX = Fix64.Abs(v1.x);
                Fix64 absY = Fix64.Abs(v1.y);
                Fix64 absZ = Fix64.Abs(v1.z);
                if (absX < absY && absX < absZ)
                    q = new Quaternion(Fix64.C0, -v1.z, v1.y, Fix64.C0);
                else if (absY < absZ)
                    //此处进行过修改 因为当人物y轴的eulurAngle = 180或-180时有问题
                    q = new Quaternion(Fix64.C0, v1.z, Fix64.C0, Fix64.C0);
                else
                    q = new Quaternion(-v1.y, v1.x, Fix64.C0, Fix64.C0);
            }
            else
            {
                Vector3 axis;
                Vector3.Cross(ref v1, ref v2, out axis);
                q = new Quaternion(axis.x, axis.y, axis.z, dot + Fix64.C1);
            }
            q.Normalize();
        }

        //The following two functions are highly similar, but it's a bit of a brain teaser to phrase one in terms of the other.
        //Providing both simplifies things.

        /// <summary>
        /// Computes the rotation from the start orientation to the end orientation such that end = Quaternion.Concatenate(start, relative).
        /// </summary>
        /// <param name="start">Starting orientation.</param>
        /// <param name="end">Ending orientation.</param>
        /// <param name="relative">Relative rotation from the start to the end orientation.</param>
        public static void GetRelativeRotation(ref Quaternion start, ref Quaternion end, out Quaternion relative)
        {
            Quaternion startInverse;
            Conjugate(ref start, out startInverse);
            Concatenate(ref startInverse, ref end, out relative);
        }

        /// <summary>
        /// Transforms the rotation into the local space of the target basis such that rotation = Quaternion.Concatenate(localRotation, targetBasis)
        /// </summary>
        /// <param name="rotation">Rotation in the original frame of reference.</param>
        /// <param name="targetBasis">Basis in the original frame of reference to transform the rotation into.</param>
        /// <param name="localRotation">Rotation in the local space of the target basis.</param>
        public static void GetLocalRotation(ref Quaternion rotation, ref Quaternion targetBasis, out Quaternion localRotation)
        {
            Quaternion basisInverse;
            Conjugate(ref targetBasis, out basisInverse);
            Concatenate(ref rotation, ref basisInverse, out localRotation);
        }
    }
}