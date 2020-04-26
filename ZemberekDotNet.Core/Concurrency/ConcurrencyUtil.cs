using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Core.Concurrency
{
    public class ConcurrencyUtil
    {
        /// <summary>
        /// Validates cpu bound [threadCount] value. If number is larger than the N =
        /// Runtime.getRuntime().availableProcessors() it will set it to N.
        /// </summary>
        /// <param name="threadCount">input thread count value to verify.</param>
        /// <returns>if threadCount is positive and not larger than N, returns the same.</returns>
        public static int ValidateCpuThreadCount(int threadCount)
        {
            if (threadCount < 1)
            {
                throw new ArgumentException(
                    "Thread count cannot be less than 1. But it is " + threadCount);
            }
            int availableProcessors = Environment.ProcessorCount;
            if (threadCount > availableProcessors)
            {
                Log.Warn(
                    "Thread count %d is larger than the CPU count %d. Available CPU count %d will be used.",
                    threadCount, availableProcessors, availableProcessors);
                return availableProcessors;
            }
            else
            {
                return threadCount;
            }
        }

        public static int GetHalfCpuCount()
        {
            int availableProcessors = Environment.ProcessorCount;
            if (availableProcessors == 1)
            {
                return 1;
            }
            return availableProcessors / 2;
        }

        public static int GetMostCpuCount(int leaveCount)
        {
            if (leaveCount < 0)
            {
                throw new ArgumentException("Remaining count cannot be negative");
            }
            int availableProcessors = Environment.ProcessorCount;
            int count = availableProcessors - leaveCount;
            if (count <= 0)
            {
                return 1;
            }
            return count;
        }
    }
}
