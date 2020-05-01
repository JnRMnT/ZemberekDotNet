using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ZemberekDotNet.Core.Native.Collections
{
    public class BitArray
    {
        private readonly bool[] bits;
        public BitArray(IEnumerable<bool> bits)
        {
            this.bits = bits.ToArray();
        }
        public BitArray(int length)
        {
            this.bits = new bool[length];
        }

        public int Length { get { return bits.Length; } }

        public static BitArray ParseHex(string hex)
        {
            if (hex == null) return null; // or do something else, throw, ...

            List<bool> bits = new List<bool>();
            for (int i = 0; i < hex.Length; i++)
            {
                int b = byte.Parse(hex[i].ToString(), NumberStyles.HexNumber);
                bits.Add((b >> 3) == 1);
                bits.Add(((b & 0x7) >> 2) == 1);
                bits.Add(((b & 0x3) >> 1) == 1);
                bits.Add((b & 0x1) == 1);
            }
            BitArray ba = new BitArray(bits.ToArray());
            return ba;
        }
        public static BitArray FromBytes(IEnumerable<byte> bytes)
        {
            List<bool> bits = new List<bool>(bytes.Count() * 8);
            foreach (byte b in bytes)
            {
                byte v = b;
                for (int i = 7; i >= 0; i--)
                {
                    bits.Add((v & (1 << i)) != 0);
                }
            }
            return new BitArray(bits);
        }
        public static BitArray operator +(BitArray a, BitArray b)
        {
            return a.Append(b);
        }
        public static BitArray FromByte(byte b)
        {
            return BitArray.FromBytes(new byte[] { b });
        }
        public static BitArray FromString(string s, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.ASCII;
            return BitArray.FromBytes(encoding.GetBytes(s));
        }

        public BitArray Append(BitArray bits)
        {
            List<bool> obits = new List<bool>(this.ToArray());
            obits.AddRange(bits.ToArray());
            return new BitArray(obits.ToArray());
        }
        public BitArray Repeat(int numReps)
        {
            BitArray dv = new BitArray(0);
            while (--numReps >= 0) dv = dv.Append(this);
            return dv;
        }
        public BitArray GetBits(int startBit, int numBits = -1)
        {
            if (numBits == -1) numBits = bits.Length;
            return new BitArray(bits.Skip(startBit).Take(numBits).ToArray());
        }
        public BitArray SetBits(int startBit, BitArray setBits)
        {
            bool[] obits = bits.ToArray();
            bool[] nbits = setBits.ToArray();
            nbits.CopyTo(obits, startBit);
            return new BitArray(obits);
        }
        public BitArray Increment(int v = 1)
        {
            byte[] bytes = ToBytes();
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                int nv = (int)bytes[i] + v;
                bytes[i] = (byte)(nv % 256);
                if (nv < 256) break;
                v = nv - 256;
            }

            return BitArray.FromBytes(bytes);
        }
        public BitArray Left(int numBits)
        {
            return new BitArray(bits.Take(numBits));
        }
        public BitArray Xor(BitArray xor, int start = 0)
        {
            bool[] allbits = this.ToArray();
            bool[] xorbits = xor.ToArray();
            for (int i = 0; i < xorbits.Length; i++)
            {
                if (start + i >= allbits.Length) break;

                allbits[start + i] = allbits[start + i] ^ xorbits[i];
            }
            return new BitArray(allbits);
        }
        public List<BitArray> Split(int numBits)
        {
            int i = 0;
            List<BitArray> bitSplits = new List<BitArray>();
            while (i < bits.Length)
            {
                bitSplits.Add(this.GetBits(i, numBits));
                i += numBits;
            }
            return bitSplits;
        }

        public string ToAsciiString()
        {
            return Encoding.ASCII.GetString(ToBytes());
        }
        public string ToHexString(string bitSep8 = null, string bitSep128 = null)
        {
            string s = string.Empty;
            int b = 0;

            for (int i = 1; i <= bits.Length; i++)
            {
                b = (b << 1) | (bits[i - 1] ? 1 : 0);
                if (i % 4 == 0)
                {
                    s = s + string.Format("{0:x}", b);
                    b = 0;
                }

                if (i % (8 * 16) == 0)
                {
                    s = s + bitSep128;
                }
                else if (i % 8 == 0)
                {
                    s = s + bitSep8;
                }
            }
            int ebits = bits.Length % 4;
            if (ebits != 0)
            {
                b = b << (4 - ebits);
                s = s + string.Format("{0:x}", b);
            }
            return s;
        }

        public override string ToString()
        {
            return ToHexString(" ", " | ");
        }
        public byte[] ToBytes(int startBit = 0, int numBits = -1)
        {
            if (numBits == -1) numBits = bits.Length - startBit;
            BitArray ba = GetBits(startBit, numBits);
            int nb = (numBits / 8) + (((numBits % 8) > 0) ? 1 : 0);
            byte[] bb = new byte[nb];
            for (int i = 0; i < ba.Length; i++)
            {
                if (!bits[i]) continue;
                int bp = 7 - (i % 8);
                bb[i / 8] = (byte)((int)bb[i / 8] | (1 << bp));
            }
            return bb;
        }
        public bool[] ToArray()
        {
            return bits.ToArray();
        }
    }
}
