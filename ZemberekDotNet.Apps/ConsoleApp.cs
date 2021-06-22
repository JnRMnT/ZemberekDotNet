using Commander.NET;
using Commander.NET.Exceptions;
using log4net.Config;
using System;

namespace ZemberekDotNet.Apps
{
    public abstract class ConsoleApp<T> where T : new()
    {
        public abstract string Description();

        public abstract void Run();

        public void Execute(params string[] args)
        {
            CommanderParser<T> parser = new CommanderParser<T>();
            try
            {
                T initializedClass = parser.Add(args).Parse();
                (initializedClass as ConsoleApp<T>).Run();
            }
            catch (ParameterFormatException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Description: ");
                Console.WriteLine(ApplicationRunner.Wrap(Description(), 80));
                Console.WriteLine();
                parser.Usage();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                parser.Usage();
            }
        }
    }
}
