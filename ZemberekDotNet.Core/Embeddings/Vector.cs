using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ZemberekDotNet.Core.Math;

namespace ZemberekDotNet.Core.Embeddings
{
    public class Vector
    {
        internal float[] data_;

        public Vector(int m_)
        {
            this.data_ = new float[m_];
        }

        public float[] GetData()
        {
            return data_;
        }

        public int Size()
        {
            return data_.Length;
        }

        public void Zero()
        {
            Array.Fill(data_, 0);
        }

        public void Mul(float a)
        {
            FloatArrays.ScaleInPlace(data_, a);
        }

        // Sums matrix[i] row values to this vector values.
        public void AddRow(Matrix A, int i)
        {
            Contract.Assert(i >= 0);
            Contract.Assert(i < A.m_);
            Contract.Assert(Size() == A.n_);
            for (int j = 0; j < A.n_; j++)
            {
                data_[j] += A.At(i, j);
            }
        }

        public void AddVector(Vector source)
        {
            Contract.Assert(Size() == source.Size());
            FloatArrays.AddToFirst(data_, source.data_);
        }

        public void AddRow(Matrix A, int i, float a)
        {
            Contract.Assert(i >= 0);
            Contract.Assert(i < A.m_);
            Contract.Assert(Size() == A.n_);
            for (int j = 0; j < A.n_; j++)
            {
                data_[j] += a * A.At(i, j);
            }
        }

        public void AddRow(QMatrix A, int i)
        {
            Contract.Assert(i >= 0);
            A.AddToVector(this, i);
        }

        public void Mul(Matrix A, Vector vec)
        {
            for (int i = 0; i < Size(); i++)
            {
                data_[i] = 0.0f;
                for (int j = 0; j < A.n_; j++)
                {
                    data_[i] += A.At(i, j) * vec.data_[j];
                }
            }
        }

        public float Norm()
        {
            float sum = 0f;
            foreach (float v in data_)
            {
                sum += v * v;
            }
            return (float)System.Math.Sqrt(sum);

        }

        public void Mul(QMatrix A, Vector vec)
        {
            Contract.Assert(A.GetM() == Size());
            Contract.Assert(A.GetN() == vec.Size());
            for (int i = 0; i < Size(); i++)
            {
                data_[i] = A.DotRow(vec, i);
            }
        }

        public int ArgMax()
        {
            float max = data_[0];
            int argmax = 0;
            for (int i = 1; i < Size(); i++)
            {
                if (data_[i] > max)
                {
                    max = data_[i];
                    argmax = i;
                }
            }
            return argmax;
        }

        public string AsString()
        {
            List<string> values = new List<string>(data_.Length);
            foreach (float v in data_)
            {
                values.Add(string.Format("%.6f", v));
            }
            return string.Join(" ", values);
        }
    }
}
