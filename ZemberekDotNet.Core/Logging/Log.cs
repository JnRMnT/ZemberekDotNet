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

        public static void Warn(string message, params object[] parameters)
        {
            Warn(string.Format(message, parameters));
        }

        public static void Debug(string message)
        {
            logger.Debug(message);
        }

        public static void Debug(string message, params object[] parameters)
        {
            logger.Debug(string.Format(message, parameters));
        }

        public static void Info(string message)
        {
            logger.Info(message);
        }

        public static void Info(string message, params object[] parameters)
        {
            logger.Info(string.Format(message, parameters));
        }
    }
}
