using System;

namespace FPPhysics
{
    /// <summary>
    /// Provides XNA-like 2D vector math.
    /// </summary>
    [Serializable]
    public struct Vector2 : IEquatable<Vector2>
    {
        /// <summary>
        /// X component of the vector.
        /// </summary>
        public Fix64 x;

        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public Fix64 y;

        private static Vector2 zeroVector = new Vector2(0, 0);
        private static Vector2 oneVector = new Vector2(1, 1);
        private static Vector2 rightVector = new Vector2(1, 0);
        private static Vector2 leftVector = new Vector2(-1, 0);
        private static Vector2 upVector = new Vector2(0, 1);
        private static Vector2 downVector = new Vector2(0, -1);

        public static Vector2 zero => zeroVector;
        public static Vector2 one => oneVector;
        public static Vector2 left => leftVector;
        public static Vector2 right => rightVector;
        public static Vector2 up => upVector;
        public static Vector2 down => downVector;

        /// <summary>
        ///   <para>Returns this vector with a magnitude of 1 (Read Only).</para>
        /// </summary>
        public Vector2 normalized
        {
            get
            {
                Vector2 result = new Vector2(x, y);
                result.Normalize();
                return result;
            }
        }

        /// <summary>
        ///   <para>Returns the length of this vector (Read Only).</para>
        /// </summary>
        public Fix64 magnitude => FPUtility.Sqrt(x * x + y * y);

        /// <summary>
        ///   <para>Returns the squared length of this vector (Read Only).</para>
        /// </summary>
        public Fix64 sqrMagnitude => x * x + y * y;

        /// <summary>
        /// Constructs a new two dimensional vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="y">Y component of the vector.</param>
        public Vector2(Fix64 x, Fix64 y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        ///   <para>Set x and y components of an existing Vector2.</para>
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        public void Set(Fix64 newX, Fix64 newY)
        {
            x = newX;
            y = newY;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, Fix64 t)
        {
            t = FPUtility.Clamp01(t);
            return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        public static Vector2 MoveTowards(Vector2 current, Vector2 target, Fix64 maxDistanceDelta)
        {
            Fix64 num = target.x - current.x;
            Fix64 num2 = target.y - current.y;
            Fix64 num3 = num * num + num2 * num2;
            if (num3 == 0 || (maxDistanceDelta >= 0 && num3 <= maxDistanceDelta * maxDistanceDelta))
            {
                return target;
            }
            Fix64 num4 = FPUtility.Sqrt(num3);
            return new Vector2(current.x + num / num4 * maxDistanceDelta, current.y + num2 / num4 * maxDistanceDelta);
        }

        public static Vector2 Scale(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        public void Scale(Vector2 scale)
        {
            x *= scale.x;
            y *= scale.y;
        }

        /// <summary>
        /// Normalizes the vector.
        /// </summary>
        public void Normalize()
        {
            Fix64 inverse = Fix64.C1 / FPUtility.Sqrt(x * x + y * y);
            x *= inverse;
            y *= inverse;
        }

        public static Vector2 Reflect(Vector2 vector, Vector2 normal)
        {
            Fix64 dot = 2 * Dot(vector, normal);
            return new Vector2(vector.x + (dot * normal.x), vector.y + (dot * normal.y));
        }

        public static Vector2 Perpendicular(Vector2 inDirection)
        {
            return new Vector2(0 - inDirection.y, inDirection.x);
        }

        /// <summary>
        /// Computes the dot product of the two vectors.
        /// </summary>
        /// <param name="a">First vector of the dot product.</param>
        /// <param name="b">Second vector of the dot product.</param>
        /// <returns>Dot product of the two vectors.</returns>
        public static Fix64 Dot(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static Fix64 Angle(Vector2 a, Vector2 b)
        {
            return FPUtility.Acos(Dot(a.normalized, b.normalized)) * Fix64.Rad2Deg;
        }

        /// <summary>
        ///   <para>Returns the signed angle in degrees between from and to.</para>
        /// </summary>
        /// <param name="from">The vector from which the angular difference is measured.</param>
        /// <param name="to">The vector to which the angular difference is measured.</param>
        public static Fix64 SignedAngle(Vector2 from, Vector2 to)
        {
            Fix64 num = Angle(from, to);
            Fix64 num2 = FPUtility.Sign(from.x * to.y - from.y * to.x);
            return num * num2;
        }

        /// <summary>
        ///   <para>Returns the distance between a and b.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static Fix64 Distance(Vector2 a, Vector2 b)
        {
            Fix64 num = a.x - b.x;
            Fix64 num2 = a.y - b.y;
            return FPUtility.Sqrt(num * num + num2 * num2);
        }

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the lesser values of each vector.</returns>
        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x < b.x ? a.x : b.x, a.y < b.y ? a.y : b.y);
        }

        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the greater values of each vector.</returns>
        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x > b.x ? a.x : b.x, a.y > b.y ? a.y : b.y);
        }

        public static implicit operator UnityEngine.Vector2(Vector2 value) => new UnityEngine.Vector2(value.x, value.y);

        public static explicit operator Vector2(UnityEngine.Vector2 value) => new Vector2((Fix64)value.x, (Fix64)value.y);

        public static implicit operator Vector3(Vector2 value) => new Vector3(value.x, value.y, 0);

        public static implicit operator Vector2(Vector3 value) => new Vector2(value.x, value.y);

        /// <summary>
        /// Gets a string representation of the vector.
        /// </summary>
        /// <returns>String representing the vector.</returns>
        public override string ToString()
        {
            return "{" + x + ", " + y + "}";
        }

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <param name="sum">Sum of the two vectors.</param>
        public static void Add(ref Vector2 a, ref Vector2 b, out Vector2 sum)
        {
            sum.x = a.x + b.x;
            sum.y = a.y + b.y;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to subtract from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <param name="difference">Result of the subtraction.</param>
        public static void Subtract(ref Vector2 a, ref Vector2 b, out Vector2 difference)
        {
            difference.x = a.x - b.x;
            difference.y = a.y - b.y;
        }

        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="scale">Amount to scale.</param>
        /// <param name="result">Scaled vector.</param>
        public static void Multiply(ref Vector2 v, Fix64 scale, out Vector2 result)
        {
            result.x = v.x * scale;
            result.y = v.y * scale;
        }

        /// <summary>
        /// Multiplies two vectors on a per-component basis.
        /// </summary>
        /// <param name="a">First vector to multiply.</param>
        /// <param name="b">Second vector to multiply.</param>
        /// <param name="result">Result of the componentwise multiplication.</param>
        public static void Multiply(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.x = a.x * b.x;
            result.y = a.y * b.y;
        }

        /// <summary>
        /// Divides a vector's components by some amount.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="divisor">Value to divide the vector's components.</param>
        /// <param name="result">Result of the division.</param>
        public static void Divide(ref Vector2 v, Fix64 divisor, out Vector2 result)
        {
            Fix64 inverse = Fix64.C1 / divisor;
            result.x = v.x * inverse;
            result.y = v.y * inverse;
        }

        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <param name="negated">Negated version of the vector.</param>
        public static void Negate(ref Vector2 v, out Vector2 negated)
        {
            negated.x = -v.x;
            negated.y = -v.y;
        }

        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <param name="result">Vector with nonnegative elements.</param>
        public static void Abs(ref Vector2 v, out Vector2 result)
        {
            if (v.x < Fix64.C0)
                result.x = -v.x;
            else
                result.x = v.x;
            if (v.y < Fix64.C0)
                result.y = -v.y;
            else
                result.y = v.y;
        }

        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <returns>Vector with nonnegative elements.</returns>
        public static Vector2 Abs(Vector2 v)
        {
            Vector2 result;
            Abs(ref v, out result);
            return result;
        }

        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="f">Amount to scale.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector2 operator *(Vector2 v, Fix64 f)
        {
            Vector2 toReturn;
            toReturn.x = v.x * f;
            toReturn.y = v.y * f;
            return toReturn;
        }

        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="f">Amount to scale.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector2 operator *(Fix64 f, Vector2 v)
        {
            Vector2 toReturn;
            toReturn.x = v.x * f;
            toReturn.y = v.y * f;
            return toReturn;
        }

        /// <summary>
        /// Multiplies two vectors on a per-component basis.
        /// </summary>
        /// <param name="a">First vector to multiply.</param>
        /// <param name="b">Second vector to multiply.</param>
        /// <returns>Result of the componentwise multiplication.</returns>
        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            Vector2 result;
            Multiply(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// Divides a vector.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="f">Amount to divide.</param>
        /// <returns>Divided vector.</returns>
        public static Vector2 operator /(Vector2 v, Fix64 f)
        {
            Vector2 toReturn;
            f = Fix64.C1 / f;
            toReturn.x = v.x * f;
            toReturn.y = v.y * f;
            return toReturn;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to be subtracted from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <returns>Resulting difference.</returns>
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            Vector2 v;
            v.x = a.x - b.x;
            v.y = a.y - b.y;
            return v;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <returns>Sum of the addition.</returns>
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            Vector2 v;
            v.x = a.x + b.x;
            v.y = a.y + b.y;
            return v;
        }

        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <returns>Negated vector.</returns>
        public static Vector2 operator -(Vector2 v)
        {
            v.x = -v.x;
            v.y = -v.y;
            return v;
        }

        /// <summary>
        /// Tests two vectors for componentwise equivalence.
        /// </summary>
        /// <param name="a">First vector to test for equivalence.</param>
        /// <param name="b">Second vector to test for equivalence.</param>
        /// <returns>Whether the vectors were equivalent.</returns>
        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        /// <summary>
        /// Tests two vectors for componentwise inequivalence.
        /// </summary>
        /// <param name="a">First vector to test for inequivalence.</param>
        /// <param name="b">Second vector to test for inequivalence.</param>
        /// <returns>Whether the vectors were inequivalent.</returns>
        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return a.x != b.x || a.y != b.y;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Vector2 other)
        {
            return x == other.x && y == other.y;
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
            if (obj is Vector2)
            {
                return Equals((Vector2)obj);
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
            return x.GetHashCode() + y.GetHashCode();
        }
    }
}