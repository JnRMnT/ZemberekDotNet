using System;
using System.Diagnostics.Contracts;
using System.IO;
using ZemberekDotNet.Core.Math;

namespace ZemberekDotNet.Core.Embeddings
{
    public class Matrix
    {
        public int m_;
        public int n_;
        public float[][] data_;

        private Matrix(int m_, int n_, float[][] data_)
        {
            this.m_ = m_;
            this.n_ = n_;
            this.data_ = data_;
        }

        private Matrix(int m_, int n_, float[] data_)
        {
            this.m_ = m_;
            this.n_ = n_;
            this.data_ = new float[m_][];
            for (int i = 0; i < m_; i++)
            {
                this.data_[i] = new float[n_];
                Array.Copy(data_, i * n_, this.data_[i], 0, n_);
            }
        }

        /// <summary>
        /// Generates a matrix that hs n_ columns and m_ rows.
        /// </summary>
        /// <param name="m_"></param>
        /// <param name="n_"></param>
        public Matrix(int m_, int n_)
        {
            this.m_ = m_;
            this.n_ = n_;
            this.data_ = new float[m_][];
            for (int i = 0; i < m_; i++)
            {
                this.data_[i] = new float[n_];
            }
        }

        public static readonly Matrix EMPTY = new Matrix(0, 0, new float[0]);

        public Matrix Copy()
        {
            return new Matrix(m_, n_, FloatArrays.Clone2D(data_));
        }

        public float[] GetData1D()
        {
            return FloatArrays.To1D(data_);
        }

        public void Set(int row, int col, float val)
        {
            data_[row][col] = val;
        }

        /// <summary>
        /// loads the values from binary stream [dis] and instantiates the matrix.
        /// </summary>
        /// <param name="dis"></param>
        /// <returns></returns>
        public static Matrix Load(BinaryReader dis)
        {
            int m_ = dis.ReadInt32();
            int n_ = dis.ReadInt32();
            float[][] data = new float[m_][];
            for (int i = 0; i < m_; i++)
            {
                data[i] = new float[n_];
            }

            int blockSize = n_ * 4;

            int block = 100_000 * blockSize;
            long totalByte = (long)m_ * blockSize;
            if (block > totalByte)
            {
                block = (int)totalByte;
            }
            int start = 0;
            int end = block / blockSize;
            int blockCounter = 1;
            while (start < m_)
            {
                byte[] b = new byte[block];
                dis.Read(b);
                float[] tmp = new float[block / 4];
                Buffer.BlockCopy(b, 0, tmp, 0, b.Length);

                for (int k = 0; k < tmp.Length / n_; k++)
                {
                    Array.Copy(tmp, k * n_, data[k + start], 0, n_);
                }
                blockCounter++;
                start = end;
                end = (block / blockSize) * blockCounter;
                if (end > m_)
                {
                    end = m_;
                    block = (end - start) * blockSize;
                }
            }
            return new Matrix(m_, n_, data);
        }

        /// <summary>
        ///  Fills the Matrix with uniform random numbers in [-a a] range.
        /// </summary>
        /// <param name="a"></param>
        public void Uniform(float a)
        {
            Random random = new Random(1);
            for (int i = 0; i < m_; i++)
            {
                for (int j = 0; j < n_; j++)
                {
                    float v = (float)(random.NextDouble() * 2 * a - a);
                    data_[i][j] = v;
                }
            }
        }

        public float At(int i, int j)
        {
            return data_[i][j];
        }

        /// <summary>
        /// Sums the [a]*values of Vector [vec] to the [i].th row of the Matrix.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="i"></param>
        /// <param name="a"></param>
        public void AddRow(Vector vec, int i, float a)
        {
            for (int j = 0; j < n_; j++)
            {
                data_[i][j] += a * vec.data_[j];
            }
        }

        /// <summary>
        /// Calculates dot product of Vector [vec] and [i]th row. If locks are enabled, access to the row  is thread safe.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public float DotRow(Vector vec, int i)
        {
            Contract.Assert(i >= 0);
            Contract.Assert(i < m_);
            Contract.Assert(vec.Size() == n_);
            float d = 0.0f;
            for (int j = 0; j < n_; j++)
            {
                d += data_[i][j] * vec.data_[j];
            }
            return d;
        }

        public void MultiplyRow(Vector nums)
        {
            MultiplyRow(nums, 0, -1);
        }

        /**
         * Multiplies values in rows of the matrix with values of `nums`. nums should have a value for
         * each row to be multiplied. If value in `nums` is zero, no multiplication is applied for the
         * row.
         *
         * @param nums values to multiply.
         * @param ib begin row index.
         * @param ie end row index. if -1, it is assumed row count `m_`
         */
        public void MultiplyRow(Vector nums, int ib, int ie)
        {
            if (ie == -1)
            {
                ie = m_;
            }
            Contract.Assert(ie <= nums.Size());
            for (int i = ib; i < ie; i++)
            {
                float n = nums.data_[i - ib];
                if (n != 0)
                {
                    for (int j = 0; j < n_; j++)
                    {
                        data_[i][j] *= n;
                    }
                }
            }
        }

        public void DivideRow(Vector denoms)
        {
            DivideRow(denoms, 0, -1);
        }

        /**
         * Divides values in rows of the matrix to values of `nums`. nums should have a value for each row
         * to be multiplied. If value in `nums` is zero, no division is applied for the row.
         *
         * @param denoms denominator values.
         * @param ib begin row index.
         * @param ie end row index. if -1, it is assumed row count `m_`
         */
        public void DivideRow(Vector denoms, int ib, int ie)
        {
            if (ie == -1)
            {
                ie = m_;
            }
            Contract.Assert(ie <= denoms.Size());
            for (int i = ib; i < ie; i++)
            {
                float n = denoms.data_[i - ib];
                if (n != 0)
                {
                    for (int j = 0; j < n_; j++)
                    {
                        data_[i][j] /= n;
                    }
                }
            }
        }

        /**
         * Calculates L2 Norm value of a row. Which is the Square root of  sum of squares of all row
         * values.
         *
         * @param i row index.
         * @return Square root of  sum of squares of all row values.
         */
        public float L2NormRow(int i)
        {
            float norm = 0f;
            for (int j = 0; j < n_; j++)
            {
                float v = data_[i][j];
                norm += v * v;
            }
            return (float)System.Math.Sqrt(norm);
        }

        /**
         * Fills the `norms` vector with l2 norm values of the rows.
         *
         * @param norms norm vector to fill.
         */
        public void L2NormRow(Vector norms)
        {
            Contract.Assert(norms.Size() == m_);
            for (int i = 0; i < m_; i++)
            {
                norms.data_[i] = L2NormRow(i);
            }
        }

        /// <summary>
        /// Saves values to binary stream [dos]
        /// </summary>
        /// <param name="dos"></param>
        public void Save(BinaryWriter dos)
        {
            dos.Write(m_);
            dos.Write(n_);

            int blockSize = n_ * 4;

            int block = 100_000 * blockSize;
            long totalByte = (long)m_ * blockSize;
            if (block > totalByte)
            {
                block = (int)totalByte;
            }
            int start = 0;
            int end = block / blockSize;
            int blockCounter = 1;
            while (start < m_)
            {
                byte[] b = new byte[block];
                int j = 0;
                for (int i = start; i < end; i++)
                {
                    Buffer.BlockCopy(data_[i], 0, b, j, data_[i].Length * 4);
                    j += data_[i].Length * 4;
                }
                dos.Write(b);
                blockCounter++;
                start = end;
                end = (block / blockSize) * blockCounter;
                if (end > m_)
                {
                    end = m_;
                    block = (end - start) * blockSize;
                }
            }
        }

        public void PrintRow(string s, int i, int amount)
        {
            int n = amount > n_ ? n_ : amount;
            Console.Write(s + "[" + i + "] = ");
            for (int k = 0; k < n; k++)
            {
                Console.Write(string.Format("%.4f ", data_[i][k]));
            }
            Console.WriteLine();
        }

    }
}
