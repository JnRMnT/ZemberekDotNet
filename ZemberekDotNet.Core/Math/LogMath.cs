using System;

namespace ZemberekDotNet.Core.Math
{
    public class LogMath
    {
        public static readonly double LogZero = -System.Math.Log(double.MaxValue);
        public static readonly double LogOne = 0;
        public static readonly double LogTen = System.Math.Log(10);
        public static readonly double LogTwo = System.Math.Log(2);
        public static readonly double InverseLogTwo = 1 / System.Math.Log(2);

        public static readonly float LogZeroFloat = (float)-System.Math.Log(float.MaxValue);
        public static readonly float LogOneFloat = 0;
        public static readonly float LogTenFloat = (float)System.Math.Log(10);
        public static readonly float LogTwoFloat = (float)System.Math.Log(2);

        // Double value log sum lookup base System.Math.E
        public static readonly LogSumLookup LogSumL = new LogSumLookup(System.Math.E);
        // Float value log sum lookup base System.Math.E
        public static readonly LogSumLookupFloat LogSumFloat = new LogSumLookupFloat(System.Math.E);

        // Double value linear to Log value converter for base System.Math.E
        public static readonly LinearToLogConverter LinearToLog = new LinearToLogConverter(System.Math.E);
        // Float value linear to Log value converter for base System.Math.E
        public static readonly LinearToLogConverterFloat LinearToLogFloat = new LinearToLogConverterFloat(
            System.Math.E);
        private static readonly double Sphinx4LogBase = 1.0001;
        private static readonly double InverseLogSphinxBase = 1 / System.Math.Log(Sphinx4LogBase);

        // do not allow instantiation
        private LogMath()
        {
        }

        /**
         * Exact calculation of log(a+b) using log(a) and log(b) with formula <p><b>log(a+b) = log(b) +
         * log(1 + exp(log(b)-log(a)))</b> where log(b)>log(a)
         *
         * @param logA logarithm of A
         * @param logB logarithm of B
         * @return approximation of log(A+B)
         */
        public static double LogSum(double logA, double logB)
        {
            if (double.IsInfinity(logA))
            {
                return logB;
            }
            if (double.IsInfinity(logB))
            {
                return logA;
            }
            if (logA > logB)
            {
                double dif = logA - logB;
                return dif >= 30d ? logA : logA + System.Math.Log(1 + System.Math.Exp(-dif));
            }
            else
            {
                double dif = logB - logA;
                return dif >= 30d ? logB : logB + System.Math.Log(1 + System.Math.Exp(-dif));
            }
        }

        /**
         * Exact calculation of log10(a+b) using log10(a) and log10(b) with formula <p><b>log10(a+b) =
         * log10(b) + log10(1 + 10^(log(b)-log(a)))</b> where log(b)>log(a)
         *
         * @param log10A 10 base logarithm of A
         * @param log10B 10 base logarithm of B
         * @return approximation of log(A+B)
         */
        public static double LogSum10(double log10A, double log10B)
        {
            if (double.IsInfinity(log10A))
            {
                return log10B;
            }
            if (double.IsInfinity(log10B))
            {
                return log10A;
            }
            if (log10A > log10B)
            {
                double dif = log10A - log10B;
                return dif >= 30d ? log10A : log10A + System.Math.Log10(1 + System.Math.Pow(10, -dif));
            }
            else
            {
                double dif = log10B - log10A;
                return dif >= 30d ? log10B : log10B + System.Math.Log10(1 + System.Math.Pow(10, -dif));
            }
        }

        /**
         * Calculates exact logSum of log values using the <code> logSum(logA,logB) </code>
         *
         * @param logValues log values to use in logSum calculation.
         * @return </p>log(a+b) value approximation
         */
        public static double LogSum(params double[] logValues)
        {
            double result = LogZero;
            foreach (double logValue in logValues)
            {
                result = LogSum(result, logValue);
            }
            return result;
        }

        /**
         * Calculates logarithm in any base.
         */
        public static double Log(double logBase, double val)
        {
            return System.Math.Log(val) / System.Math.Log(logBase);
        }

        /**
         * Calculates 2 base logarithm
         *
         * @param input value to calculate log
         * @return 2 base logarithm of the input
         */
        public static double Log2(double input)
        {
            return System.Math.Log(input) * InverseLogTwo;
        }

        /**
         * convert a value which is in log10 base to Log base.
         *
         * @param log10Value loog10 value.
         * @return loge values
         */
        public static double Log10ToLog(double log10Value)
        {
            return log10Value * LogTen;
        }

        /**
         * Converts a log value to Sphinx4 log base. Can be used for comparison.
         *
         * @param logValue value in natural logarithm
         * @return value in Sphinx4 log base.
         */
        public static double ToLogSphinx(double logValue)
        {
            return logValue * InverseLogSphinxBase;
        }

        /**
         * A lookup structure for approximate logSum calculation.
         */
        public class LogSumLookupFloat
        {
            public static readonly float DefaultScale = 1000f;
            public static readonly int DefaultLookupSize = 5000;
            public readonly float scale;
            private readonly float[] lookup;

            public LogSumLookupFloat(double logBase) : this(logBase, DefaultLookupSize, DefaultScale)
            {

            }

            public LogSumLookupFloat(double logBase, int lookupSize, float scale)
            {
                this.scale = scale;
                this.lookup = new float[lookupSize];
                for (int i = 0; i < lookup.Length; i++)
                {
                    lookup[i] = (float)Log(logBase, 1.0 + System.Math.Pow(logBase, (double)-i / scale));
                }
            }

            /**
             * Calculates an approximation of log(a+b) when log(a) and log(b) are given using the formula
             * <p><b>log(a+b) = log(b) + log(1 + exp(log(a)-log(b)))</b> where log(b)>log(a) <p>This method
             * is an approximation because it uses a lookup table for <b>log(1 + exp(log(b)-log(a)))</b>
             * part <p>This is useful for log-probabilities where values vary between -30 < log(p) <= 0
             * <p>if difference between values is larger than 20 (which means sum of the numbers will be
             * very close to the larger value in linear domain) large value is returned instead of the
             * logSum calculation because effect of the other value is negligible
             *
             * @param logA logarithm of A
             * @param logB logarithm of B
             * @return approximation of log(A+B)
             */
            public float Lookup(float logA, float logB)
            {
                if (logA > logB)
                {
                    float dif =
                        logA - logB; // logA-logB because during lookup calculation dif is multiplied with -1
                    return dif >= 5f ? logA : logA + lookup[(int)(dif * scale)];
                }
                else
                {
                    float dif = logB - logA;
                    return dif >= 5f ? logB : logB + lookup[(int)(dif * scale)];
                }
            }

            /**
             * Calculates approximate logSum of log values using the <code> logSum(logA,logB) </code>
             *
             * @param logValues log values to use in logSum calculation.
             * @return <p>log(a+b) value approximation
             */
            public float Lookup(params float[] logValues)
            {
                float result = LogZeroFloat;
                foreach (float logValue in logValues)
                {
                    result = Lookup(result, logValue);
                }
                return result;
            }
        }

        /**
         * A lookup structure for approximate logSum calculation.
         */
        public class LogSumLookup
        {
            public static readonly double DefaultScale = 1000d;
            public static readonly int DefaultLookupSize = 20000;
            public readonly double scale;
            private readonly double[] lookup;

            public LogSumLookup(double logBase): this(logBase, DefaultLookupSize, DefaultScale)
            {
                
            }

            public LogSumLookup(double logBase, int lookupSize, double scale)
            {
                this.scale = scale;
                this.lookup = new double[lookupSize];
                for (int i = 0; i < lookup.Length; i++)
                {
                    lookup[i] = Log(logBase, 1.0 + System.Math.Pow(logBase, (double)-i / scale));
                }
            }

            /**
             * Calculates an approximation of log(a+b) when log(a) and log(b) are given using the formula
             * <p><b>log(a+b) = log(b) + log(1 + exp(log(a)-log(b)))</b> where log(b)>log(a) <p>This method
             * is an approximation because it uses a lookup table for <b>log(1 + exp(log(b)-log(a)))</b>
             * part <p>This is useful for log-probabilities where values vary between -30 < log(p) <= 0
             * <p>if difference between values is larger than 20 (which means sum of the numbers will be
             * very close to the larger value in linear domain) large value is returned instead of the
             * logSum calculation because effect of the other value is negligible
             *
             * @param logA logarithm of A
             * @param logB logarithm of B
             * @return approximation of log(A+B)
             */
            public double Lookup(double logA, double logB)
            {
                if (logA > logB)
                {
                    double dif =
                        logA - logB; // logA-logB because during lookup calculation dif is multiplied with -1
                    double indexer = dif * scale;
                    return dif >= 20d ? logA : logA + lookup[double.IsNaN(indexer) ? 0 : (int)(indexer)];
                }
                else
                {
                    double dif = logB - logA;
                    double indexer = dif * scale;
                    return dif >= 20d ? logB : logB + lookup[double.IsNaN(indexer) ? 0 : (int)(indexer)];
                }
            }

            /**
             * Calculates approximate logSum of log values using the <code> logSum(logA,logB) </code>
             *
             * @param logValues log values to use in logSum calculation.
             * @return <p>log(a+b) value approximation
             */
            public double Lookup(params double[] logValues)
            {
                double result = LogZero;
                foreach (double logValue in logValues)
                {
                    result = Lookup(result, logValue);
                }
                return result;
            }
        }

        /**
         * A converter class for converting linear values log values.
         */
        public class LinearToLogConverter
        {
            public readonly double inverseLogBase;

            public LinearToLogConverter(double logBase)
            {
                if (logBase == 0)
                {
                    throw new ArgumentException("Base of the logarithm cannot be zero.");
                }
                this.inverseLogBase = (float)(1 / System.Math.Log(logBase));
            }

            public double Convert(double linear)
            {
                return System.Math.Log(linear) * inverseLogBase;
            }

            public double[] Convert(params double[] linear)
            {
                double[] result = new double[linear.Length];
                for (int i = 0; i < linear.Length; i++)
                {
                    result[i] = Convert(linear[i]);
                }
                return result;
            }

            public void ConvertInPlace(params double[] linear)
            {
                for (int i = 0; i < linear.Length; i++)
                {
                    linear[i] = Convert(linear[i]);
                }
            }
        }

        /// <summary>
        /// A converter class for converting linear values log values.
        /// </summary>
        public class LinearToLogConverterFloat
        {
            public readonly float inverseLogOfBase;

            public LinearToLogConverterFloat(double logBase)
            {
                if (logBase == 0)
                {
                    throw new ArgumentException("Base of the logarithm cannot be zero.");
                }
                this.inverseLogOfBase = (float)(1 / System.Math.Log(logBase));
            }

            public float Convert(float linear)
            {
                return (float)System.Math.Log(linear) * inverseLogOfBase;
            }

            public float[] Convert(params float[] linear)
            {
                float[] result = new float[linear.Length];
                for (int i = 0; i < linear.Length; i++)
                {
                    result[i] = Convert(linear[i]);
                }
                return result;
            }

            public void ConvertInPlace(params float[] linear)
            {
                for (int i = 0; i < linear.Length; i++)
                {
                    linear[i] = Convert(linear[i]);
                }
            }
        }
    }
}
