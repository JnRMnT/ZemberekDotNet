using System;
using System.IO;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Embeddings
{
    public class ProductQuantizer
    {
        //TODO: original is 8 bits but because byte is signed, for now make it 7
        public static readonly int nbits_ = 7;
        public static readonly int ksub_ = 1 << nbits_;
        public static readonly int MaxPointsPerCluster = 128;
        public static readonly int MaxPoints = MaxPointsPerCluster * ksub_;
        public static readonly int seed_ = 1234;
        public static readonly int niter_ = 25;
        public static readonly float eps_ = 1e-7f;

        public int dim_;
        public int nsubq_;
        public int dsub_;
        public int lastdsub_;

        FArray centroids_;

        Random rng;

        public ProductQuantizer()
        {
        }

        // used for mimicking pointer arithmetic.
        public ProductQuantizer(int dim, int dsub)
        {
            this.dim_ = dim;
            this.nsubq_ = dim / dsub;
            dsub_ = dsub;
            centroids_ = new FArray(new float[dim * ksub_]);
            rng = new Random(seed_);
            lastdsub_ = dim_ % dsub;
            if (lastdsub_ == 0)
            {
                lastdsub_ = dsub_;
            }
            else
            {
                nsubq_++;
            }
        }

        public float DistL2(FArray x, FArray y, int d)
        {
            float dist = 0;
            for (int i = 0; i < d; i++)
            {
                float tmp = x.Get(i) - y.Get(i);
                dist += tmp * tmp;
            }
            return dist;
        }

        // TODO: Original code has two metohds for this, one const other not.
        public FArray GetCentroids(int m, byte i)
        {
            if (m == nsubq_ - 1)
            {
                return centroids_.Ref(m * ksub_ * dsub_ + i * lastdsub_);
            }
            return centroids_.Ref((m * ksub_ + i) * dsub_);
        }

        public float AssignCentroid(
            FArray x,
            FArray c0,
            BArray code,
            int d)
        {

            FArray c = c0.Ref(0);
            float dis = DistL2(x, c, d);
            code.Set(0, (byte)0);
            for (int j = 1; j < ksub_; j++)
            {
                c = c.Ref(d);
                float disij = DistL2(x, c, d);
                if (disij < dis)
                {
                    code.Set(0, (byte)j);
                    dis = disij;
                }
            }
            return dis;
        }

        public void Estep(
            FArray x,
            FArray centroids,
            BArray codes,
            int d,
            int n)
        {
            for (int i = 0; i < n; i++)
            {
                AssignCentroid(
                    x.Ref(i * d), centroids, codes.Ref(i), d);
            }
        }

        public void MStep(FArray x0,
            FArray centroids,
            BArray codes,
            int d,
            int n)
        {
            int[] nelts = new int[ksub_];
            centroids.Fill(0, d * ksub_, 0);

            FArray x = x0.Ref(0);
            FArray c;
            for (int i = 0; i < n; i++)
            {
                int k = codes.Get(i);
                c = centroids.Ref(k * d);
                for (int j = 0; j < d; j++)
                {
                    c.Add(j, x.Get(j));
                }
                nelts[k]++;
                x = x.Ref(d);
            }

            c = centroids.Ref(0);
            for (int k = 0; k < ksub_; k++)
            {
                float z = (float)nelts[k];
                if (z != 0)
                {
                    for (int j = 0; j < d; j++)
                    {
                        c.Divide(j, z);
                    }
                }
                c = c.Ref(d);
            }

            for (int k = 0; k < ksub_; k++)
            {
                if (nelts[k] == 0)
                {
                    int m = 0;
                    while (rng.NextFloat() * (n - ksub_) >= nelts[m] - 1)
                    {
                        m = (m + 1) % ksub_;
                    }

                    centroids.ArrayCopy(m * d, centroids, k * d, d);
                    for (int j = 0; j < d; j++)
                    {
                        int sign = (j % 2) * 2 - 1;
                        centroids.Add(k * d + j, sign * eps_);
                        centroids.Add(m * d + j, -sign * eps_);
                    }
                    nelts[k] = nelts[m] / 2;
                    nelts[m] -= nelts[k];
                }
            }
        }

        public void KMeans(FArray x, FArray c, int n, int d)
        {
            int[] values = new int[n];
            IntVector perm = new IntVector(values);

            for (int i = 0; i < n; i++)
            {
                perm.SafeSet(i, i);
            }
            perm.Shuffle(rng);
            for (int i = 0; i < ksub_; i++)
            {
                x.ArrayCopy( // x -> src
                    perm.Get(i) * d, // src pos
                    c,     // destination
                    i * d, // destination pos
                    d);    //amount of data
            }
            BArray codes = new BArray(new byte[n]);
            for (int i = 0; i < niter_; i++)
            {
                Estep(x, c, codes, d, n);
                MStep(x, c, codes, d, n);
            }
        }

        public void Train(int n, float[] x)
        {
            if (n < ksub_)
            {
                throw new ArgumentException(
                    "Matrix too small for quantization, must have > 256 rows. But it is " + n);
            }
            IntVector perm = new IntVector(new int[n]);
            for (int i = 0; i < n; i++)
            {
                perm.SafeSet(i, i);
            }
            int d = dsub_;
            int np = System.Math.Min(n, MaxPoints);

            float[] xslice = new float[np * dsub_];
            for (int m = 0; m < nsubq_; m++)
            {
                if (m == nsubq_ - 1)
                {
                    d = lastdsub_;
                }
                if (np != n)
                {
                    perm.Shuffle(rng);
                }
                for (int j = 0; j < np; j++)
                {
                    Array.Copy(x, perm.Get(j) * dim_ + m * dsub_, xslice, j * d, d);
                }
                KMeans(new FArray(xslice),
                    GetCentroids(m, (byte)0),
                    np,
                    d);
            }
        }

        public float MulCode(Vector x, BArray codes, int t, float alpha)
        {
            float res = 0;
            int d = dsub_;
            BArray code = codes.Ref(nsubq_ * t);
            for (int m = 0; m < nsubq_; m++)
            {
                FArray c = GetCentroids(m, code.Get(m));
                if (m == nsubq_ - 1)
                {
                    d = lastdsub_;
                }
                for (int n = 0; n < d; n++)
                {
                    res += x.data_[m * dsub_ + n] * c.Get(n);
                }
            }
            return res * alpha;
        }

        public void AddCode(Vector x, BArray codes, int t, float alpha)
        {
            int d = dsub_;
            BArray code = codes.Ref(nsubq_ * t);
            for (int m = 0; m < nsubq_; m++)
            {
                FArray c = GetCentroids(m, code.Get(m));
                if (m == nsubq_ - 1)
                {
                    d = lastdsub_;
                }
                for (int n = 0; n < d; n++)
                {
                    x.data_[m * dsub_ + n] += alpha * c.Get(n);
                }
            }
        }

        public void ComputeCode(FArray x, BArray code)
        {
            int d = dsub_;
            for (int m = 0; m < nsubq_; m++)
            {
                if (m == nsubq_ - 1)
                {
                    d = lastdsub_;
                }
                AssignCentroid(
                    x.Ref(m * dsub_),
                    GetCentroids(m, (byte)0),
                    code.Ref(m),
                    d);
            }
        }

        public void ComputeNodes(FArray x, BArray codes, int n)
        {
            for (int i = 0; i < n; i++)
            {
                ComputeCode(
                    x.Ref(i * dim_),
                    codes.Ref(i * nsubq_));
            }
        }

        public void Save(BinaryWriter dos)
        {
            dos.Write(dim_);
            dos.Write(nsubq_);
            dos.Write(dsub_);
            dos.Write(lastdsub_);
            for (int i = 0; i < centroids_.data.Length; i++)
            {
                dos.Write(centroids_.data[i]);
            }
        }

        public void Load(BinaryReader dis)
        {
            dim_ = dis.ReadInt32();
            nsubq_ = dis.ReadInt32();
            dsub_ = dis.ReadInt32();
            lastdsub_ = dis.ReadInt32();
            float[]
            centroidData = new float[dim_ * ksub_];
            for (int i = 0; i < centroidData.Length; i++)
            {
                centroidData[i] = dis.ReadSingle();
            }
            centroids_ = new FArray(centroidData);
        }

        public class FArray
        {
            internal int pointer;
            internal float[] data;

            public FArray(float[] data)
            {
                this.data = data;
                this.pointer = 0;
            }

            public FArray(int pointer, float[] data)
            {
                this.pointer = pointer;
                this.data = data;
            }

            public void Fill(int from, int to, float val)
            {
                Array.Fill(data, val, pointer + from, pointer + to);
            }

            public float Get(int i)
            {
                return data[pointer + i];
            }

            public void ArrayCopy(
                int srcPos,
                FArray dest,
                int destPos,
                int amount)
            {
                Array.Copy(
                    data,
                    pointer + srcPos,
                    dest.data,
                    dest.pointer + destPos,
                    amount);
            }

            public void Set(int i, float value)
            {
                data[pointer + i] = value;
            }

            public void Add(int i, float value)
            {
                data[pointer + i] += value;
            }

            public void Divide(int i, float value)
            {
                data[pointer + i] /= value;
            }

            public FArray Ref(int offset)
            {
                return new FArray(pointer + offset, this.data);
            }
        }
        public class BArray
        {
            internal int pointer;
            internal byte[] data;

            public BArray(byte[] data)
            {
                this.data = data;
                this.pointer = 0;
            }

            public BArray(int pointer, byte[] data)
            {
                this.pointer = pointer;
                this.data = data;
            }

            public byte Get(int i)
            {
                return data[pointer + i];
            }

            public void Set(int i, byte value)
            {
                data[pointer + i] = value;
            }

            public BArray Ref(int offset)
            {
                return new BArray(pointer + offset, this.data);
            }
        }
    }
}
