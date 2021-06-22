using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZemberekDotNet.Apps
{
    public static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
    }
}
