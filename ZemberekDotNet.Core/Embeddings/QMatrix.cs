using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace ZemberekDotNet.Core.Embeddings
{
    public class QMatrix
    {
        ProductQuantizer pq_;
        ProductQuantizer npq_;

        byte[] codes_;
        byte[] norm_codes_;

        bool qnorm_;

        int m_;
        int n_;

        int codesize_;

        public QMatrix()
        {
            m_ = 0;
            n_ = 0;
            codesize_ = 0;
        }

        public QMatrix(Matrix mat, int dsub, bool qnorm)
        {
            qnorm_ = qnorm;
            m_ = mat.m_;
            n_ = mat.n_;
            codesize_ = m_ * ((n_ + dsub - 1) / dsub);
            if (codesize_ > 0)
            {
                codes_ = new byte[codesize_];
            }
            else
            {
                throw new InvalidOperationException("Code size must be a positive number.");
            }
            pq_ = new ProductQuantizer(n_, dsub);
            if (qnorm_)
            {
                norm_codes_ = new byte[m_];
                npq_ = new ProductQuantizer(1, 1);
            }
            Quantize(mat);
        }


        public void quantizeNorm(Vector norms)
        {
            Contract.Assert(qnorm_);
            Contract.Assert(norms.Size() == m_);
            float[] dataptr = norms.data_;
            npq_.Train(m_, dataptr);
            ProductQuantizer.FArray fArray = new ProductQuantizer.FArray(dataptr);
            ProductQuantizer.BArray bArray = new ProductQuantizer.BArray(norm_codes_);
            npq_.ComputeNodes(fArray, bArray, m_);
        }

        public void Quantize(Matrix matrix)
        {
            Contract.Assert(n_ == matrix.n_);
            Contract.Assert(m_ == matrix.m_);
            if (qnorm_)
            {
                Vector norms = new Vector(matrix.m_);
                matrix.L2NormRow(norms);
                matrix.DivideRow(norms);
                quantizeNorm(norms);
            }
            float[] data1D = matrix.GetData1D();
            pq_.Train(m_, data1D);
            ProductQuantizer.FArray fArray = new ProductQuantizer.FArray(data1D);
            ProductQuantizer.BArray bArray = new ProductQuantizer.BArray(codes_);
            pq_.ComputeNodes(fArray, bArray, m_);
        }

        public void AddToVector(Vector x, int t)
        {
            float norm = 1;
            if (qnorm_)
            {
                norm = npq_.GetCentroids(0, norm_codes_[t]).Get(0);
            }
            pq_.AddCode(x, new ProductQuantizer.BArray(codes_), t, norm);
        }

        public float DotRow(Vector vec, int i)
        {
            Contract.Assert(i >= 0);
            Contract.Assert(i < m_);
            Contract.Assert(vec.Size() == n_);
            float norm = 1;
            if (qnorm_)
            {
                norm = npq_.GetCentroids(0, norm_codes_[i]).Get(0);
            }
            return pq_.MulCode(vec, new ProductQuantizer.BArray(codes_), i, norm);
        }

        public int GetM()
        {
            return m_;
        }

        public int GetN()
        {
            return n_;
        }

        public void Save(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(qnorm_);
            binaryWriter.Write(m_.EnsureEndianness());
            binaryWriter.Write(n_.EnsureEndianness());
            binaryWriter.Write(codesize_.EnsureEndianness());
            binaryWriter.Write(codes_);
            pq_.Save(binaryWriter);
            if (qnorm_)
            {
                binaryWriter.Write(norm_codes_);
                npq_.Save(binaryWriter);
            }
        }

        public void Load(BinaryReader binaryReader)
        {
            qnorm_ = binaryReader.ReadBoolean();
            m_ = binaryReader.ReadInt32().EnsureEndianness();
            n_ = binaryReader.ReadInt32().EnsureEndianness();
            codesize_ = binaryReader.ReadInt32().EnsureEndianness();
            codes_ = new byte[codesize_];
            binaryReader.Read(codes_);
            pq_ = new ProductQuantizer();
            pq_.Load(binaryReader);
            if (qnorm_)
            {
                byte[] normCodesData = new byte[m_];
                binaryReader.Read(normCodesData);
                npq_ = new ProductQuantizer();
                npq_.Load(binaryReader);
            }
        }
    }
}
