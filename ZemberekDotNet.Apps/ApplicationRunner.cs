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
        public static void Main(string[] args)
        {
            ResourceBootstrap.EnsureGlobalResourcesRoot();

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            List<Type> apps = currentAssembly.GetTypes()
                .Where(IsConsoleAppType)
                .OrderBy(t => t.Name)
                .ToList();

            if (apps.Count == 0)
            {
                Console.WriteLine("No applications found.");
                return;
            }
            if (args.Length == 0)
            {
                ListApplications(apps);
                return;
            }
            string className = args[0];
            foreach (Type app in apps)
            {
                if (app.Name.Contains(className, StringComparison.OrdinalIgnoreCase))
                {
                    object appInstance = Activator.CreateInstance(app);
                    if (appInstance == null)
                    {
                        Console.WriteLine("Cannot create application instance for :" + className);
                        return;
                    }

                    string[] copiedArray = args.Skip(1).ToArray();
                    MethodInfo executeMethod = app.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);
                    executeMethod?.Invoke(appInstance, new object[] { copiedArray });
                    return;
                }
            }
            Console.WriteLine("Cannot find application for :" + className);
            ListApplications(apps);
        }

        private static bool IsConsoleAppType(Type type)
        {
            if (type.IsAbstract)
            {
                return false;
            }

            Type current = type;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ConsoleApp<>))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
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
                object appInstance = Activator.CreateInstance(app);
                MethodInfo descriptionMethod = app.GetMethod("Description", BindingFlags.Public | BindingFlags.Instance);
                string wrapped = Wrap(descriptionMethod?.Invoke(appInstance, null) as string, 80);
                Console.WriteLine(wrapped);
                Console.WriteLine();
            }
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
