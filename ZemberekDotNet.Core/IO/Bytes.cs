using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace ZemberekDotNet.Core.IO
{
    /// <summary>
    /// Byte related low level functions
    /// </summary>
    public class Bytes
    {
        public static readonly byte[] EmptyByteArray = new byte[0];

        /**
         * converts an unsigned integer array to a byte array. this is useful when defining byte arrays in
         * the code without using (byte) casts. Note that integers sent to this method should be unsigned.
         * integer are sugggested to be written in hex format.
         *
         * @param uints an integer array formed from unsigned ints.
         * @return a byte array.
         * @throws IllegalArgumentException if an array item is smaller than zero or larger than 255
         * (0xff)
         */
        public static byte[] ToByteArray(params uint[] uints)
        {
            byte[] bytez = new byte[uints.Length];
            for (int i = 0; i < uints.Length; i++)
            {
                if (uints[i] > 255 || uints[i] < 0)
                {
                    throw new ArgumentException(
                        "Cannot convert to byte. Number should be between 0 and (255) 0xff. " +
                            "Number:" + uints[i]);
                }
                bytez[i] = (byte)(uints[i] & 0xff);
            }
            return bytez;
        }

        /**
         * converts a byte array to an integer. byte array may be with the lenght of 1,2,3 or 4.
         *
         * @param pb byte array
         * @param bigEndian endianness.
         * @return an integer represented byt the byte array
         * @throws IllegalArgumentException if byte array size is larger than 4
         */
        public static uint ToInt(byte[] pb, bool bigEndian)
        {
            int result;
            switch (pb.Length)
            {
                case 1:
                    result = pb[0] & 0xff;
                    break;
                case 2:
                    if (bigEndian)
                    {
                        result = (pb[0] << 8 & 0xff00) | (pb[1] & 0xff);
                        break;
                    }
                    else
                    {
                        result = (pb[1] << 8 & 0xff00) | (pb[0] & 0xff);
                        break;
                    }
                case 3:
                    if (bigEndian)
                    {
                        result = (pb[0] << 16 & 0xff0000) | (pb[1] << 8 & 0xff00) | (pb[2] & 0xff);
                        break;
                    }
                    else
                    {
                        result = (pb[2] << 16 & 0xff0000) | (pb[1] << 8 & 0xff00) | (pb[0] & 0xff);
                        break;
                    }
                case 4:
                    if (bigEndian)
                    {
                        result = ((int)(pb[0] << 24 & 0xff000000)) |
                            (pb[1] << 16 & 0xff0000) |
                            (pb[2] << 8 & 0xff00) |
                            (pb[3] & 0xff);
                        break;
                    }
                    else
                    {
                        result = (int)((pb[3] << 24 & 0xff000000)) |
                            (pb[2] << 16 & 0xff0000) |
                            (pb[1] << 8 & 0xff00) |
                            (pb[0] & 0xff);
                        break;
                    }
                default:
                    throw new ArgumentException("1,2,3 or 4 byte arrays allowed. size:" + pb.Length);
            }

            return (uint)result;
        }

        public static int Normalize(uint i, int bitCount)
        {
            int max = ((int)(0xffffffff >> (32 - bitCount)));
            if (i > max)
            {
                throw new ArgumentException("The integer cannot fit to bit boundaries.");
            }
            if (i > (max >> 1))
            {
                return ((int)i - (max + 1));
            }
            else
            {
                return (int)i;
            }
        }

        public static void Normalize(uint[] iarr, int bitCount)
        {
            for (int i = 0; i < iarr.Length; i++)
            {
                iarr[i] = (uint)Normalize(iarr[i], bitCount);
            }
        }


        public static byte[] ToByteArray(uint i, int size, bool isBigEndian)
        {
            switch (size)
            {
                case 1:
                    return new byte[] { (byte)i };
                case 2:
                    if (isBigEndian)
                    {
                        return new byte[] { (byte)(i >> 8 & 0xff), (byte)(i & 0xff) };
                    }
                    else
                    {
                        return new byte[] { (byte)(i & 0xff), (byte)(i >> 8 & 0xff) };
                    }
                case 3:
                    if (isBigEndian)
                    {
                        return new byte[] { (byte)(i >> 16 & 0xff), (byte)(i >> 8 & 0xff), (byte)(i & 0xff) };
                    }
                    else
                    {
                        return new byte[] { (byte)(i & 0xff), (byte)(i >> 8 & 0xff), (byte)(i >> 16 & 0xff) };
                    }
                case 4:
                    return ToByteArray(i, isBigEndian);
                default:
                    throw new ArgumentException("1,2,3 or 4 size values are allowed. size:" + size);
            }
        }

        /**
         * converts 4 bytes to an integer
         *
         * @param b0 first byte
         * @param b1 second byte
         * @param b2 third byte
         * @param b3 forth byte
         * @param bigEndian , if we want it in big endian format
         * @return integer formed from bytes.
         */
        public static uint ToInt(byte b0, byte b1, byte b2, byte b3, bool bigEndian)
        {
            if (bigEndian)
            {
                return (uint)((b0 << 24 & 0xff000000) |
                    (b1 << 16 & 0xff0000) |
                    (b2 << 8 & 0xff00) |
                    (b3 & 0xff));
            }
            else
            {
                return (uint)((b3 << 24 & 0xff000000) |
                    (b2 << 16 & 0xff0000) |
                    (b1 << 8 & 0xff00) |
                    (b0 & 0xff));
            }
        }

        /**
         * converts an integer to 4 byte array.
         *
         * @param i the number.
         * @param bigEndian endianness.
         * @return byte array generated from the integer.
         */
        public static byte[] ToByteArray(uint i, bool bigEndian)
        {
            byte[] ba = new byte[4];
            if (bigEndian)
            {
                ba[0] = (byte)(i >> 24);
                ba[1] = (byte)(i >> 16 & 0xff);
                ba[2] = (byte)(i >> 8 & 0xff);
                ba[3] = (byte)(i & 0xff);
            }
            else
            {
                ba[0] = (byte)(i & 0xff);
                ba[1] = (byte)(i >> 8 & 0xff);
                ba[2] = (byte)(i >> 16 & 0xff);
                ba[3] = (byte)(i >> 24);
            }
            return ba;
        }

        /**
         * converts a short to 2 byte array.
         *
         * @param i the number.
         * @param bigEndian endianness.
         * @return byte array generated from the short.
         */
        public static byte[] ToByteArray(ushort i, bool bigEndian)
        {
            byte[] ba = new byte[2];
            if (bigEndian)
            {
                ba[0] = (byte)(i >> 8);
                ba[1] = (byte)(i & 0xff);
            }
            else
            {
                ba[0] = (byte)(i & 0xff);
                ba[1] = (byte)(i >> 8 & 0xff);
            }
            return ba;
        }

        /**
         * Converts a byte array to an integer array. byte array length must be an order of 4.
         *
         * @param ba byte array
         * @param amount amount of bytes to convert to int.
         * @param bigEndian true if big endian.
         * @return an integer array formed form byte array.
         * @throws IllegalArgumentException if amount is smaller than 4, larger than byte array, or not an
         * order of 4.
         */
        public static uint[] ToIntArray(byte[] ba, int amount, bool bigEndian)
        {
            int size = DetermineSize(amount, ba.Length, 4);
            uint[] result = new uint[size / 4];
            int i = 0;
            for (int j = 0; j < size; j += 4)
            {
                if (bigEndian)
                {
                    result[i++] = ToInt(ba[j], ba[j + 1], ba[j + 2], ba[j + 3], true);
                }
                else
                {
                    result[i++] = ToInt(ba[j + 3], ba[j + 2], ba[j + 1], ba[j], true);
                }
            }
            return result;
        }


        /**
         * Converts a byte array to an integer array. byte array length must be an order of 4.
         *
         * @param ba byte array
         * @param amount amount of bytes to convert to int.
         * @param bytePerInteger byte count per integer.
         * @param bigEndian true if big endian.
         * @return an integer array formed form byte array.
         * @throws IllegalArgumentException if amount is smaller than 4, larger than byte array, or not an
         * order of 4.
         */
        public static uint[] ToIntArray(byte[] ba, int amount, int bytePerInteger,
            bool bigEndian)
        {
            int size = DetermineSize(amount, ba.Length, bytePerInteger);
            uint[] result = new uint[size / bytePerInteger];
            int i = 0;
            byte[] bytez = new byte[bytePerInteger];
            for (int j = 0; j < size; j += bytePerInteger)
            {
                Array.Copy(ba, j, bytez, 0, bytePerInteger);
                if (bigEndian)
                {
                    result[i++] = ToInt(bytez, true);
                }
                else
                {
                    result[i++] = ToInt(bytez, false);
                }
            }
            return result;
        }

        /**
         * Converts a byte array to an integer array. byte array length must be an order of 4.
         *
         * @param ba byte array
         * @param amount amount of bytes to convert to int.
         * @param bytePerInteger byte count per integer.
         * @param bitAmount bit count where the value will be mapped.
         * @param bigEndian true if big endian.
         * @return an integer array formed form byte array.
         * @throws IllegalArgumentException if amount is smaller than 4, larger than byte array, or not an
         * order of 4.
         */
        public static uint[] ToReducedBitIntArray(
            byte[] ba,
            int amount,
            int bytePerInteger,
            int bitAmount,
            bool bigEndian)
        {
            int size = DetermineSize(amount, ba.Length, bytePerInteger);
            uint[] result = new uint[size / bytePerInteger];
            int i = 0;
            byte[] bytez = new byte[bytePerInteger];
            for (int j = 0; j < size; j += bytePerInteger)
            {
                Array.Copy(ba, j, bytez, 0, bytePerInteger);
                if (bigEndian)
                {
                    result[i++] = (uint)Normalize(ToInt(bytez, true), bitAmount);
                }
                else
                {
                    result[i++] = (uint)Normalize(ToInt(bytez, false), bitAmount);
                }
            }
            return result;
        }


        private static int DetermineSize(int amount, int arrayLength, int order)
        {
            if (amount < order || amount > arrayLength)
            {
                throw new ArgumentException(
                    "amount of bytes to read cannot be smaller than " + order +
                        " or larger than array length. Amount is:" + amount);
            }

            int size = amount < arrayLength ? amount : arrayLength;
            if (size % order != 0)
            {
                throw new ArgumentException(
                    "array size must be an order of " + order + ". The size is:" + arrayLength);
            }

            return size;
        }

        /**
         * Converts a byte array to a short array. byte array length must be an order of 2.
         *
         * @param ba byte array
         * @param amount amount of bytes to convert to short.
         * @param bigEndian true if big endian.
         * @return a short array formed from byte array.
         * @throws IllegalArgumentException if amount is smaller than 2, larger than byte array, or not an
         * order of 2.
         */
        public static ushort[] ToShortArray(byte[] ba, int amount, bool bigEndian)
        {
            int size = DetermineSize(amount, ba.Length, 2);
            ushort[] result = new ushort[size / 2];
            int i = 0;
            for (int j = 0; j < size; j += 2)
            {
                if (bigEndian)
                {
                    result[i++] = (ushort)(ba[j] << 8 & 0xff00 | ba[j + 1] & 0xff);
                }
                else
                {
                    result[i++] = (ushort)(ba[j + 1] << 8 & 0xff00 | ba[j] & 0xff);
                }
            }
            return result;
        }

        /**
         * Converts a given array of shorts to a byte array.
         *
         * @param sa short array
         * @param amount amount of data to convert from input array
         * @param bigEndian if it is big endian
         * @return an array of bytes converted from the input array of shorts. 0xBABE becomes 0xBA, 0xBE
         * (Big Endian) or 0xBE, 0xBA (Little Endian)
         */
        public static byte[] ToByteArray(ushort[] sa, int amount, bool bigEndian)
        {
            int size = amount < sa.Length ? amount : sa.Length;
            byte[] result = new byte[size * 2];
            for (int j = 0; j < size; j++)
            {
                byte bh = (byte)(sa[j] >> 8);
                byte bl = (byte)(sa[j] & 0xff);
                if (bigEndian)
                {
                    result[j * 2] = bh;
                    result[j * 2 + 1] = bl;
                }
                else
                {
                    result[j * 2] = bl;
                    result[j * 2 + 1] = bh;
                }
            }
            return result;
        }

        /**
         * Converts a given array of ints to a byte array.
         *
         * @param ia <code>int</code> array
         * @param amount Amount of data to be converted from input array
         * @param bytePerInteger Byte count per integer.
         * @param bigEndian If it is big endian
         * @return an array of bytes converted from the input array of shorts. when bytePerInteger = 2,
         * ia = {0x0000CAFE, 0x0000BABE} returns {0xCA, 0xFE, 0xBA, 0xBE} (Big Endian) returns {0xFE,
         * 0xCA, 0xBE, 0xBA } (Little Endian) when bytePerInteger=4, ia = {0xCAFEBABE} return  {0xCA,
         * 0xFE, 0xBA, 0xBE} (Big Endian) returns { 0xBE, 0xBA, 0xFE, 0xCA} (Little Endian)
         */
        public static byte[] ToByteArray(uint[] ia, int amount, int bytePerInteger, bool bigEndian)
        {

            if (bytePerInteger < 1 || bytePerInteger > 4)
            {
                throw new ArgumentException(
                    "bytePerInteger parameter can only be 1,2,3 or 4. But it is:" + bytePerInteger);
            }
            if (amount > ia.Length || amount < 0)
            {
                throw new ArgumentException(
                    "Amount cannot be negative or more than input array length. Amount:" + amount);
            }

            int size = amount < ia.Length ? amount : ia.Length;
            byte[] result = new byte[size * bytePerInteger];
            for (int j = 0; j < size; j++)
            {
                byte[] piece = ToByteArray(ia[j], bytePerInteger, bigEndian);
                Array.Copy(piece, 0, result, j * bytePerInteger, bytePerInteger);
            }
            return result;
        }

        /**
         * converts a byte to a hexadecimal string with special xx formatting. it always return two
         * characters
         * <pre>
         * <p>for 0 , returns "00"
         * <p>for 1..15 returns "00".."0f"
         * </pre>
         *
         * @param b byte
         * @return hex string.
         */
        public static string ToHexWithZeros(byte b)
        {
            if (b == 0)
            {
                return "00";
            }
            string s = ToHex(b);
            if (s.Length == 1)
            {
                return "0" + s;
            }
            else
            {
                return s;
            }
        }

        /**
         * converts a byte to a hexadecimal string. it eliminates left zeros.
         * <pre>
         * <p>for 0 , returns "0"
         * <p>for 1 to 15 returns "0".."f"
         * </pre>
         *
         * @param b byte
         * @return hex string.
         */
        public static string ToHex(byte b)
        {
            return string.Format("{0:X}", b).ToLowerInvariant();
        }

        /**
         * converts byte array to a hexadecimal string. it ignores the zeros on the left side.
         * <pre>
         * <p>{0x00, 0x0c, 0x11, 0x01, 0x00} -> "c110100"
         * </pre>
         *
         * @param bytes byte array, should be non-null, and not empty.
         * @return a string representation of the number represented by the byte array. empty string is
         * byte array is empty.
         * @throws NullPointerException if byte array is null
         */
        public static string ToHex(byte[] bytes)
        {
            Contract.Requires(bytes != null, "byte array cannot be null.");
            if (bytes.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            bool nonZeroFound = false;
            foreach (byte b in bytes)
            {
                if (!nonZeroFound)
                {
                    if (b != 0)
                    {
                        builder.Append(ToHex(b));
                        nonZeroFound = true;
                    }
                    continue;
                }
                builder.Append(ToHexWithZeros(b));
            }
            //if all bytes are zero, loop above produces nothing. so we return "0"
            if (builder.Length == 0 && bytes.Length > 0)
            {
                return "0";
            }
            else
            {
                return builder.ToString();
            }
        }

        /**
         * converts a byte array to a hexadecimal string with special xx formatting. it does not ignore
         * the left zeros.
         * <pre>
         * <p>{0x00, 0x0c, 0x11, 0x00} -> "000c1100"
         * <pre>
         *
         * @param bytes byte array
         * @return hex string.  empty string is byte array is empty.
         * @throws NullPointerException if byte array is null
         */
        public static string ToHexWithZeros(byte[] bytes)
        {
            Contract.Requires(bytes != null, "byte array cannot be null.");
            if (bytes.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                builder.Append(ToHexWithZeros(b));
            }
            return builder.ToString();
        }

        /**
         * dumps the bytes to an Outputstream.
         *
         * @param os outputstream.
         * @param bytes bytes to dump
         * @param columns column number
         * @throws java.io.IOException if an error occurs while writing.
         */
        public static void HexDump(StreamWriter os, byte[] bytes, int columns)
        {
            Dumper.HexDump(new MemoryStream(bytes), os, columns, bytes.Length);
        }

        /**
         * dumps the bytes to Console.
         *
         * @param bytes bytes to dump
         * @param columns column number
         * @throws java.io.IOException if an error occurs while writing.
         */
        public static void HexDump(byte[] bytes, int columns)
        {
            Dumper.HexDump(new MemoryStream(bytes), Console.Out, columns, bytes.Length);
        }
    }
}
