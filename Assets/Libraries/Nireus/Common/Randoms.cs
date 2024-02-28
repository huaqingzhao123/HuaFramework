using System;

namespace Nireus
{
    class LinearRandom
    {
        private const long m = 4294967296; // aka 2^32
        private const long a = 1664525;
        private const long c = 1013904223;
        private long _last;

        public LinearRandom()
        {
            _last = DateTime.Now.Ticks % m;
        }

        public LinearRandom(long seed)
        {
            _last = seed;
        }

        public long Next()
        {
            _last = ((a * _last) + c) % m;

            return _last;
        }

        public long Next(long maxValue)
        {
            return Next() % maxValue;
        }
    }

    /// <summary>
    /// Implementation of Mersenne Twister random number generator
    /// </summary>
    public class MtRandom
    {
        private readonly uint[] _matrix = new uint[624];
        private int _index = 0;

        public MtRandom() : this((uint)(0xFFFFFFFF & DateTime.Now.Ticks)) { }

        /// <summary>
        /// Initializes a new instance of the MersennePrimeRNG with a seed
        /// </summary>
        /// <param name="seed"></param>
        public MtRandom(uint seed)
        {
            _matrix[0] = seed;
            for (int i = 1; i < _matrix.Length; i++)
                _matrix[i] = (1812433253 * (_matrix[i - 1] ^ ((_matrix[i - 1]) >> 30) + 1));
        }

        /// <summary>
        /// Generates a new matrix table
        /// </summary>
        private void Generate()
        {
            for (int i = 0; i < _matrix.Length; i++)
            {
                uint y = (_matrix[i] >> 31) + ((_matrix[(i + 1) & 623]) << 1);
                _matrix[i] = _matrix[(i + 397) & 623] ^ (y >> 1);
                if (y % 2 != 0)
                    _matrix[i] = (_matrix[i] ^ (2567483615));
            }
        }

        /// <summary>
        /// Generates and returns a random number
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            if (_index == 0)
                Generate();

            uint y = _matrix[_index];
            y = y ^ (y >> 11);
            y = (y ^ (y << 7) & (2636928640));
            y = (y ^ (y << 15) & (4022730752));
            y = (y ^ (y >> 18));

            _index = (_index + 1) % 623;
            return (int)(y % int.MaxValue);
        }

        /// <summary>
        /// Generates and returns a random number
        /// </summary>
        /// <param name="max">The highest value that can be returned</param>
        /// <returns></returns>
        public int Next(int max)
        {
            var randomValue = Next();
            return randomValue % max;
        }

        /// <summary>
        /// Generates and returns a random number
        /// </summary>
        /// <param name="min">The lowest value returned</param>
        /// <param name="max">The highest value returned</param>
        /// <returns></returns>
        public int Next(int min, int max)
        {
            if (min > max)
                throw new ArgumentException("min cannot be greater than max", "min");
            return min + Next(min - max);
        }
    }

    public class Xoshiro256PlusRandom : Random
    {
        static ulong NextSeed(ref ulong x)
        {
            ulong z = (x += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        static ulong RotateLeft(ulong x, int k)
        {
            // Note. RyuJIT will compile this to a single rotate CPU instruction (as of about .NET 4.6.1 and dotnet core 2.0).
            return (x << k) | (x >> (64 - k));
        }

        // RNG state.
        ulong _s0;
        ulong _s1;
        ulong _s2;
        ulong _s3;

        public Xoshiro256PlusRandom()
        {
            var ticks = (ulong)DateTime.UtcNow.Ticks;
            Reinitialise(NextSeed(ref ticks));
        }

        /// <summary>
        /// Initialises a new instance with the provided ulong seed.
        /// </summary>
        public Xoshiro256PlusRandom(ulong seed)
        {
            Reinitialise(seed);
        }

        /// <summary>
        /// Re-initialises the random number generator state using the provided seed.
        /// </summary>
        public void Reinitialise(ulong seed)
        {
            // Notes.
            // The first random sample will be very strongly correlated to the value we give to the 
            // state variables here; such a correlation is undesirable, therefore we significantly 
            // weaken it by hashing the seed's bits using the splitmix64 PRNG.
            //
            // It is required that at least one of the state variables be non-zero;
            // use of splitmix64 satisfies this requirement because it is an equidistributed generator,
            // thus if it outputs a zero it will next produce a zero after a further 2^64 outputs.

            // Use the splitmix64 RNG to hash the seed.
            _s0 = NextSeed(ref seed);
            _s1 = NextSeed(ref seed);
            _s2 = NextSeed(ref seed);
            _s3 = NextSeed(ref seed);
        }

        // Constants.
        const double INCR_DOUBLE = 1.0 / (1UL << 53);
        const float INCR_FLOAT = 1f / (1U << 24);

        #region Public Methods [System.Random functionally equivalent methods]

        /// <summary>
        /// Generate a random Int32 over the interval [0, Int32.MaxValue), i.e. exclusive of Int32.MaxValue.
        /// </summary>
        /// <remarks>
        /// Int32.MaxValue is excluded in order to be functionally equivalent with System.Random.Next().
        /// 
        /// For slightly improved performance consider these alternatives:
        /// 
        ///  * NextInt() returns an Int32 over the interval [0 to Int32.MaxValue], i.e. inclusive of Int32.MaxValue.
        /// 
        ///  * NextUInt(). Cast the result to an Int32 to generate an value over the full range of an Int32,
        ///    including negative values.
        /// </remarks>
        public override int Next()
        {
            // Perform rejection sampling to handle the special case where the value int.MaxValue is generated;
            // this value is outside the range of permitted values for this method. 
            // Rejection sampling ensures we produce an unbiased sample.
            ulong rtn;
            do
            {
                rtn = NextULongInner() & 0x7fffffffUL;
            }
            while (rtn == 0x7fffffffUL);

            return (int)rtn;
        }

        /// <summary>
        /// Generate a random Int32 over the interval [0 to maxValue), i.e. excluding maxValue.
        /// </summary>
        public override int Next(int maxValue)
        {
            if (maxValue < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "maxValue must be > 0");
            }

            if (1 == maxValue)
            {
                return 0;
            }

            return NextInner(maxValue);
        }

        /// <summary>
        /// Generate a random Int32 over the interval [minValue, maxValue), i.e. excluding maxValue.
        /// maxValue must be >= minValue. minValue may be negative.
        /// </summary>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "maxValue must be > minValue");
            }

            long range = (long)maxValue - minValue;
            if (range <= int.MaxValue)
            {
                return NextInner((int)range) + minValue;
            }

            // Call NextInner(long); i.e. the range is greater than int.MaxValue.
            return (int)(NextInner(range) + minValue);
        }

        /// <summary>
        /// Generate a random double over the interval [0, 1), i.e. inclusive of 0.0 and exclusive of 1.0.
        /// </summary>
        public override double NextDouble()
        {
            return NextDoubleInner();
        }

        /// <summary>
        /// Generate a random double over the interval [0, 1), i.e. inclusive of 0.0 and exclusive of 1.0.
        /// </summary>
        /// <remarks>
        /// Uses an alternative sampling method that is capable of generating all possible values in the
        /// interval [0,1) that can be represented by a double precision float. Note however that this method 
        /// is significantly slower than NextDouble().
        /// </remarks>
        public double NextDoubleHighRes()
        {
            // Notes.
            // An alternative sampling method from:
            // 
            //    2014, Taylor R Campbell
            //
            //    Uniform random floats:  How to generate a double-precision
            //    floating-point number in [0, 1] uniformly at random given a uniform
            //    random source of bits.
            //
            //    https://mumble.net/~campbell/tmp/random_real.c
            //
            // The basic idea is that we generate a string of binary digits and use them to construct a 
            // base two number of the form:
            //
            //    0.{digits}
            //
            // The digits are generated in blocks of 64 bits. If all 64 bits in a block are zero then a 
            // running exponent value is reduced by 64 and another 64 bits are generated. This process is
            // repeated until a block with non-zero bits is produced, or the exponent value falls below -1074.
            //
            // The final step is to create the IEE754 double precision variable from a 64 bit significand
            // (the most recent and thus significant 64 bits), and the running exponent.
            //
            // This scheme is capable of generating all possible values in the interval [0,1) that can be 
            // represented by a double precision float, and without bias. There are a little under 2^62
            // possible discrete values, and this compares to the 2^53 possible values than can be generated by 
            // NextDouble(), however the scheme used in this method is much slower, so is likely of interest
            // in specialist scenarios.

            int exponent = -64;
            ulong significand;
            int shift;

            // Read zeros into the exponent until we hit a one; the rest
            // will go into the significand.
            while ((significand = NextULongInner()) == 0)
            {
                exponent -= 64;

                // If the exponent falls below -1074 = emin + 1 - p,
                // the exponent of the smallest subnormal, we are
                // guaranteed the result will be rounded to zero.  This
                // case is so unlikely it will happen in realistic
                // terms only if random64 is broken.
                if (exponent < -1074)
                    return 0;
            }

            // There is a 1 somewhere in significand, not necessarily in
            // the most significant position.  If there are leading zeros,
            // shift them into the exponent and refill the less-significant
            // bits of the significand.  Can't predict one way or another
            // whether there are leading zeros: there's a fifty-fifty
            // chance, if random64 is uniformly distributed.
            shift = MathUtil.LeadingZeroCount(significand);
            if (shift != 0)
            {
                exponent -= shift;
                significand <<= shift;
                significand |= (NextULongInner() >> (64 - shift));
            }

            // Set the sticky bit, since there is almost surely another 1
            // in the bit stream.  Otherwise, we might round what looks
            // like a tie to even when, almost surely, were we to look
            // further in the bit stream, there would be a 1 breaking the
            // tie.
            significand |= 1;

            // Finally, convert to double (rounding) and scale by
            // 2^exponent.
            return (double)significand * Math.Pow(2, exponent);
        }

        #endregion

        #region Public Methods [Methods not present on System.Random]

        /// <summary>
        /// Generate a random float over the interval [0, 1), i.e. inclusive of 0.0 and exclusive of 1.0.
        /// </summary>
        public float NextFloat()
        {
            // Note. Here we generate a random integer between 0 and 2^24-1 (i.e. 24 binary 1s) and multiply
            // by the fractional unit value 1.0 / 2^24, thus the result has a max value of
            // 1.0 - (1.0 / 2^24). Or 0.99999994 in decimal.
            return (NextULongInner() >> 40) * INCR_FLOAT;
        }

        /// <summary>
        /// Generate a random UInt32 over the interval [0, 2^32-1], i.e. over the full range of a UInt32.
        /// </summary>
        public uint NextUInt()
        {
            return (uint)NextULongInner();
        }

        /// <summary>
        /// Generate a random Int32 over interval [0 to 2^31-1], i.e. inclusive of Int32.MaxValue and therefore 
        /// over the full range of non-negative Int32(s).
        /// </summary>
        /// <remarks>
        /// This method can generate Int32.MaxValue, whereas Next() does not; this is the only difference
        /// between these two methods. As a consequence this method will typically be slightly faster because 
        /// Next() must test for Int32.MaxValue and resample the underlying RNG when that value occurs.
        /// </remarks>
        public int NextInt()
        {
            // Generate 64 random bits and shift right to leave the most significant 31 bits.
            // Bit 32 is the sign bit so must be zero to avoid negative results.
            // Note. Shift right is used instead of a mask because the high significant bits 
            // exhibit higher quality randomness compared to the lower bits.
            return (int)(NextULongInner() >> 33);
        }

        /// <summary>
        /// Generate a random UInt64 over the interval [0, 2^64-1], i.e. over the full range of a UInt64.
        /// </summary>
        public ulong NextULong()
        {
            return NextULongInner();
        }

        /// <summary>
        /// Generate a random double over the interval (0, 1), i.e. exclusive of both 0.0 and 1.0
        /// </summary>
        public double NextDoubleNonZero()
        {
            // Here we generate a random value in the interval [0, 0x1f_ffff_ffff_fffe], and add one
            // to generate a random value in the interval [1, 0x1f_ffff_ffff_ffff].
            //
            // We then multiply by the fractional unit 1.0 / 2^53 to obtain a floating point value 
            // in the interval [ 1/(2^53-1) , 1.0].
            //
            // Note. the bit shift right here may appear redundant, however, the high significance
            // bits have better randomness than the low bits, thus this approach is preferred.
            return ((NextULongInner() >> 11) & 0x1ffffffffffffe) * INCR_DOUBLE;
        }

        /// <summary>
        /// Generate a single random bit.
        /// </summary>
        public bool NextBool()
        {
            // Use a high bit since the low bits are linear-feedback shift registers (LFSRs) with low degree.
            // This is slower than the approach of generating and caching 64 bits for future calls, but 
            // (A) gives good quality randomness, and (B) is still very fast.
            return (NextULongInner() & 0x8000000000000000) == 0;
        }

        /// <summary>
        /// Generate a single random byte over the interval [0,255].
        /// </summary>
        public byte NextByte()
        {
            // Note. Here we shift right to use the 8 most significant bits because these exhibit higher quality
            // randomness than the lower bits.
            return (byte)(NextULongInner() >> 56);
        }

        #endregion

        #region Private Methods

        private int NextInner(int maxValue)
        {
            if (1 == maxValue)
            {
                return 0;
            }

            // Notes.
            // Here we sample an integer value within the interval [0, maxValue). Rejection sampling is used in 
            // order to produce unbiased samples. An alternative approach is:
            //
            //  return (int)(NextDoubleInner() * maxValue);
            //
            // I.e. generate a double precision float in the interval [0,1) and multiply by maxValue. However the
            // use of floating point arithmetic will introduce bias therefore this method is not used.
            //
            // The rejection sampling method used here operates as follows:
            //
            //  1) Calculate N such that  2^(N-1) < maxValue <= 2^N, i.e. N is the minimum number of bits required
            //     to represent maxValue states.
            //  2) Generate an N bit random sample.
            //  3) Reject samples that are >= maxValue, and goto (2) to resample.
            //
            // Repeat until a valid sample is generated.

            // Log2Ceiling(numberOfStates) gives the number of bits required to represent maxValue states.
            int bitCount = MathUtil.Log2Ceiling((uint)maxValue);

            // Rejection sampling loop.
            // Note. The expected number of samples per generated value is approx. 1.3862,
            // i.e. the number of loops, on average, assuming a random and uniformly distributed maxValue.
            int x;
            do
            {
                x = (int)(NextULongInner() >> (64 - bitCount));
            }
            while (x >= maxValue);

            return x;
        }

        private long NextInner(long maxValue)
        {
            if (1 == maxValue)
            {
                return 0;
            }

            // See comments on NextInner(int).

            // Log2Ceiling(numberOfStates) gives the number of bits required to represent maxValue states.
            int bitCount = MathUtil.Log2Ceiling((ulong)maxValue);

            // Rejection sampling loop.
            long x;
            do
            {
                x = (long)(NextULongInner() >> (64 - bitCount));
            }
            while (x >= maxValue);

            return x;
        }

        private double NextDoubleInner()
        {
            // Notes. 
            // Here we generate a random integer in the interval [0, 2^53-1]  (i.e. the max value is 53 binary 1s),
            // and multiply by the fractional value 1.0 / 2^53, thus the result has a min value of 0.0 and a max value of 
            // 1.0 - (1.0 / 2^53), or 0.99999999999999989 in decimal.
            //
            // I.e. we break the interval [0,1) into 2^53 uniformly distributed discrete values, and thus the interval between
            // two adjacent values is 1.0 / 2^53. This increment is chosen because it is the smallest value at which each 
            // distinct value in the full range (from 0.0 to 1.0 exclusive) can be represented directly by a double precision
            // float, and thus no rounding occurs in the representation of these values, which in turn ensures no bias in the 
            // random samples.
            return (NextULongInner() >> 11) * INCR_DOUBLE;
        }

        #endregion

        #region Abstract Methods

        protected ulong NextULongInner()
        {
            ulong s0 = _s0;
            ulong s1 = _s1;
            ulong s2 = _s2;
            ulong s3 = _s3;

            ulong result = s0 + s3;

            ulong t = s1 << 17;

            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;

            s2 ^= t;

            s3 = RotateLeft(s3, 45);

            _s0 = s0;
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;

            return result;
        }

        #endregion
    }

    public static class ZigguratGaussian
    {
        #region Consts

        /// <summary>
        /// Number of blocks.
        /// </summary>
        const int __blockCount = 128;

        /// <summary>
        /// Right hand x coord of the base rectangle, thus also the left hand x coord of the tail 
        /// (pre-determined/computed for 128 blocks).
        /// </summary>
        const double __R = 3.442619855899;

        /// <summary>
        /// Area of each rectangle (pre-determined/computed for 128 blocks).
        /// </summary>
        const double __A = 9.91256303526217e-3;

        /// <summary>
        /// Denominator for __INCR constant. This is the number of distinct values this class is capable 
        /// of generating in the interval [0,1], i.e. (2^53)-1 distinct values.
        /// </summary>
        const ulong __MAXINT = (1UL << 53) - 1;

        /// <summary>
        /// Scale factor for converting a ULong with interval [0, 0x1f_ffff_ffff_ffff] to a double with interval [0,1].
        /// </summary>
        const double __INCR = 1.0 / __MAXINT;

        #endregion

        #region Static Fields

        // __x[i] and __y[i] describe the top-right position of rectangle i.
        static readonly double[] __x;
        static readonly double[] __y;

        // The proportion of each segment that is entirely within the distribution, expressed as ulong where 
        // a value of 0 indicates 0% and 2^53-1 (i.e. 53 binary 1s) 100%. Expressing this as an integer value 
        // allows some floating point operations to be replaced with integer operations.
        static readonly ulong[] __xComp;

        // Useful precomputed values.
        // Area A divided by the height of B0. Note. This is *not* the same as __x[i] because the area 
        // of B0 is __A minus the area of the distribution tail.
        static readonly double __A_Div_Y0;

        #endregion

        #region Static Initialiser

        static ZigguratGaussian()
        {
            // Initialise rectangle position data. 
            // __x[i] and __y[i] describe the top-right position of Box i.

            // Allocate storage. We add one to the length of _x so that we have an entry at __x[__blockCount], this avoids having 
            // to do a special case test when sampling from the top box.
            __x = new double[__blockCount + 1];
            __y = new double[__blockCount];

            // Determine top right position of the base rectangle/box (the rectangle with the Gaussian tale attached). 
            // We call this Box 0 or B0 for short.
            // Note. x[0] also describes the right-hand edge of B1. (See diagram).
            __x[0] = __R;
            __y[0] = GaussianPdfDenorm(__R);

            // The next box (B1) has a right hand X edge the same as B0. 
            // Note. B1's height is the box area divided by its width, hence B1 has a smaller height than B0 because
            // B0's total area includes the attached distribution tail.
            __x[1] = __R;
            __y[1] = __y[0] + (__A / __x[1]);

            // Calc positions of all remaining rectangles.
            for (int i = 2; i < __blockCount; i++)
            {
                __x[i] = GaussianPdfDenormInv(__y[i - 1]);
                __y[i] = __y[i - 1] + (__A / __x[i]);
            }

            // For completeness we define the right-hand edge of a notional box 6 as being zero (a box with no area).
            __x[__blockCount] = 0.0;

            // Useful precomputed values.
            __A_Div_Y0 = __A / __y[0];
            __xComp = new ulong[__blockCount];

            // Special case for base box. __xComp[0] stores the area of B0 as a proportion of __R 
            // (recalling that all segments have area __A, but that the base segment is the combination of B0 and the distribution tail).
            // Thus __xComp[0] is the probability that a sample point is within the box part of the segment.
            __xComp[0] = (ulong)(((__R * __y[0]) / __A) * (double)__MAXINT);

            for (int i = 1; i < __blockCount - 1; i++)
            {
                __xComp[i] = (ulong)((__x[i + 1] / __x[i]) * (double)__MAXINT);
            }
            __xComp[__blockCount - 1] = 0;  // Shown for completeness.
        }

        #endregion

        #region Public Static Methods

        static Xoshiro256PlusRandom DefaultRnd = new Xoshiro256PlusRandom();

        /// <summary>
        /// Take a sample from the standard Gaussian distribution, i.e. with mean of 0 and standard deviation of 1.
        /// </summary>
        /// <returns>A random sample.</returns>
        public static double Sample(Xoshiro256PlusRandom rnd = null)
        {
            rnd = rnd ?? DefaultRnd;

            for (; ; )
            {
                // Generate 64 random bits.
                ulong u = rnd.NextULong();

                // Notes. We require 61 of the random bits in total so we discard the lowest three bits because these
                // generally exhibit lower quality randomness than the higher bits (depending on the PRNG is use, but
                // it is a common feature of many PRNGs).

                // Select a segment (7 bits, bits 3 to 9).
                int s = (int)((u >> 3) & 0x7f);

                // Select sign bit (bit 10).
                double sign = ((u & 0x400) == 0) ? 1.0 : -1.0;

                // Get a uniform random value with interval [0, 2^53-1], or in hexadecimal [0, 0x1f_ffff_ffff_ffff] 
                // (i.e. a random 53 bit number) (bits 11 to 63).
                ulong u2 = u >> 11;

                // Special case for the base segment.
                if (0 == s)
                {
                    if (u2 < __xComp[0])
                    {
                        // Generated x is within R0.
                        return u2 * __INCR * __A_Div_Y0 * sign;
                    }
                    // Generated x is in the tail of the distribution.
                    return SampleTail(rnd) * sign;
                }

                // All other segments.
                if (u2 < __xComp[s])
                {
                    // Generated x is within the rectangle.
                    return u2 * __INCR * __x[s] * sign;
                }

                // Generated x is outside of the rectangle.
                // Generate a random y coordinate and test if our (x,y) is within the distribution curve.
                // This execution path is relatively slow/expensive (makes a call to Math.Exp()) but is relatively rarely executed,
                // although more often than the 'tail' path (above).
                double x = u2 * __INCR * __x[s];
                if (__y[s - 1] + ((__y[s] - __y[s - 1]) * rnd.NextDouble()) < GaussianPdfDenorm(x))
                {
                    return x * sign;
                }
            }
        }

        /// <summary>
        /// Take a sample from the a Gaussian distribution with the specified mean and standard deviation.
        /// </summary>
        /// <param name="rng">Random source.</param>
        /// <param name="mean">Distribution mean.</param>
        /// <param name="stdDev">Distribution standard deviation.</param>
        /// <returns>A random sample.</returns>
        public static double Sample(double mean, double stdDev, Xoshiro256PlusRandom rnd = null)
        {
            rnd = rnd ?? DefaultRnd;

            return mean + (Sample(rnd) * stdDev);
        }

        /// <summary>
        /// Fill an array with samples from the standard Gaussian distribution, i.e. with mean of 0 and standard deviation of 1.
        /// </summary>
        /// <param name="rng">Random source.</param>
        /// <param name="buf">The array to fill with samples.</param>
        public static void Sample(double[] buf, Xoshiro256PlusRandom rnd = null)
        {
            rnd = rnd ?? DefaultRnd;

            for (int i = 0; i <= buf.Length; i++)
            {
                buf[i] = Sample(rnd);
            }
        }

        /// <summary>
        /// Fill an array with samples from a Gaussian distribution with the specified mean and standard deviation.
        /// </summary>
        /// <param name="rng">Random source.</param>
        /// <param name="mean">Distribution mean.</param>
        /// <param name="stdDev">Distribution standard deviation.</param>
        /// <param name="buf">The array to fill with samples.</param>
        public static void Sample(double mean, double stdDev, double[] buf, Xoshiro256PlusRandom rnd = null)
        {
            rnd = rnd ?? DefaultRnd;

            for (int i = 0; i <= buf.Length; i++)
            {
                buf[i] = mean + (Sample(rnd) * stdDev);
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Sample from the distribution tail (defined as having x >= __R).
        /// </summary>
        /// <returns></returns>
        private static double SampleTail(Xoshiro256PlusRandom rnd)
        {
            double x, y;
            do
            {
                // Note. we use NextDoubleNonZero() because Log(0) returns NaN and will also tend to be a very slow execution path (when it occurs, which is rarely).
                x = -Math.Log(rnd.NextDoubleNonZero()) / __R;
                y = -Math.Log(rnd.NextDoubleNonZero());
            }
            while (y + y < x * x);
            return __R + x;
        }

        /// <summary>
        /// Gaussian probability density function, denormalised, that is, y = e^-(x^2/2).
        /// </summary>
        private static double GaussianPdfDenorm(double x)
        {
            return Math.Exp(-(x * x / 2.0));
        }

        /// <summary>
        /// Inverse function of GaussianPdfDenorm(x)
        /// </summary>
        private static double GaussianPdfDenormInv(double y)
        {
            // Operates over the y interval (0,1], which happens to be the y interval of the pdf, 
            // with the exception that it does not include y=0, but we would never call with 
            // y=0 so it doesn't matter. Note that a Gaussian effectively has a tail going
            // into infinity on the x-axis, hence asking what is x when y=0 is an invalid question
            // in the context of this class.
            return Math.Sqrt(-2.0 * Math.Log(y));
        }

        #endregion
    }

    public static class GaussianRandom
    {
        public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }
    }
}