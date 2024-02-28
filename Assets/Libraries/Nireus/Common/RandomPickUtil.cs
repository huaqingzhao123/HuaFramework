using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nireus
{
    public interface IRandomPickItem
    {
        int randomPickWeight { get; }
    }

    public static class RandomPickUtil
    {
        public static readonly Random rand = new Random();

        /// <summary>
        /// Randomly pick an item (with weight) from a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemList"></param>
        /// <param name="weightSum">Give the sum of weight to reduce computation</param>
        /// <returns>The picked item</returns>
        public static T pickOne<T>(List<T> itemList, int weightSum = -1) where T : IRandomPickItem
        {
            if (itemList == null || itemList.Count == 0) return default(T);

            if (weightSum < 0)
            {
                weightSum = itemList.Sum(a => a.randomPickWeight);
            }

            if (weightSum == 0)
            {
                weightSum = itemList.Count;
            }

            int[] pool = new int[weightSum];
            int curIdx = 0;
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                for (int j = 0; j < item.randomPickWeight; j++)
                {
                    pool[curIdx] = i;
                    curIdx++;
                }
            }

            int pickedIndex = pool[rand.Next(0, weightSum)];
            return itemList[pickedIndex];
        }

        /// <summary>
        /// Randomly pick items (with weight) from a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemList"></param>
        /// <param name="pickCount">How many items you want to pick out</param>
        /// <param name="weightSum">Give the sum of weight to reduce computation</param>
        /// <returns>The picked items</returns>
        public static List<T> pick<T>(List<T> itemList, int pickCount, int weightSum = -1) where T : IRandomPickItem
        {
            var retList = new List<T>(); 

            if (itemList == null || itemList.Count == 0 || pickCount == 0 || itemList.Count < pickCount) return retList;

            List<T> itemListCopy = new List<T>(itemList);
            int weightSumCopy = weightSum;

            while (pickCount > 0)
            {
                var pickedItem = pickOne(itemListCopy, weightSumCopy);
                if (pickedItem == null) break;
                retList.Add(pickedItem);
                itemListCopy.Remove(pickedItem);
                weightSumCopy -= pickedItem.randomPickWeight;
                pickCount--;
            }

            return retList;
        }
    }
}
