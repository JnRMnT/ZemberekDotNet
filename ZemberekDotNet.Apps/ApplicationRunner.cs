using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Apps
{
    public class ApplicationRunner
    {
        public static void Nain(string[] args)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            List<Type> apps = currentAssembly.GetTypes().Where(t => typeof(ConsoleApp<object>).IsAssignableFrom(t)).ToList();

            if (apps.Count == 0)
            {
                Console.WriteLine("No applications found.");
                Environment.Exit(0);
            }
            if (args.Length == 0)
            {
                ListApplications(apps);
                Environment.Exit(0);
            }
            string className = args[0];
            foreach (Type app in apps)
            {
                if (app.Name.Contains(className))
                {
                    string[] copiedArray = null;
                    Array.Copy(args, 1, copiedArray, 0, args.Length - 1);
                    MethodInfo executeMethod = app.GetMethod("Execute");
                    executeMethod.Invoke(null, copiedArray);
                    Environment.Exit(0);
                }
            }
            Console.WriteLine("Cannot find application for :" + className);
            ListApplications(apps);
        }

        private static void ListApplications(List<Type> apps)
        {
            Console.WriteLine("List of available applications:");
            Console.WriteLine("===============================");
            foreach (Type app in apps)
            {
                string simpleName = app.Name;
                Console.WriteLine(simpleName);
                Console.WriteLine(Strings.Repeat("-", simpleName.Length));
                MethodInfo descriptionMethod = app.GetMethod("Description");
                string wrapped = Wrap(descriptionMethod.Invoke(null, null) as string, 80);
                Console.WriteLine(wrapped);
                Console.WriteLine();
            }
            Environment.Exit(0);
        }

        internal static string Wrap(string s, int lineLength)
        {
            if (s == null)
            {
                return "";
            }
            List<string> paragrahs = s.Split("\n").ToList();
            List<string> result = new List<string>();
            foreach (string paragrah in paragrahs)
            {
                result.Add(WrapParagraph(paragrah, lineLength));
            }
            return string.Join("\n", result);
        }

        private static string WrapParagraph(string s, int lineLength)
        {
            List<string> lines = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (string token in s.Split(" "))
            {
                sb.Append(token).Append(" ");
                if (sb.Length >= lineLength)
                {
                    lines.Add(sb.ToString().Trim());
                    sb = new StringBuilder();
                }
            }
            if (sb.Length > 0)
            {
                lines.Add(sb.ToString().Trim());
            }
            return string.Join("\n", lines);
        }
    }
}
