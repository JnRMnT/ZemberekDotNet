using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Core.Math
{
    public class FloatArrays
    {
        private static readonly CultureInfo EnUsCulture = CultureInfo.GetCultureInfo("en-US");
        public static readonly float[] ZeroLengthArray = new float[0];
        public static readonly float[][] ZeroLengthMatrix = new float[0][];
        public static readonly Regex FeatureLinesPattern = new Regex("(?:\\[)(.+?)(?:\\])", RegexOptions.Singleline | RegexOptions.Multiline);

        // do not allow instantiation
        private FloatArrays()
        {
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="range"></param>
        /// <returns>true if difference is smaller or equal to range</returns>
        public static bool InRange(float d1, float d2, float range)
        {
            return System.Math.Abs(d1 - d2) <= range;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">float array</param>
        /// <returns>reverse of the float array</returns>
        public static float[] Reverse(float[] input)
        {
            float[] result = new float[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[input.Length - i - 1] = input[i];
            }
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">int array</param>
        /// <returns>float array converted from input int array</returns>
        public static float[] Convert(int[] input)
        {
            float[] data = new float[input.Length];
            int k = 0;
            foreach (int i in input)
            {
                data[k++] = i;
            }
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">2d int array</param>
        /// <returns>2d float array converted from 2d int array</returns>
        public static float[][] Convert(int[][] input)
        {
            float[][] data = new float[input.Length][];
            int k = 0;
            foreach (int[] i in input)
            {
                data[k] = new float[i.Length];
                int j = 0;
                foreach (int ii in i)
                {
                    data[k][j++] = ii;
                }
                k++;
            }
            return data;
        }

        /**
         * @param input float array
         * @return float array converted from input float array
         */
        public static float[] Convert(double[] input)
        {
            float[] data = new float[input.Length];
            int k = 0;
            foreach (double i in input)
            {
                data[k++] = (float)i;
            }
            return data;
        }

        /**
         * @param d1 input float array
         * @param d2 input float array
         * @param range float input
         * @return true if the difference between elements of d1 and d2 is smaller than or equal to given
         * range
         */
        public static bool ArrayEqualsInRange(float[] d1, float[] d2, float range)
        {
            ValidateArrays(d1, d2);
            for (int i = 0; i < d1.Length; i++)
            {
                if (System.Math.Abs(d1[i] - d2[i]) > range)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns>true if two input float arrays are equal</returns>
        public static bool ArrayEquals(float[] d1, float[] d2)
        {
            ValidateArrays(d1, d2);
            return Enumerable.SequenceEqual(d1, d2);
        }

        /**
         * @return the float array after appending zeros to its end with the given amount
         * @throws IllegalArgumentException when amount input is negative
         */
        public static float[] AppendZeros(float[] darray, int zeroAmountToAppend)
        {
            if (zeroAmountToAppend < 0)
            {
                throw new ArgumentException(
                    "Cannot append negative amount of zeros. Amount:" + zeroAmountToAppend);
            }
            return darray.CopyOf(darray.Length + zeroAmountToAppend);
        }

        public static float[] Normalize16bitLittleEndian(byte[] bytez)
        {
            return Normalize16bitLittleEndian(bytez, bytez.Length);
        }

        /**
         * @param bytez input byte array
         * @param amount input, size of the byte array
         * @return float array including the normalized float value of each byte elements as Little-Endian
         * representation For 0xABCD: Big-Endian Rep.-->0xABCD Little-Endian Rep-->0xCDBA
         */
        public static float[] Normalize16bitLittleEndian(byte[] bytez, int amount)
        {
            if ((amount & 1) != 0)
            {
                throw new ArgumentException(
                    "Amount of bytes must be an order of 2. But it is: " + amount);
            }
            float[] result = new float[amount / 2];
            for (int i = 0; i < amount; i += 2)
            {
                int val = (short)(bytez[i + 1] << 8) | (bytez[i] & 0xff);
                if (val >= 0)
                {
                    result[i >> 1] = (float)val / short.MaxValue;
                }
                else
                {
                    result[i >> 1] = -(float)val / short.MinValue;
                }
            }
            return result;
        }

        /**
         * @param input input float array
         * @return byte array including the de-normalized 16-bit Big-Endian representations of float
         * values in float array
         */
        public static byte[] Denormalize16BitLittleEndian(float[] input)
        {
            byte[] result = new byte[input.Length * 2];
            for (int i = 0; i < input.Length; i++)
            {
                int denorm;
                if (input[i] < 0)
                {
                    denorm = (int)(-input[i] * short.MinValue);
                }
                else
                {
                    denorm = (int)(input[i] * short.MaxValue);
                }
                result[i * 2] = (byte)(denorm & 0xff);
                result[i * 2 + 1] = (byte)(denorm >> 8);
            }
            return result;
        }

        /**
         * @param input input float array
         * @param bitsPerSample input as bit number
         * @return byte array including the de-normalized n-bit Big-Endian representations of float values
         * in float array where n is bitsPerSample
         */
        public static byte[] DenormalizeLittleEndian(float[] input, int bitsPerSample)
        {
            int bytesPerSample = bitsPerSample % 8 == 0 ? bitsPerSample / 8 : bitsPerSample / 8 + 1;
            int maxVal = 1 << bitsPerSample - 1;
            byte[] result = new byte[input.Length * bytesPerSample];
            for (int i = 0; i < input.Length; i++)
            {
                int denorm;
                if (input[i] < 0)
                {
                    denorm = (int)(-input[i] * maxVal);
                }
                else
                {
                    denorm = (int)(input[i] * maxVal);
                }
                for (int j = 0; j < bytesPerSample; j++)
                {
                    result[i * bytesPerSample + j] = (byte)((denorm >> j * 8) & 0xff);
                }
            }
            return result;
        }

        /**
         * gets a float array with values between -1.0 and 1.0 and converts it to an integer in the range
         * of [0,max]
         *
         * @param input float array
         * @param max max integer value.
         * @return an integer array/
         */
        public static int[] ToUnsignedInteger(float[] input, int max)
        {
            if (max < 1)
            {
                throw new ArgumentException("Maximum int value must be positive. But it is:" + max);
            }
            int[] arr = new int[input.Length];
            float divider = (float)((double)max / 2.0);
            for (int i = 0; i < input.Length; i++)
            {
                float d = input[i];
                if (d < -1.0 || d > 1.0)
                {
                    throw new ArgumentException(
                        "Array value should be between -1.0 and 1.0. But it is: " + d);
                }
                arr[i] = (int)(input[i] * divider);
            }
            return arr;
        }

        /**
         * Converts to an integer array. if values are outside of the given boundaries, throws Exception.
         */
        public static int[] ToInteger(float[] input, int min, int max)
        {
            ValidateArray(input);
            int[] result = new int[input.Length];
            int i = 0;
            foreach (float v in input)
            {
                if (!float.IsFinite(v) || float.IsNaN(v))
                {
                    throw new InvalidOperationException("Value" + v + " cannot be converted.");
                }
                if (v < min || v > max)
                {
                    throw new InvalidOperationException("Value" + v + "is outside of min-max boundaries.");
                }
                result[i] = (int)v;
                i++;
            }
            return result;
        }

        /**
         * finds the maximum value of an array.
         *
         * @param input input array
         * @return maximum value.
         * @throws IllegalArgumentException if array is empty or null.
         */
        public static float Max(params float[] input)
        {
            ValidateArray(input);
            float max = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] > max)
                {
                    max = input[i];
                }
            }
            return max;
        }

        /**
         * Trims the values of an array against a minimum and maximum value.
         *
         * @param input input array
         * @throws IllegalArgumentException if array is empty or null.
         */
        public static void TrimValues(float[] input, float minVal, float maxVal)
        {
            ValidateArray(input);
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] < minVal)
                {
                    input[i] = minVal;
                }
                else if (input[i] > maxVal)
                {
                    input[i] = maxVal;
                }
            }
        }

        /**
         * Finds the maximum absolute value of an array.
         *
         * @param input input array
         * @return maximum absolute value.
         * @throws IllegalArgumentException if array is empty or null.
         */
        public static float AbsMax(params float[] input)
        {
            ValidateArray(input);
            float max = System.Math.Abs(input[0]);
            for (int i = 1; i < input.Length; i++)
            {
                float abs = System.Math.Abs(input[i]);
                if (abs > max)
                {
                    max = abs;
                }
            }
            return max;
        }

        /**
         * Formats a float array as string using English Locale.
         */
        public static String Format(params float[] input)
        {
            return Format(10, 3, " ", input);
        }

        /**
         * Formats a float array as string using English Locale.
         */
        public static string Format(int fractionDigits, params float[] input)
        {
            return Format(fractionDigits, " ", input);
        }

        /**
         * Formats a float array as string using English Locale.
         */
        public static string Format(int fractionDigits, String delimiter, params float[] input)
        {
            StringBuilder sb = new StringBuilder();
            String formatStr = "{0:F" + fractionDigits + "}";
            int i = 0;
            foreach (float v in input)
            {
                sb.Append(String.Format(EnUsCulture, formatStr, v));
                if (i++ < input.Length - 1)
                {
                    sb.Append(delimiter);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Formats a float array as string using English Locale.
        /// </summary>
        /// <param name="rightPad"></param>
        /// <param name="fractionDigits"></param>
        /// <param name="delimiter"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Format(int rightPad, int fractionDigits, String delimiter, params float[] input)
        {
            StringBuilder sb = new StringBuilder();
            String formatStr = "{0:F" + fractionDigits + "}";
            int i = 0;
            foreach (float v in input)
            {
                string num = string.Format(EnUsCulture, formatStr, v);
                sb.Append(string.Format(EnUsCulture, "{0,-" + rightPad + "}", num));
                if (i++ < input.Length - 1)
                {
                    sb.Append(delimiter);
                }
            }
            return sb.ToString().Trim();
        }

        /**
         * finds the minimum value of an array.
         *
         * @param input input array
         * @return minimum value.
         * @throws IllegalArgumentException if array is empty or null.
         */
        public static float Min(params float[] input)
        {
            ValidateArray(input);
            float min = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] < min)
                {
                    min = input[i];
                }
            }
            return min;
        }

        /**
         * Finds the minimum absolute value of an array.
         *
         * @param input input array
         * @return minimum value.
         * @throws IllegalArgumentException if array is empty or null.
         */
        public static float AbsMin(params float[] input)
        {
            ValidateArray(input);
            float min = System.Math.Abs(input[0]);
            for (int i = 1; i < input.Length; i++)
            {
                float abs = System.Math.Abs(input[i]);
                if (abs < min)
                {
                    min = abs;
                }
            }
            return min;
        }

        /**
         * checks whether the input array is null or empty
         *
         * @param input input float array
         */
        public static void ValidateArray(params float[] input)
        {
            if (input == null)
            {
                throw new ArgumentException("array is null!");
            }
            else if (input.Length == 0)
            {
                throw new ArgumentException("array is empty!");
            }
        }

        /**
         * @param input input array
         * @return index at which the maximum value of input is, minimum index is returned when multiple
         * maximums
         */
        public static int MaxIndex(params float[] input)
        {
            ValidateArray(input);
            float max = input[0];
            int index = 0;
            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] > max)
                {
                    max = input[i];
                    index = i;
                }
            }
            return index;
        }

        /**
         * @param input input array
         * @return index at which the minimum value element of input is, minimum index is returned when
         * multiple minimums
         */
        public static int MinIndex(params float[] input)
        {
            ValidateArray(input);
            float min = input[0];
            int minIndex = 0;
            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] < min)
                {
                    min = input[i];
                    minIndex = i;
                }
            }
            return minIndex;
        }

        /**
         * @param input input
         * @return sum of all elements in array
         */
        public static float Sum(params float[] input)
        {
            float sum = 0;
            foreach (float v in input)
            {
                sum += v;
            }
            return sum;
        }

        /**
         * @param a1 input
         * @param a2 input
         * @return float array of which elements are the sum of 2 input arrays' elements
         */
        public static float[] Sum(float[] a1, float[] a2)
        {
            ValidateArrays(a1, a2);
            float[] sum = new float[a1.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                sum[i] = a1[i] + a2[i];
            }
            return sum;
        }

        /**
         * sums two float vector. result is written to first vector.
         *
         * @param first first vector.
         * @param second second vector
         */
        public static void AddToFirst(float[] first, float[] second)
        {
            ValidateArrays(first, second);
            for (int i = 0; i < first.Length; i++)
            {
                first[i] = first[i] + second[i];
            }
        }

        /**
         * Adds a value to all elements of the [data] array.
         */
        public static void AddToAll(float[] data, float valueToAdd)
        {
            ValidateArray(data);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] += valueToAdd;
            }
        }

        /**
         * sums two float vectors (second vector is scaled by scale factor). result is written to first
         * vector.
         *
         * @param first first vector.
         * @param second second vector
         * @param scale scale factor for second
         */
        public static void AddToFirstScaled(float[] first, float[] second, float scale)
        {
            ValidateArrays(first, second);
            for (int i = 0; i < first.Length; i++)
            {
                first[i] = first[i] + second[i] * scale;
            }
        }

        /**
         * @param input input float array
         * @return an array containing square-values of the input array's elements
         */
        public static float[] Square(params float[] input)
        {
            float[] res = new float[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                res[i] = input[i] * input[i];
            }
            return res;
        }

        public static void SquareInPlace(params float[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = input[i] * input[i];
            }
        }

        /**
         * Subtracts two float vector.
         *
         * @param a1 first vector.
         * @param a2 second vector
         * @return Subtraction result
         */
        public static float[] Subtract(float[] a1, float[] a2)
        {
            ValidateArrays(a1, a2);
            float[] diff = new float[a1.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                diff[i] = a1[i] - a2[i];
            }
            return diff;
        }

        /**
         * substracts two float vector. result is written to first vector.
         *
         * @param first first vector.
         * @param second second vector
         */
        public static void SubtractFromFirst(float[] first, float[] second)
        {
            ValidateArrays(first, second);
            for (int i = 0; i < first.Length; i++)
            {
                first[i] = first[i] - second[i];
            }
        }

        /**
         * @param a1 input float array
         * @param a2 input float array
         * @return the array produced after multiplying the elements of input arrays
         */
        public static float[] Multiply(float[] a1, float[] a2)
        {
            ValidateArrays(a1, a2);
            float[] mul = new float[a1.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                mul[i] = a1[i] * a2[i];
            }
            return mul;
        }

        /**
         * @param a1 input float array
         * @param a2 input float array
         * @return the dot product value of elements in input arrays
         */

        public static float DotProduct(float[] a1, float[] a2)
        {
            return Sum(Multiply(a1, a2));
        }

        /**
         * multiplies two float vectors and result is written to the first vector.
         *
         * @param first first vector
         * @param second second vector.
         */
        public static void MultiplyToFirst(float[] first, float[] second)
        {
            ValidateArrays(first, second);
            for (int i = 0; i < first.Length; i++)
            {
                first[i] = first[i] * second[i];
            }
        }

        /**
         * Multiplies all elements of a vector with a float number and returns a new vector
         *
         * @param a1 vector
         * @param b scale factor
         * @return new scaled vector
         */
        public static float[] Scale(float[] a1, float b)
        {
            ValidateArray(a1);
            float[] mul = new float[a1.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                mul[i] = a1[i] * b;
            }
            return mul;
        }

        /**
         * Multiplies all elements of a vector with a float number
         *
         * @param a1 vector
         * @param b scale factor
         */
        public static void ScaleInPlace(float[] a1, float b)
        {
            ValidateArray(a1);
            for (int i = 0; i < a1.Length; i++)
            {
                a1[i] = a1[i] * b;
            }
        }

        /**
         * Calculates mean of a vector.
         *
         * @param input float array
         * @return mean
         */
        public static float Mean(params float[] input)
        {
            ValidateArray(input);
            return Sum(input) / input.Length;
        }

        /**
         * for A=[a0, a1, ...,an] for B=[b0, b1, ...,bn] returns C=|a0-b0|+|a1-b1|+...+|an-bn|
         *
         * @param a input array a
         * @param b input array b
         * @return squared sum of array elements.
         */
        public static float AbsoluteSumOfDifferences(float[] a, float[] b)
        {
            return Sum(AbsoluteDifference(a, b));
        }

        /**
         * for A=[a0, a1, ...,an] for B=[b0, b1, ...,bn] returns C=[|a0-b0|,|a1-b1|,...,|an-bn|]
         *
         * @param a input array a
         * @param b input array b
         * @return squared sum of array elements.
         */
        public static float[] AbsoluteDifference(float[] a, float[] b)
        {
            ValidateArrays(a, b);
            float[] diff = new float[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                diff[i] += System.Math.Abs(a[i] - b[i]);
            }
            return diff;
        }

        /**
         * checks whether one of the input arrays are null or not, and whether their length is equal or
         * not
         *
         * @param a1 input float array
         * @param a2 input float array
         */
        public static void ValidateArrays(float[] a1, float[] a2)
        {
            if (a1 == null)
            {
                throw new NullReferenceException("first array is null!");
            }
            if (a2 == null)
            {
                throw new NullReferenceException("second array is null!");
            }
            if (a1.Length != a2.Length)
            {
                throw new ArgumentException("Array sizes must be equal. But, first:"
                    + a1.Length + ", and second:" + a2.Length);
            }
        }

        /**
         * for A=[a0, a1, ...,an] returns a0*a0+a1*a1+....+an*an
         *
         * @param array input array
         * @return squared sum of array elements.
         */
        public static float SquaredSum(float[] array)
        {
            float result = 0;
            ValidateArray(array);
            foreach (float a in array)
            {
                result += a * a;
            }
            return result;
        }

        public static float SquaredSumOfDifferences(float[] a, float[] b)
        {
            return (SquaredSum(Subtract(a, b)));
        }

        /**
         * @param input input float array
         * @return variance value of the elements in the input array
         */
        public static float Variance(float[] input)
        {
            float sigmaSquare = 0;
            float mean = Mean(input);
            foreach (float a in input)
            {
                float meanDiff = a - mean;
                sigmaSquare += meanDiff * meanDiff;
            }
            return sigmaSquare / (input.Length - 1);
        }

        /**
         * @param a input float array
         * @return standard deviation value of the elements in the input array
         */
        public static float StandardDeviation(float[] a)
        {
            return (float)System.Math.Sqrt(Variance(a));
        }

        /**
         * @param a input float array
         * @return true if array includes at least one Not-a-Number (NaN) value, false otherwise
         */
        public static bool ContainsNaN(float[] a)
        {
            foreach (float v in a)
            {
                if (float.IsNaN(v))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * @param a input float array
         * @return true if array includes at least one Not-a-Number (NaN) or infinite value, false
         * otherwise
         */
        public static bool ContainsNanOrInfinite(float[] a)
        {
            foreach (float v in a)
            {
                if (float.IsNaN(v) || !float.IsFinite(v))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * replaces the elements smaller than minValue with the minValue
         *
         * @param var input float array
         * @param minValue float
         */
        public static void FloorInPlace(float[] var, float minValue)
        {
            for (int k = 0; k < var.Length; k++)
            {
                if (var[k] < minValue)
                {
                    var[k] = minValue;
                }
            }
        }

        /**
         * If a data point is non-zero and below 'floor' make it equal to floor
         *
         * @param data the data to floor
         * @param floor the floored value
         */
        public static void NonZeroFloorInPlace(float[] data, float floor)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0.0f && data[i] < floor)
                {
                    data[i] = floor;
                }
            }
        }

        /**
         * Normalize the given data.
         *
         * @param data the data to normalize
         */
        public static void NormalizeInPlace(float[] data)
        {
            float sum = Sum(data);
            ScaleInPlace(data, 1f / sum);
        }

        public static float[] FromString(string str, string delimiter)
        {
            List<string> tokens = str.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList(); ;
            float[] result = new float[tokens.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = float.Parse(tokens[i], CultureInfo.InvariantCulture);
            }
            return result;
        }

        public static void Serialize(BinaryWriter dos, float[] data)
        {
            dos.Write(data.Length.EnsureEndianness());
            foreach (float v in data)
            {
                dos.Write(v.EnsureEndianness());
            }
        }

        public static void SerializeRaw(BinaryWriter dos, float[] data)
        {
            foreach (float v in data)
            {
                dos.Write(v.EnsureEndianness());
            }
        }

        public static void Serialize(BinaryWriter dos, float[][] data)
        {
            dos.Write(data.Length.EnsureEndianness());
            foreach (float[] floats in data)
            {
                Serialize(dos, floats);
            }
        }

        public static void SerializeRaw(BinaryWriter dos, float[][] data)
        {
            foreach (float[] floats in data)
            {
                SerializeRaw(dos, floats);
            }
        }

        public static float[] Deserialize(BinaryReader dis)
        {
            int amount = dis.ReadInt32().EnsureEndianness();
            float[]
            result = new float[amount];
            for (int i = 0; i < amount; i++)
            {
                result[i] = dis.ReadSingle().EnsureEndianness();
            }
            return result;
        }

        public static float[] DeserializeRaw(BinaryReader dis, int amount)
        {
            float[] result = new float[amount];
            for (int i = 0; i < amount; i++)
            {
                result[i] = dis.ReadSingle().EnsureEndianness();
            }
            return result;
        }

        public static void DeserializeRaw(BinaryReader dis, float[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = dis.ReadSingle().EnsureEndianness();
            }
        }

        public static float[][] Deserialize2d(BinaryReader dis)
        {
            int amount = dis.ReadInt32().EnsureEndianness();
            float[][] result = new float[amount][];
            for (int i = 0; i < amount; i++)
            {
                result[i] = Deserialize(dis);
            }
            return result;
        }

        public static void Deserialize2DRaw(BinaryReader dis, float[][] result)
        {
            foreach (float[]  row in result)
            {
                DeserializeRaw(dis, row);
            }
        }

        public static float[][] Clone2D(float[][] result)
        {
            float[][] arr = new float[result.Length][];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (float[])result[i].Clone();
            }
            return arr;
        }

        public static float[] To1D(float[][] matrix)
        {
            if (matrix.Length == 0)
            {
                return ZeroLengthArray;
            }
            int dimension = matrix[0].Length;
            if (dimension == 0)
            {
                return ZeroLengthArray;
            }
            float[] result = new float[matrix.Length * dimension];
            for (int i = 0; i < matrix.Length; i++)
            {
                float[] floats = matrix[i];
                if (floats.Length != dimension)
                {
                    throw new InvalidOperationException("Unexpected array size.");
                }
                Array.Copy(floats, 0, result, i * dimension, dimension);
            }
            return result;
        }

        public static float[][] ToMatrix(float[] vector, int dimension)
        {
            if (vector.Length == 0)
            {
                return ZeroLengthMatrix;
            }
            if (dimension <= 0)
            {
                throw new InvalidOperationException("Dimension must be a positive number.");
            }
            int vectorCount = vector.Length / dimension;
            if (vectorCount * dimension != vector.Length)
            {
                throw new InvalidOperationException("vector length is not a factor of dimension.");
            }
            float[][] result = new float[vectorCount][];
            for (int i = 0; i < vectorCount; i++)
            {
                result[i] = new float[dimension];
                Array.Copy(vector, i * dimension, result[i], 0, dimension);
            }
            return result;
        }

        /// <summary>
        /// loads float array from file with format: [1 2 3] [4 5 6]
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static float[][] LoadFromText(string input)
        {
            String wholeThing = new SimpleTextReader(input, "UTF-8").AsString();
            List<String> featureBlocks = Regexps.FirstGroupMatches(FeatureLinesPattern, wholeThing);
            float[][] result = new float[featureBlocks.Count][];
            int i = 0;
            foreach (string featureBlock in featureBlocks)
            {
                result[i] = FloatArrays.FromString(featureBlock, " ");
                i++;
            }
            return result;
        }
    }
}
