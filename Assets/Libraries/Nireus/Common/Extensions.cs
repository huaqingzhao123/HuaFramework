//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Net;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Nireus
//{

//    #region Random Extensions

//    public static class RandomExtensions
//    {
//        static int _seed = Environment.TickCount;

//        //[ThreadStatic]
//        //static Xoshiro256PlusRandom _random;
//        //public static Xoshiro256PlusRandom Instance => _random ?? (_random = new Xoshiro256PlusRandom((ulong)Interlocked.Increment(ref _seed)));

//        static readonly ThreadLocal<Xoshiro256PlusRandom> _random = new ThreadLocal<Xoshiro256PlusRandom>(() => new Xoshiro256PlusRandom((ulong)Interlocked.Increment(ref _seed)));
//        public static Xoshiro256PlusRandom Instance => _random.Value;

//        public static bool RandomHit(this Random rnd, int percent, int max)
//        {
//            if (max < 0) return false;
//            if (percent >= max) return true;

//            //避免随机永远出现0后,总是命中
//            return rnd.Next(max) > (max - percent);
//        }

//        public static string RandomString(this Random rnd, int size)
//        {
//            const string seedString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

//            StringBuilder builder = new StringBuilder();
//            for (int i = 0; i < size; i++)
//            {
//                var ch = seedString[rnd.Next(seedString.Length)];
//                builder.Append(ch);
//            }

//            return builder.ToString();
//        }

//        public static IList<int> RandomNumbers(this Random rnd, int max, int count)
//        {
//            return RandomNumbers(rnd, 0, max, count);
//        }

//        public static IList<int> RandomNumbers(this Random rnd, int min, int max, int count)
//        {
//            var values = new List<int>();
//            if (count < 0 || max < min) return values;

//            var total = max - min;
//            if (count > total)
//                count = total;

//            var seeds = new int[total];
//            for (int i = 0; i < total; i++)
//            {
//                seeds[i] = i + min;
//            }
//            for (int i = 0; i < count; i++)
//            {
//                int idx = rnd.Next(total - i);
//                values.Add(seeds[idx]);
//                seeds[idx] = seeds[total - i - 1];
//            }

//            return values;
//        }

//        public static IList<int> RandomNumbersIgnore(this Random rnd, int min, int max, IList<int> ignores, int count, bool canRepeat = false)
//        {
//            var values = new List<int>();
//            int maxCount = max - min;
//            if (count < 0 || (!canRepeat && maxCount < count)) return values;

//            var tmpList = new List<int>();
//            for(int i= min; i < max; i++)
//            {
//                tmpList.Add(i);
//            }
//            for (int i = ignores.Count-1; i >=0; i--)
//            {
//                tmpList.Remove(ignores[i]);
//            }
//            if(tmpList.Count<=0 ||(!canRepeat && tmpList.Count< count)) return values;

//            for (int m = 0; m < count; m++)
//            {
//                var total = tmpList.Count;
//                var target = rnd.Next(total);
//                values.Add(tmpList[target]);
//                if (!canRepeat)
//                {
//                    tmpList.Remove(tmpList[target]);
//                }
//            }

//            return values;
//        }

//        public static IList<int> FastRandomNumbers(this Random rnd, int max, int count)
//        {
//            if (count >= max) return rnd.RandomNumbers(0, max, count);
//            if (max < 100) return rnd.RandomNumbers(0, max, count);

//            int maxSeg = max / count;
//            int segCnt = rnd.Next(maxSeg) + 1;
//            float maxStep = max / segCnt;
//            float minStep = maxStep / count;

//            List<int> values = new List<int>();
//            for (int i = 0; i < count; i++)
//            {
//                int segId = rnd.Next(segCnt);
//                var offsetA = rnd.NextDouble() * minStep + i * minStep;
//                var offsetB = segId * maxStep + offsetA;

//                var left = (int)Math.Ceiling(segId * maxStep + i * minStep);
//                var right = (int)(segId * maxStep + (i + 1) * minStep);

//                var value = Math.Min(Math.Max(left, (int)offsetB), right);

//                values.Add(value);
//            }
//            return values;
//        }

//        [Obsolete("Performance issue. Use RandomNumbers(total, count).ToList() for instead.")]
//        public static List<int> RandomIndexes(this Random rnd, int total, int count, bool canRepeat = false)
//        {
//            var values = new List<int>();
//            if (count < 0 || (!canRepeat && total < count)) return values;

//            var tmpList = new List<int>();
//            for (int i = 0; i < total; i++)
//            {
//                tmpList.Add(i);
//            }

//            for (int m = 0; m < count; m++)
//            {
//                var target = Instance.Next(tmpList.Count);
//                values.Add(tmpList[target]);
//                tmpList.RemoveAt(target);
//            }

//            return values;
//        }

//        public static IList<int> RandomIndexes(this Random rnd, IList<int> weights, int count, bool canRepeat = false)
//        {
//            var values = new List<int>();
//            if (count < 0 || (!canRepeat && weights.Count < count)) return values;

//            var tmpList = new List<int>();
//            tmpList.AddRange(weights);

//            for (int m = 0; m < count; m++)
//            {
//                var total = tmpList.Sum();
//                var target = Instance.Next(total) + 1;
//                var current = 0;

//                for (int n = 0; n < tmpList.Count; n++)
//                {
//                    current += tmpList[n];
//                    if(target <= current)
//                    {
//                        values.Add(n);
//                        if (!canRepeat)
//                        {
//                            tmpList[n] = 0;
//                        }
//                        break;
//                    }
//                }
//            }

//            return values;
//        }

//        public static int RandomIndex(this Random rnd, IList<int> weights)
//        {
//            return RandomIndexes(rnd, weights, 1).FirstOrDefault();
//        }

//        public static IList<int> RandomIndexesByConfig<T>(this Random rnd, Dictionary<int, T> ConfigMap, int count, bool canRepeat = false)
//        {
//            var values = new List<int>();
//            var indexs = new List<int>();
//            var weights = new List<int>();
//            foreach (var config in ConfigMap)
//            {
//                var porp = config.Value.GetType().GetProperty("weight");
//                if (porp != null)
//                {
//                    var value = porp.GetValue(config.Value, null);
//                    weights.Add((int)value);
//                    indexs.Add(config.Key);
//                }
//                else
//                {
//                    throw new System.Exception("config err weight is null");
//                }
//            }

//            var temp = RandomIndexes(rnd, weights, count, canRepeat);
//            foreach (var config in temp)
//            {
//                values.Add(indexs[config]);
//            }

//            return values;
//        }

//        public static IList<int> RandomIndexesByConfigIgnore<T>(this Random rnd, Dictionary<int, T> ConfigMap, int count, IList<int> ignores,bool canRepeat = false)
//        {
//            var values = new List<int>();
//            var indexs = new List<int>();
//            var weights = new List<int>();
//            foreach (var config in ConfigMap)
//            {
//                bool bignore = false;
//                foreach(var ignore in ignores)
//                {
//                    if (config.Key == ignore)
//                    {
//                        bignore = true;
//                        continue;
//                    }
//                }
//                if (bignore)
//                {
//                    continue;
//                }
//                var porp = config.Value.GetType().GetProperty("weight");
//                if (porp != null)
//                {
//                    var value = porp.GetValue(config.Value, null);
//                    weights.Add((int)value);
//                    indexs.Add(config.Key);
//                }
//                else
//                {
//                    throw new System.Exception("config err weight is null");
//                }
//            }

//            var temp = RandomIndexes(rnd, weights, count, canRepeat);
//            foreach (var config in temp)
//            {
//                values.Add(indexs[config]);
//            }

//            return values;
//        }

//        public static List<T> Shuffle<T>(this Random rnd, List<T> source, out List<int> order)
//        {
//            order = rnd.RandomNumbers(source.Count, source.Count).ToList();
//            var output = new List<T>();
//            for (int i = 0; i < order.Count; i++)
//            {
//                output.Add(source[order[i]]);
//            }
//            return output;
//        }

//        public static TV RandomFromDict<TK, TV>(this Random rnd, IDictionary<TK, TV> dict)
//        {
//            if (dict == null || dict.Count == 0) return default(TV);

//            return dict.ElementAt(rnd.Next(0, dict.Count)).Value;
//        }
//    }

//    #endregion
//}
