using System;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// A simple integer array backed list like structure
    /// </summary>
    public class IntVector
    {
        private static readonly int DefaultInitialCapacity = 7;
        private int[] data;
        private int size = 0;

        public IntVector()
        {
            data = new int[DefaultInitialCapacity];
        }

        public IntVector(int initialCapacity)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentException("Initial capacity must be positive. But it is " + initialCapacity);
            }
            data = new int[initialCapacity];
        }

        public IntVector(int[] values)
        {
            data = new int[values.Length + DefaultInitialCapacity];
            Array.Copy(values, 0, data, 0, values.Length);
            size = values.Length;
        }

        public void Add(int i)
        {
            if (size == data.Length)
            {
                Expand();
            }
            data[size] = i;
            size++;
        }

        public void AddAll(int[] arr)
        {
            if (size + arr.Length >= data.Length)
            {
                Expand(arr.Length);
            }
            Array.Copy(arr, 0, data, size, arr.Length);
            size += arr.Length;
        }

        public void AddAll(IntVector vec)
        {
            if (size + vec.size >= data.Length)
            {
                Expand(vec.size);
            }
            Array.Copy(vec.data, 0, data, size, vec.size);
            size += vec.size;
        }

        public int Get(int index)
        {
            return data[index];
        }

        public void Set(int index, int value)
        {
            data[index] = value;
        }

        public void SafeSet(int index, int value)
        {
            if (index < 0 || index >= size)
            {
                throw new IndexOutOfRangeException("Bad index: " + index);
            }
            data[index] = value;
        }

        public int Size()
        {
            return size;
        }

        public int Capacity()
        {
            return data.Length;
        }

        public void Sort()
        {
            Array.Sort(data, 0, size);
        }

        public int[] CopyOf()
        {
            return data.CopyOf(size);
        }

        public void Shuffle(Random random)
        {
            for (int i = size - 1; i > 0; i--)
            {
                int index = random.Next(i + 1);
                int d = data[index];
                data[index] = data[i];
                data[i] = d;
            }
        }

        public bool Contains(int i)
        {
            for (int j = 0; j < size; j++)
            {
                if (data[j] == i)
                {
                    return true;
                }
            }
            return false;
        }

        public void Shuffle()
        {
            Shuffle(new Random());
        }

        public void TrimToSize()
        {
            data = data.CopyOf(size);
        }

        private void Expand()
        {
            Expand(0);
        }

        public bool IsEmpty()
        {
            return size == 0;
        }

        private void Expand(int offset)
        {
            if (size + offset >= int.MaxValue)
            {
                throw new InvalidOperationException("List size exceeded positive integer limit.");
            }
            long newSize = size * 2L + offset;
            if (newSize > int.MaxValue)
            {
                size = int.MaxValue;
            }

            data = data.CopyOf((int)newSize);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            IntVector vector = (IntVector)obj;

            if (size != vector.size)
            {
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (data[i] != vector.data[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = 0;
            for (int i = 0; i < size; i++)
            {
                result = 31 * result + data[i];
            }
            result = 31 * result + size;
            return result;
        }
    }
}
