using System;

namespace FPPhysics
{
    /// <summary>
    /// Provides XNA-like 3D vector math.
    /// </summary>
    [Serializable]
    public struct Vector3 : IEquatable<Vector3>
    {
        /// <summary>
        /// X component of the vector.
        /// </summary>
        public Fix64 x;

        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public Fix64 y;

        /// <summary>
        /// Z component of the vector.
        /// </summary>
        public Fix64 z;

        private static readonly Vector3 zeroVector = new Vector3(0, 0, 0);

        private static readonly Vector3 oneVector = new Vector3(1, 1, 1);

        private static readonly Vector3 upVector = new Vector3(0, 1, 0);

        private static readonly Vector3 downVector = new Vector3(0, -1, 0);

        private static readonly Vector3 leftVector = new Vector3(-1, 0, 0);

        private static readonly Vector3 rightVector = new Vector3(1, 0, 0);

        private static readonly Vector3 forwardVector = new Vector3(0, 0, 1);

        private static readonly Vector3 backVector = new Vector3(0, 0, -1);

        private static readonly Vector3 mirrorOneVector = new Vector3(-1, 1, 1);

        /// <summary>
        ///   <para>Returns this vector with a magnitude of 1 (Read Only).</para>
        /// </summary>
        public Vector3 normalized => Normalize(this);

        /// <summary>
        ///   <para>Returns the length of this vector (Read Only).</para>
        /// </summary>
        public Fix64 magnitude => FPUtility.Sqrt(x * x + y * y + z * z);

        /// <summary>
        ///   <para>Returns the squared length of this vector (Read Only).</para>
        /// </summary>
        public Fix64 sqrMagnitude => x * x + y * y + z * z;

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 0, 0).</para>
        /// </summary>
        public static Vector3 zero => zeroVector;

        /// <summary>
        ///   <para>Shorthand for writing Vector3(1, 1, 1).</para>
        /// </summary>
        public static Vector3 one => oneVector;

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 0, 1).</para>
        /// </summary>
        public static Vector3 forward => forwardVector;

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 0, -1).</para>
        /// </summary>
        public static Vector3 back => backVector;

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 1, 0).</para>
        /// </summary>
        public static Vector3 up => upVector;

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, -1, 0).</para>
        /// </summary>
        public static Vector3 down => downVector;

        /// <summary>
        ///   <para>Shorthand for writing Vector3(-1, 0, 0).</para>
        /// </summary>
        public static Vector3 left => leftVector;

        /// <summary>
        ///   <para>Shorthand for writing Vector3(1, 0, 0).</para>
        /// </summary>
        public static Vector3 right => rightVector;

        public static Vector3 xMirror => mirrorOneVector;

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="y">Y component of the vector.</param>
        /// <param name="z">Z component of the vector.</param>
        public Vector3(Fix64 x, Fix64 y, Fix64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="xy">X and Y components of the vector.</param>
        /// <param name="z">Z component of the vector.</param>
        public Vector3(Vector2 xy, Fix64 z)
        {
            this.x = xy.x;
            this.y = xy.y;
            this.z = z;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="yz">Y and Z components of the vector.</param>
        public Vector3(Fix64 x, Vector2 yz)
        {
            this.x = x;
            this.y = yz.x;
            this.z = yz.y;
        }

        /// <summary>
        /// Computes an interpolated state between two vectors.
        /// </summary>
        /// <param name="start">Starting location of the interpolation.</param>
        /// <param name="end">Ending location of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end location to use.</param>
        /// <returns>Interpolated intermediate state.</returns>
        public static Vector3 Lerp(Vector3 start, Vector3 end, Fix64 interpolationAmount)
        {
            Vector3 toReturn;
            Lerp(ref start, ref end, interpolationAmount, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Normalizes the vector.
        /// </summary>
        public void Normalize()
        {
            Fix64 inverse = Fix64.C1 / Fix64.Sqrt(x * x + y * y + z * z);
            x *= inverse;
            y *= inverse;
            z *= inverse;
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">First vector in the product.</param>
        /// <param name="b">Second vector in the product.</param>
        /// <returns>Resulting dot product.</returns>
        public static Fix64 Dot(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Computes the distance between two two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Distance between the two vectors.</returns>
        public static Fix64 Distance(Vector3 a, Vector3 b)
        {
            Fix64 toReturn;
            Distance(ref a, ref b, out toReturn);
            return toReturn;
        }

        /// <summary>
        ///   <para>Returns the angle in degrees between from and to.</para>
        /// </summary>
        /// <param name="from">The vector from which the angular difference is measured.</param>
        /// <param name="to">The vector to which the angular difference is measured.</param>
        /// <returns>
        ///   <para>The angle in degrees between the two vectors.</para>
        /// </returns>
        public static Fix64 Angle(Vector3 from, Vector3 to)
        {
            return FPUtility.Acos(FPUtility.Clamp(Dot(from.normalized, to.normalized), Fix64.MinusOne, Fix64.One)) * Fix64.Rad2Deg;
        }

        /// <summary>
        ///   <para>Returns the signed angle in degrees between from and to.</para>
        /// </summary>
        /// <param name="from">The vector from which the angular difference is measured.</param>
        /// <param name="to">The vector to which the angular difference is measured.</param>
        /// <param name="axis">A vector around which the other vectors are rotated.</param>
        public static Fix64 SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            Vector3 fromNorm = from.normalized, toNorm = to.normalized;
            Fix64 unsignedAngle = FPUtility.Acos(FPUtility.Clamp(Dot(fromNorm, toNorm), Fix64.MinusOne, Fix64.One)) * Fix64.Rad2Deg;
            Fix64 sign = FPUtility.Sign(Dot(axis, Cross(fromNorm, toNorm)));
            return unsignedAngle * sign;
        }

        public static implicit operator UnityEngine.Vector3(Vector3 value) => new UnityEngine.Vector3(value.x, value.y, value.z);

        public static explicit operator Vector3(UnityEngine.Vector3 value) => new Vector3((Fix64)value.x, (Fix64)value.y, (Fix64)value.z);

        /// <summary>
        /// Gets a string representation of the vector.
        /// </summary>
        /// <returns>String representing the vector.</returns>
        public override string ToString()
        {
            return "{" + x + ", " + y + ", " + z + "}";
        }

        /// <summary>
        /// Computes the squared length of the vector.
        /// </summary>
        /// <returns>Squared length of the vector.</returns>
        public Fix64 LengthSquared()
        {
            return x * x + y * y + z * z;
        }

        /// <summary>
        /// Computes the length of the vector.
        /// </summary>
        /// <returns>Length of the vector.</returns>
        public Fix64 Length()
        {
            return Fix64.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">First vector in the product.</param>
        /// <param name="b">Second vector in the product.</param>
        /// <param name="product">Resulting dot product.</param>
        public static void Dot(ref Vector3 a, ref Vector3 b, out Fix64 product)
        {
            product = a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <param name="sum">Sum of the two vectors.</param>
        public static void Add(ref Vector3 a, ref Vector3 b, out Vector3 sum)
        {
            sum.x = a.x + b.x;
            sum.y = a.y + b.y;
            sum.z = a.z + b.z;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to subtract from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <param name="difference">Result of the subtraction.</param>
        public static void Subtract(ref Vector3 a, ref Vector3 b, out Vector3 difference)
        {
            difference.x = a.x - b.x;
            difference.y = a.y - b.y;
            difference.z = a.z - b.z;
        }

        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="scale">Amount to scale.</param>
        /// <param name="result">Scaled vector.</param>
        public static void Multiply(ref Vector3 v, Fix64 scale, out Vector3 result)
        {
            result.x = v.x * scale;
            result.y = v.y * scale;
            result.z = v.z * scale;
        }

        /// <summary>
        /// Multiplies two vectors on a per-component basis.
        /// </summary>
        /// <param name="a">First vector to multiply.</param>
        /// <param name="b">Second vector to multiply.</param>
        /// <param name="result">Result of the componentwise multiplication.</param>
        public static void Multiply(ref Vector3 a, ref Vector3 b, out Vector3 result)
        {
            result.x = a.x * b.x;
            result.y = a.y * b.y;
            result.z = a.z * b.z;
        }

        /// <summary>
        /// Divides a vector's components by some amount.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="divisor">Value to divide the vector's components.</param>
        /// <param name="result">Result of the division.</param>
        public static void Divide(ref Vector3 v, Fix64 divisor, out Vector3 result)
        {
            Fix64 inverse = Fix64.C1 / divisor;
            result.x = v.x * inverse;
            result.y = v.y * inverse;
            result.z = v.z * inverse;
        }

        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="f">Amount to scale.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector3 operator *(Vector3 v, Fix64 f)
        {
            Vector3 toReturn;
            toReturn.x = v.x * f;
            toReturn.y = v.y * f;
            toReturn.z = v.z * f;
            return toReturn;
        }

        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="f">Amount to scale.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector3 operator *(Fix64 f, Vector3 v)
        {
            Vector3 toReturn;
            toReturn.x = v.x * f;
            toReturn.y = v.y * f;
            toReturn.z = v.z * f;
            return toReturn;
        }

        /// <summary>
        /// Multiplies two vectors on a per-component basis.
        /// </summary>
        /// <param name="a">First vector to multiply.</param>
        /// <param name="b">Second vector to multiply.</param>
        /// <returns>Result of the componentwise multiplication.</returns>
        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            Vector3 result;
            Multiply(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// Divides a vector's components by some amount.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="f">Value to divide the vector's components.</param>
        /// <returns>Result of the division.</returns>
        public static Vector3 operator /(Vector3 v, Fix64 f)
        {
            Vector3 toReturn;
            f = Fix64.C1 / f;
            toReturn.x = v.x * f;
            toReturn.y = v.y * f;
            toReturn.z = v.z * f;
            return toReturn;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to subtract from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <returns>Result of the subtraction.</returns>
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            Vector3 v;
            v.x = a.x - b.x;
            v.y = a.y - b.y;
            v.z = a.z - b.z;
            return v;
        }

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <returns>Sum of the two vectors.</returns>
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            Vector3 v;
            v.x = a.x + b.x;
            v.y = a.y + b.y;
            v.z = a.z + b.z;
            return v;
        }

        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <returns>Negated vector.</returns>
        public static Vector3 operator -(Vector3 v)
        {
            v.x = -v.x;
            v.y = -v.y;
            v.z = -v.z;
            return v;
        }

        /// <summary>
        /// Tests two vectors for componentwise equivalence.
        /// </summary>
        /// <param name="a">First vector to test for equivalence.</param>
        /// <param name="b">Second vector to test for equivalence.</param>
        /// <returns>Whether the vectors were equivalent.</returns>
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        /// <summary>
        /// Tests two vectors for componentwise inequivalence.
        /// </summary>
        /// <param name="a">First vector to test for inequivalence.</param>
        /// <param name="b">Second vector to test for inequivalence.</param>
        /// <returns>Whether the vectors were inequivalent.</returns>
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Vector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
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
            if (obj is Vector3)
            {
                return Equals((Vector3)obj);
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
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
        }

        /// <summary>
        /// Computes the squared distance between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="distanceSquared">Squared distance between the two vectors.</param>
        public static void DistanceSquared(ref Vector3 a, ref Vector3 b, out Fix64 distanceSquared)
        {
            Fix64 x = a.x - b.x;
            Fix64 y = a.y - b.y;
            Fix64 z = a.z - b.z;
            distanceSquared = x * x + y * y + z * z;
        }

        /// <summary>
        /// Computes the squared distance between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Squared distance between the two vectors.</returns>
        public static Fix64 DistanceSquared(Vector3 a, Vector3 b)
        {
            Fix64 x = a.x - b.x;
            Fix64 y = a.y - b.y;
            Fix64 z = a.z - b.z;
            return x * x + y * y + z * z;
        }

        /// <summary>
        /// Computes the distance between two two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="distance">Distance between the two vectors.</param>
        public static void Distance(ref Vector3 a, ref Vector3 b, out Fix64 distance)
        {
            Fix64 x = a.x - b.x;
            Fix64 y = a.y - b.y;
            Fix64 z = a.z - b.z;
            distance = Fix64.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Computes the cross product between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Cross product of the two vectors.</returns>
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            Vector3 toReturn;
            Vector3.Cross(ref a, ref b, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Computes the cross product between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="result">Cross product of the two vectors.</param>
        public static void Cross(ref Vector3 a, ref Vector3 b, out Vector3 result)
        {
            Fix64 resultX = a.y * b.z - a.z * b.y;
            Fix64 resultY = a.z * b.x - a.x * b.z;
            Fix64 resultZ = a.x * b.y - a.y * b.x;
            result.x = resultX;
            result.y = resultY;
            result.z = resultZ;
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="v">Vector to normalize.</param>
        /// <returns>Normalized vector.</returns>
        public static Vector3 Normalize(Vector3 v)
        {
            Vector3 toReturn;
            Vector3.Normalize(ref v, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="v">Vector to normalize.</param>
        /// <param name="result">Normalized vector.</param>
        public static void Normalize(ref Vector3 v, out Vector3 result)
        {
            Fix64 inverse = Fix64.C1 / Fix64.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            result.x = v.x * inverse;
            result.y = v.y * inverse;
            result.z = v.z * inverse;
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <param name="negated">Negated vector.</param>
        public static void Negate(ref Vector3 v, out Vector3 negated)
        {
            negated.x = -v.x;
            negated.y = -v.y;
            negated.z = -v.z;
        }

        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <param name="result">Vector with nonnegative elements.</param>
        public static void Abs(ref Vector3 v, out Vector3 result)
        {
            if (v.x < Fix64.C0)
                result.x = -v.x;
            else
                result.x = v.x;
            if (v.y < Fix64.C0)
                result.y = -v.y;
            else
                result.y = v.y;
            if (v.z < Fix64.C0)
                result.z = -v.z;
            else
                result.z = v.z;
        }

        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <returns>Vector with nonnegative elements.</returns>
        public static Vector3 Abs(Vector3 v)
        {
            Vector3 result;
            Abs(ref v, out result);
            return result;
        }

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <param name="min">Vector containing the lesser values of each vector.</param>
        public static void Min(ref Vector3 a, ref Vector3 b, out Vector3 min)
        {
            min.x = a.x < b.x ? a.x : b.x;
            min.y = a.y < b.y ? a.y : b.y;
            min.z = a.z < b.z ? a.z : b.z;
        }

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the lesser values of each vector.</returns>
        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            Vector3 result;
            Min(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <param name="max">Vector containing the greater values of each vector.</param>
        public static void Max(ref Vector3 a, ref Vector3 b, out Vector3 max)
        {
            max.x = a.x > b.x ? a.x : b.x;
            max.y = a.y > b.y ? a.y : b.y;
            max.z = a.z > b.z ? a.z : b.z;
        }

        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the greater values of each vector.</returns>
        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            Vector3 result;
            Max(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// Computes an interpolated state between two vectors.
        /// </summary>
        /// <param name="start">Starting location of the interpolation.</param>
        /// <param name="end">Ending location of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end location to use.</param>
        /// <param name="result">Interpolated intermediate state.</param>
        public static void Lerp(ref Vector3 start, ref Vector3 end, Fix64 interpolationAmount, out Vector3 result)
        {
            interpolationAmount = FPUtility.Clamp01(interpolationAmount);
            Fix64 startAmount = Fix64.C1 - interpolationAmount;
            result.x = start.x * startAmount + end.x * interpolationAmount;
            result.y = start.y * startAmount + end.y * interpolationAmount;
            result.z = start.z * startAmount + end.z * interpolationAmount;
        }

        /// <summary>
        /// Computes an intermediate location using hermite interpolation.
        /// </summary>
        /// <param name="value1">First position.</param>
        /// <param name="tangent1">Tangent associated with the first position.</param>
        /// <param name="value2">Second position.</param>
        /// <param name="tangent2">Tangent associated with the second position.</param>
        /// <param name="interpolationAmount">Amount of the second point to use.</param>
        /// <param name="result">Interpolated intermediate state.</param>
        public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, Fix64 interpolationAmount, out Vector3 result)
        {
            Fix64 weightSquared = interpolationAmount * interpolationAmount;
            Fix64 weightCubed = interpolationAmount * weightSquared;
            Fix64 value1Blend = Fix64.C2 * weightCubed - Fix64.C3 * weightSquared + Fix64.C1;
            Fix64 tangent1Blend = weightCubed - Fix64.C2 * weightSquared + interpolationAmount;
            Fix64 value2Blend = -2 * weightCubed + Fix64.C3 * weightSquared;
            Fix64 tangent2Blend = weightCubed - weightSquared;
            result.x = value1.x * value1Blend + value2.x * value2Blend + tangent1.x * tangent1Blend + tangent2.x * tangent2Blend;
            result.y = value1.y * value1Blend + value2.y * value2Blend + tangent1.y * tangent1Blend + tangent2.y * tangent2Blend;
            result.z = value1.z * value1Blend + value2.z * value2Blend + tangent1.z * tangent1Blend + tangent2.z * tangent2Blend;
        }

        /// <summary>
        /// Computes an intermediate location using hermite interpolation.
        /// </summary>
        /// <param name="value1">First position.</param>
        /// <param name="tangent1">Tangent associated with the first position.</param>
        /// <param name="value2">Second position.</param>
        /// <param name="tangent2">Tangent associated with the second position.</param>
        /// <param name="interpolationAmount">Amount of the second point to use.</param>
        /// <returns>Interpolated intermediate state.</returns>
        public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, Fix64 interpolationAmount)
        {
            Vector3 toReturn;
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, interpolationAmount, out toReturn);
            return toReturn;
        }
    }
}