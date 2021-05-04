using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DynHosts.Classes
{
    class Log
    {
        public static List<string> Errors = new List<string>() { };

        public static void Write(string text = "", bool toLogFile = true)
        {
            if (Environment.UserInteractive)
            {
                Console.Write(text);
            }

            if (toLogFile)
            {
                File.AppendAllText(LogFile, text);
            }
        }

        public static void WriteLine(int value)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(value);
            }
        }

        public static void WriteLine(string text = "", bool toLogFile = true)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(text);
            }

            if (toLogFile)
            {
                File.AppendAllText(LogFile, text + Environment.NewLine);
            }
        }

        public static void Flush()
        {
            WriteLine(string.Join(Environment.NewLine, Errors));
            Errors = new List<string>();
        }

        public static void DisplayHelp()
        {
            var executable = new FileInfo(Assembly.GetExecutingAssembly().Location).Name;
            var help = Program.HelpInfo;
            help = help.Replace("__FILE__", executable);
            help = help.Replace("__ERROR__",
                Log.Errors.Count > 0
                    ? "Errors: " + Environment.NewLine + string.Join(Environment.NewLine + "- ", Log.Errors)
                    : ""
            );
            Log.WriteLine(help, false);
        }

        public static string LogFile
        {
            get { return Assembly.GetExecutingAssembly().Location + ".log"; }
        }
    }
}