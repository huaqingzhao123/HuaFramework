namespace FPPhysics
{
    /// <summary>
    /// Provides XNA-like 4x4 matrix math.
    /// </summary>
    public struct Matrix4x4
    {
        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        public Fix64 M11;

        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        public Fix64 M12;

        /// <summary>
        /// Value at row 1, column 3 of the matrix.
        /// </summary>
        public Fix64 M13;

        /// <summary>
        /// Value at row 1, column 4 of the matrix.
        /// </summary>
        public Fix64 M14;

        /// <summary>
        /// Value at row 2, column 1 of the matrix.
        /// </summary>
        public Fix64 M21;

        /// <summary>
        /// Value at row 2, column 2 of the matrix.
        /// </summary>
        public Fix64 M22;

        /// <summary>
        /// Value at row 2, column 3 of the matrix.
        /// </summary>
        public Fix64 M23;

        /// <summary>
        /// Value at row 2, column 4 of the matrix.
        /// </summary>
        public Fix64 M24;

        /// <summary>
        /// Value at row 3, column 1 of the matrix.
        /// </summary>
        public Fix64 M31;

        /// <summary>
        /// Value at row 3, column 2 of the matrix.
        /// </summary>
        public Fix64 M32;

        /// <summary>
        /// Value at row 3, column 3 of the matrix.
        /// </summary>
        public Fix64 M33;

        /// <summary>
        /// Value at row 3, column 4 of the matrix.
        /// </summary>
        public Fix64 M34;

        /// <summary>
        /// Value at row 4, column 1 of the matrix.
        /// </summary>
        public Fix64 M41;

        /// <summary>
        /// Value at row 4, column 2 of the matrix.
        /// </summary>
        public Fix64 M42;

        /// <summary>
        /// Value at row 4, column 3 of the matrix.
        /// </summary>
        public Fix64 M43;

        /// <summary>
        /// Value at row 4, column 4 of the matrix.
        /// </summary>
        public Fix64 M44;

        /// <summary>
        /// Constructs a new 4 row, 4 column matrix.
        /// </summary>
        /// <param name="m11">Value at row 1, column 1 of the matrix.</param>
        /// <param name="m12">Value at row 1, column 2 of the matrix.</param>
        /// <param name="m13">Value at row 1, column 3 of the matrix.</param>
        /// <param name="m14">Value at row 1, column 4 of the matrix.</param>
        /// <param name="m21">Value at row 2, column 1 of the matrix.</param>
        /// <param name="m22">Value at row 2, column 2 of the matrix.</param>
        /// <param name="m23">Value at row 2, column 3 of the matrix.</param>
        /// <param name="m24">Value at row 2, column 4 of the matrix.</param>
        /// <param name="m31">Value at row 3, column 1 of the matrix.</param>
        /// <param name="m32">Value at row 3, column 2 of the matrix.</param>
        /// <param name="m33">Value at row 3, column 3 of the matrix.</param>
        /// <param name="m34">Value at row 3, column 4 of the matrix.</param>
        /// <param name="m41">Value at row 4, column 1 of the matrix.</param>
        /// <param name="m42">Value at row 4, column 2 of the matrix.</param>
        /// <param name="m43">Value at row 4, column 3 of the matrix.</param>
        /// <param name="m44">Value at row 4, column 4 of the matrix.</param>
        public Matrix4x4(Fix64 m11, Fix64 m12, Fix64 m13, Fix64 m14,
                      Fix64 m21, Fix64 m22, Fix64 m23, Fix64 m24,
                      Fix64 m31, Fix64 m32, Fix64 m33, Fix64 m34,
                      Fix64 m41, Fix64 m42, Fix64 m43, Fix64 m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;

            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;

            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;

            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        public Matrix4x4 inverse => Invert(this);

        public Quaternion rotation => Quaternion.CreateFromRotationMatrix(this);

        public Vector3 MultiplyVector(Vector3 vector)
        {
            Vector3 result = default(Vector3);
            result.x = M11 * vector.x + M21 * vector.y + M31 * vector.z;
            result.y = M12 * vector.x + M22 * vector.y + M32 * vector.z;
            result.z = M13 * vector.x + M23 * vector.y + M33 * vector.z;
            return result;
        }

        public Vector3 MultiplyPoint(Vector3 point)
        {
            Vector3 result = default(Vector3);
            result.x = M11 * point.x + M21 * point.y + M31 * point.z + M41;
            result.y = M12 * point.x + M22 * point.y + M32 * point.z + M42;
            result.z = M13 * point.x + M23 * point.y + M33 * point.z + M43;
            Fix64 num = M14 * point.x + M24 * point.y + M34 * point.z + M44;
            num = 1 / num;
            result.x *= num;
            result.y *= num;
            result.z *= num;
            return result;
        }

        public static Matrix4x4 Rotate(Quaternion q)
        {
            Fix64 num = q.x * 2;
            Fix64 num2 = q.y * 2;
            Fix64 num3 = q.z * 2;
            Fix64 num4 = q.x * num;
            Fix64 num5 = q.y * num2;
            Fix64 num6 = q.z * num3;
            Fix64 num7 = q.x * num2;
            Fix64 num8 = q.x * num3;
            Fix64 num9 = q.y * num3;
            Fix64 num10 = q.w * num;
            Fix64 num11 = q.w * num2;
            Fix64 num12 = q.w * num3;
            Matrix4x4 result = default(Matrix4x4);
            result.M11 = 1 - (num5 + num6);
            result.M12 = num7 + num12;
            result.M13 = num8 - num11;
            result.M14 = 0;
            result.M21 = num7 - num12;
            result.M22 = 1 - (num4 + num6);
            result.M23 = num9 + num10;
            result.M24 = 0;
            result.M31 = num8 + num11;
            result.M32 = num9 - num10;
            result.M33 = 1 - (num4 + num5);
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;
            return result;
        }

        public static Matrix4x4 TRS(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            return Translate(translation) * Rotate(rotation) * Scale(scale);
        }

        public static Vector4 operator *(Matrix4x4 lhs, Vector4 vector)
        {
            Vector4 result = default(Vector4);
            result.x = lhs.M11 * vector.x + lhs.M21 * vector.y + lhs.M31 * vector.z + lhs.M41 * vector.w;
            result.y = lhs.M12 * vector.x + lhs.M22 * vector.y + lhs.M32 * vector.z + lhs.M42 * vector.w;
            result.z = lhs.M13 * vector.x + lhs.M23 * vector.y + lhs.M33 * vector.z + lhs.M43 * vector.w;
            result.w = lhs.M14 * vector.x + lhs.M24 * vector.y + lhs.M34 * vector.z + lhs.M44 * vector.w;
            return result;
        }

        public static implicit operator UnityEngine.Matrix4x4(Matrix4x4 value) => new UnityEngine.Matrix4x4()
        {
            m00 = value.M11,
            m10 = value.M12,
            m20 = value.M13,
            m30 = value.M14,

            m01 = value.M21,
            m11 = value.M22,
            m21 = value.M23,
            m31 = value.M24,

            m02 = value.M31,
            m12 = value.M32,
            m22 = value.M33,
            m32 = value.M34,

            m03 = value.M41,
            m13 = value.M42,
            m23 = value.M43,
            m33 = value.M44,
        };

        public static explicit operator Matrix4x4(UnityEngine.Matrix4x4 value) => new Matrix4x4()
        {
            M11 = (Fix64)value.m00,
            M12 = (Fix64)value.m10,
            M13 = (Fix64)value.m20,
            M14 = (Fix64)value.m30,

            M21 = (Fix64)value.m01,
            M22 = (Fix64)value.m11,
            M23 = (Fix64)value.m21,
            M24 = (Fix64)value.m31,

            M31 = (Fix64)value.m02,
            M32 = (Fix64)value.m12,
            M33 = (Fix64)value.m22,
            M34 = (Fix64)value.m32,

            M41 = (Fix64)value.m03,
            M42 = (Fix64)value.m13,
            M43 = (Fix64)value.m23,
            M44 = (Fix64)value.m33,
        };

        /// <summary>
        /// Creates a string representation of the matrix.
        /// </summary>
        /// <returns>A string representation of the matrix.</returns>
        public override string ToString()
        {
            return "{" + M11 + ", " + M12 + ", " + M13 + ", " + M14 + "} " +
                   "{" + M21 + ", " + M22 + ", " + M23 + ", " + M24 + "} " +
                   "{" + M31 + ", " + M32 + ", " + M33 + ", " + M34 + "} " +
                   "{" + M41 + ", " + M42 + ", " + M43 + ", " + M44 + "}";
        }

        /// <summary>
        /// Gets or sets the translation component of the transform.
        /// </summary>
        public Vector3 Translation
        {
            get
            {
                return new Vector3()
                {
                    x = M41,
                    y = M42,
                    z = M43
                };
            }
            set
            {
                M41 = value.x;
                M42 = value.y;
                M43 = value.z;
            }
        }

        /// <summary>
        /// Gets or sets the backward vector of the matrix.
        /// </summary>
        public Vector3 Backward
        {
            get
            {
#if !WINDOWS
                Vector3 vector = new Vector3();
#else
                Vector3 vector;
#endif
                vector.x = M31;
                vector.y = M32;
                vector.z = M33;
                return vector;
            }
            set
            {
                M31 = value.x;
                M32 = value.y;
                M33 = value.z;
            }
        }

        /// <summary>
        /// Gets or sets the down vector of the matrix.
        /// </summary>
        public Vector3 Down
        {
            get
            {
#if !WINDOWS
                Vector3 vector = new Vector3();
#else
                Vector3 vector;
#endif
                vector.x = -M21;
                vector.y = -M22;
                vector.z = -M23;
                return vector;
            }
            set
            {
                M21 = -value.x;
                M22 = -value.y;
                M23 = -value.z;
            }
        }

        /// <summary>
        /// Gets or sets the forward vector of the matrix.
        /// </summary>
        public Vector3 Forward
        {
            get
            {
#if !WINDOWS
                Vector3 vector = new Vector3();
#else
                Vector3 vector;
#endif
                vector.x = -M31;
                vector.y = -M32;
                vector.z = -M33;
                return vector;
            }
            set
            {
                M31 = -value.x;
                M32 = -value.y;
                M33 = -value.z;
            }
        }

        /// <summary>
        /// Gets or sets the left vector of the matrix.
        /// </summary>
        public Vector3 Left
        {
            get
            {
#if !WINDOWS
                Vector3 vector = new Vector3();
#else
                Vector3 vector;
#endif
                vector.x = -M11;
                vector.y = -M12;
                vector.z = -M13;
                return vector;
            }
            set
            {
                M11 = -value.x;
                M12 = -value.y;
                M13 = -value.z;
            }
        }

        /// <summary>
        /// Gets or sets the right vector of the matrix.
        /// </summary>
        public Vector3 Right
        {
            get
            {
#if !WINDOWS
                Vector3 vector = new Vector3();
#else
                Vector3 vector;
#endif
                vector.x = M11;
                vector.y = M12;
                vector.z = M13;
                return vector;
            }
            set
            {
                M11 = value.x;
                M12 = value.y;
                M13 = value.z;
            }
        }

        /// <summary>
        /// Gets or sets the up vector of the matrix.
        /// </summary>
        public Vector3 Up
        {
            get
            {
#if !WINDOWS
                Vector3 vector = new Vector3();
#else
                Vector3 vector;
#endif
                vector.x = M21;
                vector.y = M22;
                vector.z = M23;
                return vector;
            }
            set
            {
                M21 = value.x;
                M22 = value.y;
                M23 = value.z;
            }
        }

        /// <summary>
        /// Computes the determinant of the matrix.
        /// </summary>
        /// <returns></returns>
        public Fix64 Determinant()
        {
            //Compute the re-used 2x2 determinants.
            Fix64 det1 = M33 * M44 - M34 * M43;
            Fix64 det2 = M32 * M44 - M34 * M42;
            Fix64 det3 = M32 * M43 - M33 * M42;
            Fix64 det4 = M31 * M44 - M34 * M41;
            Fix64 det5 = M31 * M43 - M33 * M41;
            Fix64 det6 = M31 * M42 - M32 * M41;
            return
                (M11 * ((M22 * det1 - M23 * det2) + M24 * det3)) -
                (M12 * ((M21 * det1 - M23 * det4) + M24 * det5)) +
                (M13 * ((M21 * det2 - M22 * det4) + M24 * det6)) -
                (M14 * ((M21 * det3 - M22 * det5) + M23 * det6));
        }

        /// <summary>
        /// Transposes the matrix in-place.
        /// </summary>
        public void Transpose()
        {
            Fix64 intermediate = M12;
            M12 = M21;
            M21 = intermediate;

            intermediate = M13;
            M13 = M31;
            M31 = intermediate;

            intermediate = M14;
            M14 = M41;
            M41 = intermediate;

            intermediate = M23;
            M23 = M32;
            M32 = intermediate;

            intermediate = M24;
            M24 = M42;
            M42 = intermediate;

            intermediate = M34;
            M34 = M43;
            M43 = intermediate;
        }

        /// <summary>
        /// Creates a matrix representing the given axis and angle rotation.
        /// </summary>
        /// <param name="axis">Axis around which to rotate.</param>
        /// <param name="angle">Angle to rotate around the axis.</param>
        /// <returns>Matrix created from the axis and angle.</returns>
        public static Matrix4x4 CreateFromAxisAngle(Vector3 axis, Fix64 angle)
        {
            Matrix4x4 toReturn;
            CreateFromAxisAngle(ref axis, angle, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Creates a matrix representing the given axis and angle rotation.
        /// </summary>
        /// <param name="axis">Axis around which to rotate.</param>
        /// <param name="angle">Angle to rotate around the axis.</param>
        /// <param name="result">Matrix created from the axis and angle.</param>
        public static void CreateFromAxisAngle(ref Vector3 axis, Fix64 angle, out Matrix4x4 result)
        {
            Fix64 xx = axis.x * axis.x;
            Fix64 yy = axis.y * axis.y;
            Fix64 zz = axis.z * axis.z;
            Fix64 xy = axis.x * axis.y;
            Fix64 xz = axis.x * axis.z;
            Fix64 yz = axis.y * axis.z;

            Fix64 sinAngle = Fix64.Sin(angle);
            Fix64 oneMinusCosAngle = Fix64.C1 - Fix64.Cos(angle);

            result.M11 = Fix64.C1 + oneMinusCosAngle * (xx - Fix64.C1);
            result.M21 = -axis.z * sinAngle + oneMinusCosAngle * xy;
            result.M31 = axis.y * sinAngle + oneMinusCosAngle * xz;
            result.M41 = Fix64.C0;

            result.M12 = axis.z * sinAngle + oneMinusCosAngle * xy;
            result.M22 = Fix64.C1 + oneMinusCosAngle * (yy - Fix64.C1);
            result.M32 = -axis.x * sinAngle + oneMinusCosAngle * yz;
            result.M42 = Fix64.C0;

            result.M13 = -axis.y * sinAngle + oneMinusCosAngle * xz;
            result.M23 = axis.x * sinAngle + oneMinusCosAngle * yz;
            result.M33 = Fix64.C1 + oneMinusCosAngle * (zz - Fix64.C1);
            result.M43 = Fix64.C0;

            result.M14 = Fix64.C0;
            result.M24 = Fix64.C0;
            result.M34 = Fix64.C0;
            result.M44 = Fix64.C1;
        }

        /// <summary>
        /// Creates a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="quaternion">Quaternion to convert.</param>
        /// <param name="result">Rotation matrix created from the quaternion.</param>
        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix4x4 result)
        {
            Fix64 qX2 = quaternion.x + quaternion.x;
            Fix64 qY2 = quaternion.y + quaternion.y;
            Fix64 qZ2 = quaternion.z + quaternion.z;
            Fix64 XX = qX2 * quaternion.x;
            Fix64 YY = qY2 * quaternion.y;
            Fix64 ZZ = qZ2 * quaternion.z;
            Fix64 XY = qX2 * quaternion.y;
            Fix64 XZ = qX2 * quaternion.z;
            Fix64 XW = qX2 * quaternion.w;
            Fix64 YZ = qY2 * quaternion.z;
            Fix64 YW = qY2 * quaternion.w;
            Fix64 ZW = qZ2 * quaternion.w;

            result.M11 = Fix64.C1 - YY - ZZ;
            result.M21 = XY - ZW;
            result.M31 = XZ + YW;
            result.M41 = Fix64.C0;

            result.M12 = XY + ZW;
            result.M22 = Fix64.C1 - XX - ZZ;
            result.M32 = YZ - XW;
            result.M42 = Fix64.C0;

            result.M13 = XZ - YW;
            result.M23 = YZ + XW;
            result.M33 = Fix64.C1 - XX - YY;
            result.M43 = Fix64.C0;

            result.M14 = Fix64.C0;
            result.M24 = Fix64.C0;
            result.M34 = Fix64.C0;
            result.M44 = Fix64.C1;
        }

        /// <summary>
        /// Creates a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="quaternion">Quaternion to convert.</param>
        /// <returns>Rotation matrix created from the quaternion.</returns>
        public static Matrix4x4 CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix4x4 toReturn;
            CreateFromQuaternion(ref quaternion, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Multiplies two matrices together.
        /// </summary>
        /// <param name="a">First matrix to multiply.</param>
        /// <param name="b">Second matrix to multiply.</param>
        /// <param name="result">Combined transformation.</param>
        public static void Multiply(ref Matrix4x4 a, ref Matrix4x4 b, out Matrix4x4 result)
        {
            Fix64 resultM11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
            Fix64 resultM12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
            Fix64 resultM13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
            Fix64 resultM14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44;

            Fix64 resultM21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
            Fix64 resultM22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
            Fix64 resultM23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
            Fix64 resultM24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44;

            Fix64 resultM31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
            Fix64 resultM32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
            Fix64 resultM33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
            Fix64 resultM34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44;

            Fix64 resultM41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
            Fix64 resultM42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
            Fix64 resultM43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
            Fix64 resultM44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44;

            result.M11 = resultM11;
            result.M12 = resultM12;
            result.M13 = resultM13;
            result.M14 = resultM14;

            result.M21 = resultM21;
            result.M22 = resultM22;
            result.M23 = resultM23;
            result.M24 = resultM24;

            result.M31 = resultM31;
            result.M32 = resultM32;
            result.M33 = resultM33;
            result.M34 = resultM34;

            result.M41 = resultM41;
            result.M42 = resultM42;
            result.M43 = resultM43;
            result.M44 = resultM44;
        }

        /// <summary>
        /// Multiplies two matrices together.
        /// </summary>
        /// <param name="a">First matrix to multiply.</param>
        /// <param name="b">Second matrix to multiply.</param>
        /// <returns>Combined transformation.</returns>
        public static Matrix4x4 Multiply(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 result;
            Multiply(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// Scales all components of the matrix.
        /// </summary>
        /// <param name="matrix">Matrix to scale.</param>
        /// <param name="scale">Amount to scale.</param>
        /// <param name="result">Scaled matrix.</param>
        public static void Multiply(ref Matrix4x4 matrix, Fix64 scale, out Matrix4x4 result)
        {
            result.M11 = matrix.M11 * scale;
            result.M12 = matrix.M12 * scale;
            result.M13 = matrix.M13 * scale;
            result.M14 = matrix.M14 * scale;

            result.M21 = matrix.M21 * scale;
            result.M22 = matrix.M22 * scale;
            result.M23 = matrix.M23 * scale;
            result.M24 = matrix.M24 * scale;

            result.M31 = matrix.M31 * scale;
            result.M32 = matrix.M32 * scale;
            result.M33 = matrix.M33 * scale;
            result.M34 = matrix.M34 * scale;

            result.M41 = matrix.M41 * scale;
            result.M42 = matrix.M42 * scale;
            result.M43 = matrix.M43 * scale;
            result.M44 = matrix.M44 * scale;
        }

        /// <summary>
        /// Multiplies two matrices together.
        /// </summary>
        /// <param name="a">First matrix to multiply.</param>
        /// <param name="b">Second matrix to multiply.</param>
        /// <returns>Combined transformation.</returns>
        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 toReturn;
            Multiply(ref a, ref b, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Scales all components of the matrix by the given value.
        /// </summary>
        /// <param name="m">First matrix to multiply.</param>
        /// <param name="f">Scaling value to apply to all components of the matrix.</param>
        /// <returns>Product of the multiplication.</returns>
        public static Matrix4x4 operator *(Matrix4x4 m, Fix64 f)
        {
            Matrix4x4 result;
            Multiply(ref m, f, out result);
            return result;
        }

        /// <summary>
        /// Scales all components of the matrix by the given value.
        /// </summary>
        /// <param name="m">First matrix to multiply.</param>
        /// <param name="f">Scaling value to apply to all components of the matrix.</param>
        /// <returns>Product of the multiplication.</returns>
        public static Matrix4x4 operator *(Fix64 f, Matrix4x4 m)
        {
            Matrix4x4 result;
            Multiply(ref m, f, out result);
            return result;
        }

        /// <summary>
        /// Transforms a vector using a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void Transform(ref Vector4 v, ref Matrix4x4 matrix, out Vector4 result)
        {
            Fix64 vX = v.x;
            Fix64 vY = v.y;
            Fix64 vZ = v.z;
            Fix64 vW = v.w;
            result.x = vX * matrix.M11 + vY * matrix.M21 + vZ * matrix.M31 + vW * matrix.M41;
            result.y = vX * matrix.M12 + vY * matrix.M22 + vZ * matrix.M32 + vW * matrix.M42;
            result.z = vX * matrix.M13 + vY * matrix.M23 + vZ * matrix.M33 + vW * matrix.M43;
            result.w = vX * matrix.M14 + vY * matrix.M24 + vZ * matrix.M34 + vW * matrix.M44;
        }

        /// <summary>
        /// Transforms a vector using a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to apply to the vector.</param>
        /// <returns>Transformed vector.</returns>
        public static Vector4 Transform(Vector4 v, Matrix4x4 matrix)
        {
            Vector4 toReturn;
            Transform(ref v, ref matrix, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Transforms a vector using the transpose of a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to tranpose and apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformTranspose(ref Vector4 v, ref Matrix4x4 matrix, out Vector4 result)
        {
            Fix64 vX = v.x;
            Fix64 vY = v.y;
            Fix64 vZ = v.z;
            Fix64 vW = v.w;
            result.x = vX * matrix.M11 + vY * matrix.M12 + vZ * matrix.M13 + vW * matrix.M14;
            result.y = vX * matrix.M21 + vY * matrix.M22 + vZ * matrix.M23 + vW * matrix.M24;
            result.z = vX * matrix.M31 + vY * matrix.M32 + vZ * matrix.M33 + vW * matrix.M34;
            result.w = vX * matrix.M41 + vY * matrix.M42 + vZ * matrix.M43 + vW * matrix.M44;
        }

        /// <summary>
        /// Transforms a vector using the transpose of a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to tranpose and apply to the vector.</param>
        /// <returns>Transformed vector.</returns>
        public static Vector4 TransformTranspose(Vector4 v, Matrix4x4 matrix)
        {
            Vector4 toReturn;
            TransformTranspose(ref v, ref matrix, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Transforms a vector using a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void Transform(ref Vector3 v, ref Matrix4x4 matrix, out Vector4 result)
        {
            result.x = v.x * matrix.M11 + v.y * matrix.M21 + v.z * matrix.M31 + matrix.M41;
            result.y = v.x * matrix.M12 + v.y * matrix.M22 + v.z * matrix.M32 + matrix.M42;
            result.z = v.x * matrix.M13 + v.y * matrix.M23 + v.z * matrix.M33 + matrix.M43;
            result.w = v.x * matrix.M14 + v.y * matrix.M24 + v.z * matrix.M34 + matrix.M44;
        }

        /// <summary>
        /// Transforms a vector using a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to apply to the vector.</param>
        /// <returns>Transformed vector.</returns>
        public static Vector4 Transform(Vector3 v, Matrix4x4 matrix)
        {
            Vector4 toReturn;
            Transform(ref v, ref matrix, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Transforms a vector using the transpose of a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to tranpose and apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformTranspose(ref Vector3 v, ref Matrix4x4 matrix, out Vector4 result)
        {
            result.x = v.x * matrix.M11 + v.y * matrix.M12 + v.z * matrix.M13 + matrix.M14;
            result.y = v.x * matrix.M21 + v.y * matrix.M22 + v.z * matrix.M23 + matrix.M24;
            result.z = v.x * matrix.M31 + v.y * matrix.M32 + v.z * matrix.M33 + matrix.M34;
            result.w = v.x * matrix.M41 + v.y * matrix.M42 + v.z * matrix.M43 + matrix.M44;
        }

        /// <summary>
        /// Transforms a vector using the transpose of a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to tranpose and apply to the vector.</param>
        /// <returns>Transformed vector.</returns>
        public static Vector4 TransformTranspose(Vector3 v, Matrix4x4 matrix)
        {
            Vector4 toReturn;
            TransformTranspose(ref v, ref matrix, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Transforms a vector using a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void Transform(ref Vector3 v, ref Matrix4x4 matrix, out Vector3 result)
        {
            Fix64 vX = v.x;
            Fix64 vY = v.y;
            Fix64 vZ = v.z;
            result.x = vX * matrix.M11 + vY * matrix.M21 + vZ * matrix.M31 + matrix.M41;
            result.y = vX * matrix.M12 + vY * matrix.M22 + vZ * matrix.M32 + matrix.M42;
            result.z = vX * matrix.M13 + vY * matrix.M23 + vZ * matrix.M33 + matrix.M43;
        }

        /// <summary>
        /// Transforms a vector using the transpose of a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to tranpose and apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformTranspose(ref Vector3 v, ref Matrix4x4 matrix, out Vector3 result)
        {
            Fix64 vX = v.x;
            Fix64 vY = v.y;
            Fix64 vZ = v.z;
            result.x = vX * matrix.M11 + vY * matrix.M12 + vZ * matrix.M13 + matrix.M14;
            result.y = vX * matrix.M21 + vY * matrix.M22 + vZ * matrix.M23 + matrix.M24;
            result.z = vX * matrix.M31 + vY * matrix.M32 + vZ * matrix.M33 + matrix.M34;
        }

        /// <summary>
        /// Transforms a vector using a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformNormal(ref Vector3 v, ref Matrix4x4 matrix, out Vector3 result)
        {
            Fix64 vX = v.x;
            Fix64 vY = v.y;
            Fix64 vZ = v.z;
            result.x = vX * matrix.M11 + vY * matrix.M21 + vZ * matrix.M31;
            result.y = vX * matrix.M12 + vY * matrix.M22 + vZ * matrix.M32;
            result.z = vX * matrix.M13 + vY * matrix.M23 + vZ * matrix.M33;
        }

        /// <summary>
        /// Transforms a vector using a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to apply to the vector.</param>
        /// <returns>Transformed vector.</returns>
        public static Vector3 TransformNormal(Vector3 v, Matrix4x4 matrix)
        {
            Vector3 toReturn;
            TransformNormal(ref v, ref matrix, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Transforms a vector using the transpose of a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to tranpose and apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformNormalTranspose(ref Vector3 v, ref Matrix4x4 matrix, out Vector3 result)
        {
            Fix64 vX = v.x;
            Fix64 vY = v.y;
            Fix64 vZ = v.z;
            result.x = vX * matrix.M11 + vY * matrix.M12 + vZ * matrix.M13;
            result.y = vX * matrix.M21 + vY * matrix.M22 + vZ * matrix.M23;
            result.z = vX * matrix.M31 + vY * matrix.M32 + vZ * matrix.M33;
        }

        /// <summary>
        /// Transforms a vector using the transpose of a matrix.
        /// </summary>
        /// <param name="v">Vector to transform.</param>
        /// <param name="matrix">Transform to tranpose and apply to the vector.</param>
        /// <returns>Transformed vector.</returns>
        public static Vector3 TransformNormalTranspose(Vector3 v, Matrix4x4 matrix)
        {
            Vector3 toReturn;
            TransformNormalTranspose(ref v, ref matrix, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Transposes the matrix.
        /// </summary>
        /// <param name="m">Matrix to transpose.</param>
        /// <param name="transposed">Matrix to transpose.</param>
        public static void Transpose(ref Matrix4x4 m, out Matrix4x4 transposed)
        {
            Fix64 intermediate = m.M12;
            transposed.M12 = m.M21;
            transposed.M21 = intermediate;

            intermediate = m.M13;
            transposed.M13 = m.M31;
            transposed.M31 = intermediate;

            intermediate = m.M14;
            transposed.M14 = m.M41;
            transposed.M41 = intermediate;

            intermediate = m.M23;
            transposed.M23 = m.M32;
            transposed.M32 = intermediate;

            intermediate = m.M24;
            transposed.M24 = m.M42;
            transposed.M42 = intermediate;

            intermediate = m.M34;
            transposed.M34 = m.M43;
            transposed.M43 = intermediate;

            transposed.M11 = m.M11;
            transposed.M22 = m.M22;
            transposed.M33 = m.M33;
            transposed.M44 = m.M44;
        }

        /// <summary>
        /// Inverts the matrix.
        /// </summary>
        /// <param name="m">Matrix to invert.</param>
        /// <param name="inverted">Inverted version of the matrix.</param>
        public static void Invert(ref Matrix4x4 m, out Matrix4x4 inverted)
        {
            Matrix4x8.Invert(ref m, out inverted);
        }


        /// <summary>
        /// Inverts the matrix.
        /// </summary>
        /// <param name="m">Matrix to invert.</param>
        /// <returns>Inverted version of the matrix.</returns>
        public static Matrix4x4 Invert(Matrix4x4 m)
        {
            Matrix4x4 inverted;
            Invert(ref m, out inverted);
            return inverted;
        }

        /// <summary>
        /// Inverts the matrix using a process that only works for rigid transforms.
        /// </summary>
        /// <param name="m">Matrix to invert.</param>
        /// <param name="inverted">Inverted version of the matrix.</param>
        public static void InvertRigid(ref Matrix4x4 m, out Matrix4x4 inverted)
        {
            //Invert (transpose) the upper left 3x3 rotation.
            Fix64 intermediate = m.M12;
            inverted.M12 = m.M21;
            inverted.M21 = intermediate;

            intermediate = m.M13;
            inverted.M13 = m.M31;
            inverted.M31 = intermediate;

            intermediate = m.M23;
            inverted.M23 = m.M32;
            inverted.M32 = intermediate;

            inverted.M11 = m.M11;
            inverted.M22 = m.M22;
            inverted.M33 = m.M33;

            //Translation component
            var vX = m.M41;
            var vY = m.M42;
            var vZ = m.M43;
            inverted.M41 = -(vX * inverted.M11 + vY * inverted.M21 + vZ * inverted.M31);
            inverted.M42 = -(vX * inverted.M12 + vY * inverted.M22 + vZ * inverted.M32);
            inverted.M43 = -(vX * inverted.M13 + vY * inverted.M23 + vZ * inverted.M33);

            //Last chunk.
            inverted.M14 = Fix64.C0;
            inverted.M24 = Fix64.C0;
            inverted.M34 = Fix64.C0;
            inverted.M44 = Fix64.C1;
        }

        /// <summary>
        /// Inverts the matrix using a process that only works for rigid transforms.
        /// </summary>
        /// <param name="m">Matrix to invert.</param>
        /// <returns>Inverted version of the matrix.</returns>
        public static Matrix4x4 InvertRigid(Matrix4x4 m)
        {
            Matrix4x4 inverse;
            InvertRigid(ref m, out inverse);
            return inverse;
        }

        /// <summary>
        /// Gets the 4x4 identity matrix.
        /// </summary>
        public static Matrix4x4 identity
        {
            get
            {
                Matrix4x4 toReturn;
                toReturn.M11 = Fix64.C1;
                toReturn.M12 = Fix64.C0;
                toReturn.M13 = Fix64.C0;
                toReturn.M14 = Fix64.C0;

                toReturn.M21 = Fix64.C0;
                toReturn.M22 = Fix64.C1;
                toReturn.M23 = Fix64.C0;
                toReturn.M24 = Fix64.C0;

                toReturn.M31 = Fix64.C0;
                toReturn.M32 = Fix64.C0;
                toReturn.M33 = Fix64.C1;
                toReturn.M34 = Fix64.C0;

                toReturn.M41 = Fix64.C0;
                toReturn.M42 = Fix64.C0;
                toReturn.M43 = Fix64.C0;
                toReturn.M44 = Fix64.C1;
                return toReturn;
            }
        }

        /// <summary>
        /// Creates a right handed orthographic projection.
        /// </summary>
        /// <param name="left">Leftmost coordinate of the projected area.</param>
        /// <param name="right">Rightmost coordinate of the projected area.</param>
        /// <param name="bottom">Bottom coordinate of the projected area.</param>
        /// <param name="top">Top coordinate of the projected area.</param>
        /// <param name="zNear">Near plane of the projection.</param>
        /// <param name="zFar">Far plane of the projection.</param>
        /// <param name="projection">The resulting orthographic projection matrix.</param>
        public static void CreateOrthographicRH(Fix64 left, Fix64 right, Fix64 bottom, Fix64 top, Fix64 zNear, Fix64 zFar, out Matrix4x4 projection)
        {
            Fix64 width = right - left;
            Fix64 height = top - bottom;
            Fix64 depth = zFar - zNear;
            projection.M11 = Fix64.C2 / width;
            projection.M12 = Fix64.C0;
            projection.M13 = Fix64.C0;
            projection.M14 = Fix64.C0;

            projection.M21 = Fix64.C0;
            projection.M22 = Fix64.C2 / height;
            projection.M23 = Fix64.C0;
            projection.M24 = Fix64.C0;

            projection.M31 = Fix64.C0;
            projection.M32 = Fix64.C0;
            projection.M33 = -1 / depth;
            projection.M34 = Fix64.C0;

            projection.M41 = (left + right) / -width;
            projection.M42 = (top + bottom) / -height;
            projection.M43 = zNear / -depth;
            projection.M44 = Fix64.C1;
        }

        /// <summary>
        /// Creates a right-handed perspective matrix.
        /// </summary>
        /// <param name="fieldOfView">Field of view of the perspective in radians.</param>
        /// <param name="aspectRatio">Width of the viewport over the height of the viewport.</param>
        /// <param name="nearClip">Near clip plane of the perspective.</param>
        /// <param name="farClip">Far clip plane of the perspective.</param>
        /// <param name="perspective">Resulting perspective matrix.</param>
        public static void CreatePerspectiveFieldOfViewRH(Fix64 fieldOfView, Fix64 aspectRatio, Fix64 nearClip, Fix64 farClip, out Matrix4x4 perspective)
        {
            Fix64 h = Fix64.C1 / Fix64.Tan(fieldOfView / Fix64.C2);
            Fix64 w = h / aspectRatio;
            perspective.M11 = w;
            perspective.M12 = Fix64.C0;
            perspective.M13 = Fix64.C0;
            perspective.M14 = Fix64.C0;

            perspective.M21 = Fix64.C0;
            perspective.M22 = h;
            perspective.M23 = Fix64.C0;
            perspective.M24 = Fix64.C0;

            perspective.M31 = Fix64.C0;
            perspective.M32 = Fix64.C0;
            perspective.M33 = farClip / (nearClip - farClip);
            perspective.M34 = -1;

            perspective.M41 = Fix64.C0;
            perspective.M42 = Fix64.C0;
            perspective.M44 = Fix64.C0;
            perspective.M43 = nearClip * perspective.M33;
        }

        /// <summary>
        /// Creates a right-handed perspective matrix.
        /// </summary>
        /// <param name="fieldOfView">Field of view of the perspective in radians.</param>
        /// <param name="aspectRatio">Width of the viewport over the height of the viewport.</param>
        /// <param name="nearClip">Near clip plane of the perspective.</param>
        /// <param name="farClip">Far clip plane of the perspective.</param>
        /// <returns>Resulting perspective matrix.</returns>
        public static Matrix4x4 CreatePerspectiveFieldOfViewRH(Fix64 fieldOfView, Fix64 aspectRatio, Fix64 nearClip, Fix64 farClip)
        {
            Matrix4x4 perspective;
            CreatePerspectiveFieldOfViewRH(fieldOfView, aspectRatio, nearClip, farClip, out perspective);
            return perspective;
        }

        /// <summary>
        /// Creates a view matrix pointing from a position to a target with the given up vector.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="target">Target of the camera.</param>
        /// <param name="upVector">Up vector of the camera.</param>
        /// <param name="viewMatrix">Look at matrix.</param>
        public static void CreateLookAtRH(ref Vector3 position, ref Vector3 target, ref Vector3 upVector, out Matrix4x4 viewMatrix)
        {
            Vector3 forward;
            Vector3.Subtract(ref target, ref position, out forward);
            CreateViewRH(ref position, ref forward, ref upVector, out viewMatrix);
        }

        /// <summary>
        /// Creates a view matrix pointing from a position to a target with the given up vector.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="target">Target of the camera.</param>
        /// <param name="upVector">Up vector of the camera.</param>
        /// <returns>Look at matrix.</returns>
        public static Matrix4x4 CreateLookAtRH(Vector3 position, Vector3 target, Vector3 upVector)
        {
            Matrix4x4 lookAt;
            Vector3 forward;
            Vector3.Subtract(ref target, ref position, out forward);
            CreateViewRH(ref position, ref forward, ref upVector, out lookAt);
            return lookAt;
        }

        /// <summary>
        /// Creates a view matrix pointing in a direction with a given up vector.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="forward">Forward direction of the camera.</param>
        /// <param name="upVector">Up vector of the camera.</param>
        /// <param name="viewMatrix">Look at matrix.</param>
        public static void CreateViewRH(ref Vector3 position, ref Vector3 forward, ref Vector3 upVector, out Matrix4x4 viewMatrix)
        {
            Vector3 z;
            Fix64 length = forward.Length();
            Vector3.Divide(ref forward, -length, out z);
            Vector3 x;
            Vector3.Cross(ref upVector, ref z, out x);
            x.Normalize();
            Vector3 y;
            Vector3.Cross(ref z, ref x, out y);

            viewMatrix.M11 = x.x;
            viewMatrix.M12 = y.x;
            viewMatrix.M13 = z.x;
            viewMatrix.M14 = Fix64.C0;
            viewMatrix.M21 = x.y;
            viewMatrix.M22 = y.y;
            viewMatrix.M23 = z.y;
            viewMatrix.M24 = Fix64.C0;
            viewMatrix.M31 = x.z;
            viewMatrix.M32 = y.z;
            viewMatrix.M33 = z.z;
            viewMatrix.M34 = Fix64.C0;
            Vector3.Dot(ref x, ref position, out viewMatrix.M41);
            Vector3.Dot(ref y, ref position, out viewMatrix.M42);
            Vector3.Dot(ref z, ref position, out viewMatrix.M43);
            viewMatrix.M41 = -viewMatrix.M41;
            viewMatrix.M42 = -viewMatrix.M42;
            viewMatrix.M43 = -viewMatrix.M43;
            viewMatrix.M44 = Fix64.C1;
        }

        /// <summary>
        /// Creates a view matrix pointing looking in a direction with a given up vector.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="forward">Forward direction of the camera.</param>
        /// <param name="upVector">Up vector of the camera.</param>
        /// <returns>Look at matrix.</returns>
        public static Matrix4x4 CreateViewRH(Vector3 position, Vector3 forward, Vector3 upVector)
        {
            Matrix4x4 lookat;
            CreateViewRH(ref position, ref forward, ref upVector, out lookat);
            return lookat;
        }

        /// <summary>
        /// Creates a world matrix pointing from a position to a target with the given up vector.
        /// </summary>
        /// <param name="position">Position of the transform.</param>
        /// <param name="forward">Forward direction of the transformation.</param>
        /// <param name="upVector">Up vector which is crossed against the forward vector to compute the transform's basis.</param>
        /// <param name="worldMatrix">World matrix.</param>
        public static void CreateWorldRH(ref Vector3 position, ref Vector3 forward, ref Vector3 upVector, out Matrix4x4 worldMatrix)
        {
            Vector3 z;
            Fix64 length = forward.Length();
            Vector3.Divide(ref forward, -length, out z);
            Vector3 x;
            Vector3.Cross(ref upVector, ref z, out x);
            x.Normalize();
            Vector3 y;
            Vector3.Cross(ref z, ref x, out y);

            worldMatrix.M11 = x.x;
            worldMatrix.M12 = x.y;
            worldMatrix.M13 = x.z;
            worldMatrix.M14 = Fix64.C0;
            worldMatrix.M21 = y.x;
            worldMatrix.M22 = y.y;
            worldMatrix.M23 = y.z;
            worldMatrix.M24 = Fix64.C0;
            worldMatrix.M31 = z.x;
            worldMatrix.M32 = z.y;
            worldMatrix.M33 = z.z;
            worldMatrix.M34 = Fix64.C0;

            worldMatrix.M41 = position.x;
            worldMatrix.M42 = position.y;
            worldMatrix.M43 = position.z;
            worldMatrix.M44 = Fix64.C1;
        }

        /// <summary>
        /// Creates a world matrix pointing from a position to a target with the given up vector.
        /// </summary>
        /// <param name="position">Position of the transform.</param>
        /// <param name="forward">Forward direction of the transformation.</param>
        /// <param name="upVector">Up vector which is crossed against the forward vector to compute the transform's basis.</param>
        /// <returns>World matrix.</returns>
        public static Matrix4x4 CreateWorldRH(Vector3 position, Vector3 forward, Vector3 upVector)
        {
            Matrix4x4 lookat;
            CreateWorldRH(ref position, ref forward, ref upVector, out lookat);
            return lookat;
        }

        /// <summary>
        /// Creates a matrix representing a translation.
        /// </summary>
        /// <param name="translation">Translation to be represented by the matrix.</param>
        /// <param name="translationMatrix">Matrix representing the given translation.</param>
        public static void CreateTranslation(ref Vector3 translation, out Matrix4x4 translationMatrix)
        {
            translationMatrix = new Matrix4x4
            {
                M11 = Fix64.C1,
                M22 = Fix64.C1,
                M33 = Fix64.C1,
                M44 = Fix64.C1,
                M41 = translation.x,
                M42 = translation.y,
                M43 = translation.z
            };
        }

        /// <summary>
        /// Creates a matrix representing a translation.
        /// </summary>
        /// <param name="translation">Translation to be represented by the matrix.</param>
        /// <returns>Matrix representing the given translation.</returns>
        public static Matrix4x4 Translate(Vector3 translation)
        {
            Matrix4x4 translationMatrix;
            CreateTranslation(ref translation, out translationMatrix);
            return translationMatrix;
        }

        /// <summary>
        /// Creates a matrix representing the given axis aligned scale.
        /// </summary>
        /// <param name="scale">Scale to be represented by the matrix.</param>
        /// <param name="scaleMatrix">Matrix representing the given scale.</param>
        public static void CreateScale(ref Vector3 scale, out Matrix4x4 scaleMatrix)
        {
            scaleMatrix = new Matrix4x4
            {
                M11 = scale.x,
                M22 = scale.y,
                M33 = scale.z,
                M44 = Fix64.C1
            };
        }

        /// <summary>
        /// Creates a matrix representing the given axis aligned scale.
        /// </summary>
        /// <param name="scale">Scale to be represented by the matrix.</param>
        /// <returns>Matrix representing the given scale.</returns>
        public static Matrix4x4 Scale(Vector3 scale)
        {
            Matrix4x4 scaleMatrix;
            CreateScale(ref scale, out scaleMatrix);
            return scaleMatrix;
        }

        /// <summary>
        /// Creates a matrix representing the given axis aligned scale.
        /// </summary>
        /// <param name="x">Scale along the x axis.</param>
        /// <param name="y">Scale along the y axis.</param>
        /// <param name="z">Scale along the z axis.</param>
        /// <param name="scaleMatrix">Matrix representing the given scale.</param>
        public static void CreateScale(Fix64 x, Fix64 y, Fix64 z, out Matrix4x4 scaleMatrix)
        {
            scaleMatrix = new Matrix4x4
            {
                M11 = x,
                M22 = y,
                M33 = z,
                M44 = Fix64.C1
            };
        }

        /// <summary>
        /// Creates a matrix representing the given axis aligned scale.
        /// </summary>
        /// <param name="x">Scale along the x axis.</param>
        /// <param name="y">Scale along the y axis.</param>
        /// <param name="z">Scale along the z axis.</param>
        /// <returns>Matrix representing the given scale.</returns>
        public static Matrix4x4 CreateScale(Fix64 x, Fix64 y, Fix64 z)
        {
            Matrix4x4 scaleMatrix;
            CreateScale(x, y, z, out scaleMatrix);
            return scaleMatrix;
        }
    }
}