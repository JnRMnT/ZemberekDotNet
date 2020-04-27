using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ZemberekDotNet.Core.Math
{
    public class DoubleArrays
    {
        private static readonly CultureInfo EnUsCulture = CultureInfo.GetCultureInfo("en-US");
        public static readonly double[] ZeroLengthArray = new double[0];

        // do not allow instantiation
        private DoubleArrays()
        {
        }

        /**
         * @return true if difference is smaller or equal to range
         */
        public static bool InRange(double d1, double d2, double range)
        {
            return System.Math.Abs(d1 - d2) <= range;
        }

        /**
         * @param input double array
         * @return reverse of the double array
         */
        public static double[] Reverse(double[] input)
        {
            double[] result = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[input.Length - i - 1] = input[i];
            }
            return result;
        }

        /**
         * @param input int array
         * @return double array converted from input int array
         */
        public static double[] Convert(int[] input)
        {
            double[] data = new double[input.Length];
            int k = 0;
            foreach (int i in input)
            {
                data[k++] = i;
            }
            return data;
        }

        /**
         * @param input 2d int array
         * @return 2d double array converted from 2d int array
         */
        public static double[][] Convert(int[][] input)
        {
            double[][] data = new double[input.Length][];
            int k = 0;
            foreach (int[] i in input)
            {
                data[k] = new double[i.Length];
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
         * @return double array converted from input float array
         */
        public static double[] Convert(float[] input)
        {
            double[] data = new double[input.Length];
            int k = 0;
            foreach (float i in input)
            {
                data[k++] = i;
            }
            return data;
        }

        /**
         * @param d1 input double array
         * @param d2 input double array
         * @param range double input
         * @return true if the difference between elements of d1 and d2 is smaller than or equal to given
         * range
         */
        public static bool ArrayEqualsInRange(double[] d1, double[] d2, double range)
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

        /**
         * @return true if two input double arrays are equal
         */
        public static bool ArrayEquals(double[] d1, double[] d2)
        {
            ValidateArrays(d1, d2);
            return Enumerable.SequenceEqual(d1, d2);
        }

        /**
         * @return the double array after appending zeros to its end with the given amount
         * @throws ArgumentException when amount input is negative
         */
        public static double[] AppendZeros(double[] darray, int zeroAmountToAppend)
        {
            if (zeroAmountToAppend < 0)
            {
                throw new ArgumentException(
                    "Cannot append negative amount of zeros. Amount:" + zeroAmountToAppend);
            }
            return darray.CopyOf(darray.Length + zeroAmountToAppend);
        }

        public static double[] Normalize16bitLittleEndian(byte[] bytez)
        {
            return Normalize16bitLittleEndian(bytez, bytez.Length);
        }

        /**
         * @param bytez input byte array
         * @param amount input, size of the byte array
         * @return double array inncluding the normalized double value of each byte elements as
         * Little-Endian representation For 0xABCD: Big-Endian Rep.-->0xABCD Little-Endian Rep-->0xCDBA
         */
        public static double[] Normalize16bitLittleEndian(byte[] bytez, int amount)
        {
            if ((amount & 1) != 0)
            {
                throw new ArgumentException(
                    "Amount of bytes must be an order of 2. But it is: " + amount);
            }
            double[] result = new double[amount / 2];
            for (int i = 0; i < amount; i += 2)
            {
                int val = (short)(bytez[i + 1] << 8) | (bytez[i] & 0xff);
                if (val >= 0)
                {
                    result[i >> 1] = (double)val / short.MaxValue;
                }
                else
                {
                    result[i >> 1] = -(double)val / short.MinValue;
                }
            }
            return result;
        }

        /**
         * @param input input double array
         * @return byte array including the de-normalized 16-bit Big-Endian representations of double
         * values in double array
         */
        public static byte[] Denormalize16BitLittleEndian(double[] input)
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
         * @param input input double array
         * @param bitsPerSample input as bit number
         * @return byte array including the de-normalized n-bit Big-Endian representations of double
         * values in double array where n is bitsPerSample
         */
        public static byte[] DenormalizeLittleEndian(double[] input, int bitsPerSample)
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
         * gets a double array with values between -1.0 and 1.0 and converts it to an integer in the range
         * of [0,max]
         *
         * @param input double array
         * @param max max integer value.
         * @return an integer array/
         */
        public static int[] ToUnsignedInteger(double[] input, int max)
        {
            if (max < 1)
            {
                throw new ArgumentException("Maximum int value must be positive. But it is:" + max);
            }
            int[] iarr = new int[input.Length];
            double divider = (double)max / 2.0;
            for (int i = 0; i < input.Length; i++)
            {
                double d = input[i];
                if (d < -1.0 || d > 1.0)
                {
                    throw new ArgumentException(
                        "Array value should be between -1.0 and 1.0. But it is: " + d);
                }
                iarr[i] = (int)(input[i] * divider);
            }
            return iarr;
        }

        /**
         * finds the maximum value of an array.
         *
         * @param input input array
         * @return maximum value.
         * @throws ArgumentException if array is empty or null.
         */
        public static double Max(params double[] input)
        {
            ValidateArray(input);
            double max = input[0];
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
         * finds the minimum value of an array.
         *
         * @param input input array
         * @return minimum value.
         * @throws ArgumentException if array is empty or null.
         */
        public static double Min(params double[] input)
        {
            ValidateArray(input);
            double min = input[0];
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
         * checks whether the input array is null or empty
         *
         * @param input input double array
         */
        public static void ValidateArray(params double[] input)
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
        public static int MaxIndex(params double[] input)
        {
            ValidateArray(input);
            double max = input[0];
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
        public static int MinIndex(params double[] input)
        {
            ValidateArray(input);
            double min = input[0];
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
        public static double Sum(params double[] input)
        {
            double sum = 0;
            foreach (double v in input)
            {
                sum += v;
            }
            return sum;
        }

        /**
         * @param a1 input
         * @param a2 input
         * @return double array of which elements are the sum of 2 input arrays' elements
         */
        public static double[] Sum(double[] a1, double[] a2)
        {
            ValidateArrays(a1, a2);
            double[] sum = new double[a1.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                sum[i] = a1[i] + a2[i];
            }
            return sum;
        }

        /**
         * sums two double vector. result is written to first vector.
         *
         * @param first first vector.
         * @param second second vector
         */
        public static void AddToFirst(double[] first, double[] second)
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
        public static void AddToAll(double[] data, double valueToAdd)
        {
            ValidateArray(data);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] += valueToAdd;
            }
        }


        /**
         * sums two double vectors (second vector is scaled by scale factor). result is written to first
         * vector.
         *
         * @param first first vector.
         * @param second second vector
         * @param scale scale factor for second
         */
        public static void AddToFirstScaled(double[] first, double[] second, double scale)
        {
            ValidateArrays(first, second);
            for (int i = 0; i < first.Length; i++)
            {
                first[i] = first[i] + second[i] * scale;
            }
        }

        /**
         * @param input input double array
         * @return an array containing square-values of the input array's elements
         */
        public static double[] Square(params double[] input)
        {
            double[] res = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                res[i] = input[i] * input[i];
            }
            return res;
        }

        public static void SquareInPlace(params double[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = input[i] * input[i];
            }
        }

        /**
         * substracts two double vector.
         *
         * @param a1 first vector.
         * @param a2 second vector
         * @return substraction result
         */
        public static double[] Subtract(double[] a1, double[] a2)
        {
            ValidateArrays(a1, a2);
            double[] diff = new double[a1.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                diff[i] = a1[i] - a2[i];
            }
            return diff;
        }

        /**
         * substracts two double vector. result is written to first vector.
         *
         * @param first first vector.
         * @param second second vector
         */
        public static void SubtractFromFirst(double[] first, double[] second)
        {
            ValidateArrays(first, second);
            for (int i = 0; i < first.Length; i++)
            {
                first[i] = first[i] - second[i];
            }
        }

        /**
         * @param a1 input double array
         * @param a2 input double array
         * @return the array produced after multiplying the elements of input arrays
         */
        public static double[] Multiply(double[] a1, double[] a2)
        {
            ValidateArrays(a1, a2);
            double[] mul = new double[a1.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                mul[i] = a1[i] * a2[i];
            }
            return mul;
        }

        /**
         * @param a1 input double array
         * @param a2 input double array
         * @return the dot product value of elements in input arrays
         */

        public static double DotProduct(double[] a1, double[] a2)
        {
            return Sum(Multiply(a1, a2));
        }

        /**
         * multiplies two double vectors and result is written to the first vector.
         *
         * @param first first vector
         * @param second second vector.
         */
        public static void MultiplyToFirst(double[] first, double[] second)
        {
            ValidateArrays(first, second);
            for (int i = 0; i < first.Length; i++)
            {
                first[i] = first[i] * second[i];
            }
        }

        /**
         * Multiplies all elements of a vector with a double number and returns a new vector
         *
         * @param a1 vector
         * @param b scale factor
         * @return new scaled vector
         */
        public static double[] Scale(double[] a1, double b)
        {
            ValidateArray(a1);
            double[] mul = new double[a1.Length];
            for (int i = 0; i < a1.Length; i++)
            {
                mul[i] = a1[i] * b;
            }
            return mul;
        }

        /**
         * Multiplies all elements of a vector with a double number
         *
         * @param a1 vector
         * @param b scale factor
         */
        public static void ScaleInPlace(double[] a1, double b)
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
         * @param input double array
         * @return mean
         */
        public static double Mean(params double[] input)
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
        public static double AbsoluteSumOfDifferences(double[] a, double[] b)
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
        public static double[] AbsoluteDifference(double[] a, double[] b)
        {
            ValidateArrays(a, b);
            double[] diff = new double[a.Length];
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
         * @param a1 input double array
         * @param a2 input double array
         */
        public static void ValidateArrays(double[] a1, double[] a2)
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
        public static double SquaredSum(double[] array)
        {
            double result = 0;
            ValidateArray(array);
            foreach (double a in array)
            {
                result += a * a;
            }
            return result;
        }

        public static double SquaredSumOfDifferences(double[] a, double[] b)
        {
            return (SquaredSum(Subtract(a, b)));
        }

        /**
         * @param input input double array
         * @return variance value of the elements in the input array
         */
        public static double Variance(double[] input)
        {
            double sigmaSquare = 0;
            double mean = Mean(input);
            foreach (double a in input)
            {
                double meanDiff = a - mean;
                sigmaSquare += meanDiff * meanDiff;
            }
            return sigmaSquare / (input.Length - 1);
        }

        /**
         * @param a input double array
         * @return standard deviation value of the elements in the input array
         */
        public static double StandardDeviation(double[] a)
        {
            return System.Math.Sqrt(Variance(a));
        }

        /**
         * @param a input double array
         * @return true if array includes at least one Not-a-Number (NaN) value, false otherwise
         */
        public static bool ContainsNaN(double[] a)
        {
            foreach (double v in a)
            {
                if (double.IsNaN(v))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * replaces the elements smaller than minValue with the minValue
         *
         * @param var input double array
         * @param minValue double
         */
        public static void FloorInPlace(double[] var, double minValue)
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
        public static void NonZeroFloorInPlace(double[] data, double floor)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0.0 && data[i] < floor)
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
        public static void NormalizeInPlace(double[] data)
        {
            double sum = Sum(data);
            ScaleInPlace(data, 1d / sum);
        }

        public static void Serialize(BinaryWriter dos, double[] data)
        {
            dos.Write(data.Length);
            foreach (double v in data)
            {
                dos.Write(v);
            }
        }

        public static void SerializeRaw(BinaryWriter dos, double[] data)
        {
            foreach (double v in data)
            {
                dos.Write(v);
            }
        }


        public static void Serialize(BinaryWriter dos, double[][] data)
        {
            dos.Write(data.Length);
            foreach (double[] doubles in data)
            {
                Serialize(dos, doubles);
            }
        }

        public static void SerializeRaw(BinaryWriter dos, double[][] data)
        {
            foreach (double[] doubles in data)
            {
                SerializeRaw(dos, doubles);
            }
        }


        public static double[] Deserialize(BinaryReader dis)
        {
            int amount = dis.ReadInt32();
            double[]
            result = new double[amount];
            for (int i = 0; i < amount; i++)
            {
                result[i] = dis.ReadDouble();
            }
            return result;
        }

        public static double[] DeserializeRaw(BinaryReader dis, int amount)
        {
            double[]
            result = new double[amount];
            for (int i = 0; i < amount; i++)
            {
                result[i] = dis.ReadDouble();
            }
            return result;
        }

        public static void DeserializeRaw(BinaryReader dis, double[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = dis.ReadDouble();
            }
        }

        public static double[][] Deserialize2d(BinaryReader dis)
        {
            int amount = dis.ReadInt32();
            double[]
            []
            result = new double[amount][];
            for (int i = 0; i < amount; i++)
            {
                result[i] = Deserialize(dis);
            }
            return result;
        }

        public static void Deserialize2DRaw(BinaryReader dis, double[][] result)
        {
            foreach (double[]
            row in result)
            {
                DeserializeRaw(dis, row);
            }
        }

        public static double[][] Clone2D(double[][] result)
        {
            double[]
            []
            arr = new double[result.Length][];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (double[])result[i].Clone();
            }
            return arr;
        }


        public static double[] FromString(string str, string delimiter)
        {
            List<string> tokens = str.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
            double[] result = new double[tokens.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = float.Parse(tokens[i]);
            }
            return result;
        }

        /**
         * Formats a float array as string using English Locale.
         */
        public static String Format(params double[] input)
        {
            return Format(10, 3, " ", input);
        }

        /**
         * Formats a float array as string using English Locale.
         */
        public static string Format(int fractionDigits, string delimiter, params double[] input)
        {
            StringBuilder sb = new StringBuilder();
            String formatStr = "{0:F" + fractionDigits + "}";
            int i = 0;
            foreach (double v in input)
            {
                sb.Append(String.Format(EnUsCulture, formatStr, v));
                if (i++ < input.Length - 1)
                {
                    sb.Append(delimiter);
                }
            }
            return sb.ToString();
        }

        /**
         * Formats a float array as string using English Locale.
         */
        public static string Format(int rightPad, int fractionDigits, String delimiter, params double[] input)
        {
            StringBuilder sb = new StringBuilder();
            String formatStr = "{0:F" + fractionDigits + "}";
            int i = 0;
            foreach (double v in input)
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


        public static double[] ReduceFractionDigits(double[] arr, int digitCount)
        {
            if (digitCount < 1 || digitCount > 10)
            {
                throw new ArgumentException("Digit count cannot be less than 1 or more than 10");
            }
            double[] newArr = new double[arr.Length];
            int powerOfTen = (int)System.Math.Pow(10, digitCount);
            for (int i = 0; i < arr.Length; i++)
            {
                double val = arr[i] * powerOfTen;
                val = System.Math.Round(val);
                val = val / powerOfTen;
                newArr[i] = val;
            }
            return newArr;
        }

        public static float[] FromDelimitedString(string input, string delimiter)
        {
            List<string> tokens = input.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
            float[] result = new float[tokens.Count];
            for (int i = 0; i < tokens.Count; i++)
            {
                result[i] = float.Parse(tokens[i]);
            }
            return result;
        }
    }
}
