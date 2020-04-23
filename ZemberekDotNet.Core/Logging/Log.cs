using log4net;
using System;

namespace ZemberekDotNet.Core.Logging
{
    /// <summary>
    /// A convenient Log class.
    /// </summary>
    public class Log
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Log));

        public static void Warn(string message)
        {
            logger.Warn(message);
        }
    }
}
