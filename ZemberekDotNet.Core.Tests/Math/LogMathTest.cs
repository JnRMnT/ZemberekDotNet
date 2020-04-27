using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ZemberekDotNet.Core.Math;

namespace ZemberekDotNet.Core.Tests.Math
{
    [TestClass]
    public class LogMathTest
    {
        [TestMethod]
        public void LogSumTest()
        {
            double[] aLinear = new double[1000];
            double[] bLinear = new double[1000];

            for (int i = 0; i < bLinear.Length; i++)
            {
                aLinear[i] = (double)i / 1000d;
                bLinear[i] = aLinear[i];
            }

            LogMath.LogSumLookup logSumE = LogMath.LogSumL;

            foreach (double a in aLinear)
            {
                foreach (double b in bLinear)
                {
                    double logSumExpected = System.Math.Log(a + b);
                    Assert.AreEqual(logSumExpected, logSumE.Lookup(System.Math.Log(a), System.Math.Log(b)), 0.001);
                }
            }
        }

        [TestMethod]
        public void LogSumFloatTest()
        {
            float[]
            aLinear = new float[1000];
            float[] bLinear = new float[1000];

            for (int i = 0; i < bLinear.Length; i++)
            {
                aLinear[i] = (float)i / 1000f;
                bLinear[i] = aLinear[i];
            }

            foreach (float a in aLinear)
            {
                foreach (float b in bLinear)
                {
                    float logSumExpected = (float)System.Math.Log(a + b);
                    Assert
                        .AreEqual(logSumExpected, LogMath.LogSumL.Lookup(System.Math.Log(a), System.Math.Log(b)), 0.001);
                }
            }
        }

        [TestMethod]
        public void LogSumExactTest()
        {
            double[]
            aLinear = new double[1000];
            double[] bLinear = new double[1000];

            int j = 0;
            for (int i = 0; i < bLinear.Length; i++)
            {
                aLinear[j] = (double)i / 1000d;
                bLinear[j] = aLinear[j];
                j++;
            }

            foreach (double a in aLinear)
            {
                foreach (double b in bLinear)
                {
                    double logSumExact = LogMath.LogSum(System.Math.Log(a), System.Math.Log(b));
                    Assert.AreEqual(logSumExact, LogMath.LogSumL.Lookup(System.Math.Log(a), System.Math.Log(b)), 0.001,
                        "a=" + a + " b=" + b + " lin=" + System.Math.Log(a + b));
                }
            }
        }

        [TestMethod]
        [Ignore("Not a unit test")]
        public void LogSumPerf()
        {
            double[]
            loga = new double[15000];
            double[] logb = new double[15000];

            int j = 0;
            for (int i = 0; i < logb.Length; i++)
            {
                loga[j] = System.Math.Log((double)i / 100000d);
                logb[j] = loga[j];
                j++;
            }

            Stopwatch sw = Stopwatch.StartNew();
            foreach (double a in loga)
            {
                foreach (double b in logb)
                {
                    LogMath.LogSum(a, b);
                }
            }
            Console.WriteLine("Exact: " + sw.ElapsedMilliseconds);
            sw.Restart();
            foreach (double a in loga)
            {
                foreach (double b in logb)
                {
                    LogMath.LogSumL.Lookup(a, b);
                }
            }
            Console.WriteLine("Lookup: " + sw.ElapsedMilliseconds);
            sw.Stop();
        }

        [TestMethod]
        public void LogSumError()
        {

            int VALS = 1000000;

            double[]
            logA = new double[VALS];

            for (int i = 0; i < VALS; i++)
            {
                if (i == 0)
                {
                    logA[i] = LogMath.LogZero;
                }
                else
                {
                    logA[i] = System.Math.Log((double)i / VALS);
                }
            }

            Stopwatch sw = Stopwatch.StartNew();

            double maxError = 0;
            double a = 0;
            double b = 0;

            for (int i = 0; i < logA.Length; i++)
            {
                double la = logA[i];
                double lb = logA[logA.Length - i - 1];
                double exact = LogMath.LogSum(la, lb);
                double approx = LogMath.LogSumL.Lookup(la, lb);
                double error = System.Math.Abs(exact - approx);
                if (error > maxError)
                {
                    maxError = error;
                    a = la;
                    b = lb;
                }
            }

            Console.WriteLine("Max error: " + maxError);
            Console.WriteLine("Max error values: " + a + ":" + b);
            Assert.IsTrue(maxError < 0.0005);
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Stop();
        }

        [TestMethod]
        public void LogSumErrorFloat()
        {

            int VALS = 1000000;

            float[] logA = new float[VALS];

            for (int i = 0; i < VALS; i++)
            {
                if (i == 0)
                {
                    logA[i] = LogMath.LogZeroFloat;
                }
                else
                {
                    logA[i] = (float)System.Math.Log((double)i / VALS);
                }
            }

            Stopwatch sw = Stopwatch.StartNew();

            float maxError = 0;
            float a = 0;
            float b = 0;

            for (int i = 0; i < logA.Length; i++)
            {
                float la = logA[i];
                float lb = logA[logA.Length - i - 1];
                float exact = (float)LogMath.LogSum(la, lb);
                float approx = LogMath.LogSumFloat.Lookup(la, lb);
                float error = System.Math.Abs(exact - approx);
                if (error > maxError)
                {
                    maxError = error;
                    a = la;
                    b = lb;
                }
            }

            Console.WriteLine("Max error: " + maxError);
            Console.WriteLine("Max error values: " + a + ":" + b);
            Assert.IsTrue(maxError < 0.007);
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Stop();
        }

        [TestMethod]
        public void TestLog2()
        {
            Assert.AreEqual(2, LogMath.Log2(4), 0.0001);
            Assert.AreEqual(3, LogMath.Log2(8), 0.0001);
            Assert.AreEqual(10, LogMath.Log2(1024), 0.0001);
            Assert.AreEqual(-1, LogMath.Log2(0.5), 0.0001);
        }

        [TestMethod]
        public void LinearToLogTest()
        {
            LogMath.LinearToLogConverter converter = new LogMath.LinearToLogConverter(System.Math.E);
            Assert.AreEqual(System.Math.Log(2), converter.Convert(2), 0.00000001d);
            converter = new LogMath.LinearToLogConverter(10d);
            Assert.AreEqual(System.Math.Log10(2), converter.Convert(2), 0.00000001d);

        }
    }
}
