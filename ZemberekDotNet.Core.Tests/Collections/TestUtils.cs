using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    public class TestUtils
    {
        public static List<int[]> CreateFuzzingLists()
        {
            List<int[]> fuzzLists = new List<int[]>(5000);
            int maxListSize = 300;
            Random r = new Random(Convert.ToInt32("0xbeefcafe", 16));
            // Random sized lists with values in [0..n] shuffled.
            for (int i = 0; i < 1000; i++)
            {
                int[] arr = new int[r.Next(maxListSize) + 1];
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = j;
                }
                Shuffle(arr);
                fuzzLists.Add(arr);
            }
            // Random sized lists with values in [-n..n] shuffled.
            for (int i = 0; i < 1000; i++)
            {
                int size = r.Next(maxListSize) + 1;
                int[] arr = new int[size * 2];
                int idx = 0;
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[idx++] = j - size;
                }
                Shuffle(arr);
                fuzzLists.Add(arr);
            }
            // Random sized lists in [-m,m] shuffled. Possible duplicates.
            int m = 1 << 10;
            for (int i = 0; i < 2000; i++)
            {
                int size = r.Next(maxListSize) + 1;
                int[] arr = new int[size];
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = r.Next(2 * m) - m;
                }
                Shuffle(arr);
                fuzzLists.Add(arr);
            }
            return fuzzLists;
        }

        // Fisher yates shuffle
        public static void Shuffle(int[] array)
        {
            int index, temp;
            Random random = new Random(Convert.ToInt32("0xbeefcafe", 16));
            for (int i = array.Length - 1; i > 0; i--)
            {
                index = random.Next(i + 1);
                temp = array[index];
                array[index] = array[i];
                array[i] = temp;
            }
        }

        public static int[] CreateRandomUintArray(int size)
        {
            Random random = new Random(Convert.ToInt32("0xbeefcafe", 16));
            UIntSet uIntSet = new UIntSet();
            while (uIntSet.Size() < size)
            {
                uIntSet.Add(System.Math.Abs(random.Next()));
            }
            int[] res = uIntSet.GetKeys();
            Shuffle(res);
            return res;
        }

        public static int[] createRandomUintArray(int size, int limit)
        {
            Random random = new Random(Convert.ToInt32("0xbeefcafe", 16));
            UIntSet uIntSet = new UIntSet();
            while (uIntSet.Size() < size)
            {
                uIntSet.Add(System.Math.Abs(random.Next(limit)));
            }
            int[] res = uIntSet.GetKeys();
            Shuffle(res);
            return res;
        }
    }
}
