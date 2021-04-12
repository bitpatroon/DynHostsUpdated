using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using DynHosts.Properties;
using static System.Security.SecurityManager;

namespace DynHosts
{
    class Program
    {
        protected static DateTime LastChangeTime = new DateTime();
        protected static string WatchFile = null;
        public static string TargetFile = null;
        protected static bool RequestAdminRights = false;
        protected static List<string> Errors = new List<string>() { };

        protected static string[] startupArgs;

        protected static string HelpInfoHeader = @"Dynamic Hosts file Updater
(c) 2021 Bitpatroon";

        protected const string HelpInfo = @"
__FILE__ [arguments] 

arguments
    run                Start then dynamic dns host updater
    -w <watchFile>     The file to watch 
    -t <targetFile>    The file to write to
    -ra                Request Access automatically when not permitted
    help/no argument   This help

__ERROR__
";

        static void Main(string[] args)
        {
            startupArgs = args;

            Console.WriteLine(HelpInfoHeader);
            Console.WriteLine();

            var command = "";

            var index = 0;
            while (index < args.Length)
            {
                var arg = args[index];
                switch (arg)
                {
                    case "run":
                        command = arg;
                        break;

                    case "-w":
                        ++index;
                        WatchFile = args[index];
                        break;

                    case "-t":
                        ++index;
                        TargetFile = args[index];
                        break;

                    case "-ra":
                        RequestAdminRights = true;
                        break;
                }

                index++;
            }

            switch (command)
            {
                case "run":
                    Run();
                    Console.WriteLine("Stopped");
                    return;
            }

            DisplayHelp();
        }

        private static void DisplayHelp()
        {
            var help = HelpInfo;
            var executable = new FileInfo(Assembly.GetExecutingAssembly().Location).Name;
            help = help.Replace("__FILE__", executable);
            help = help.Replace("__ERROR__",
                "Errors: " + Environment.NewLine + string.Join(Environment.NewLine + "- ", Errors));
            Console.WriteLine(help);
        }

        private static void Run()
        {
            WatchFile = WatchFile ?? Settings.Default.watchFile;
            TargetFile = TargetFile ?? Settings.Default.targetFile;
            if (File.Exists(TargetFile))
            {
                var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, TargetFile);
                if (!IsGranted(writePermission))
                {
                    EnsureElevated();
                }
            }

            if (File.Exists(WatchFile))
            {
                LastChangeTime = File.GetLastWriteTime(WatchFile);
            }

            Console.WriteLine("Watching " + WatchFile);
            Console.WriteLine("Targeting " + TargetFile);
            Console.WriteLine();
            Console.WriteLine("Press ESC to quit.");

            var stopSignalled = false;

            while (!stopSignalled)
            {
                // check if the file was changed
                if (File.Exists(WatchFile))
                {
                    LastChangeTime = File.GetLastWriteTime(WatchFile);
                    if (LastChangeTime > File.GetLastWriteTime(TargetFile))
                    {
                        Console.Write(
                            $"{DateTime.Now.ToShortTimeString()} Change detected; Updating {TargetFile} ... ");
                        CopyWatchFileInTarget();
                        Console.WriteLine("[Done]");
                    }
                }

                System.Threading.Thread.Sleep(1000);
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Escape:
                            stopSignalled = true;
                            Console.WriteLine("Quitting...");
                            break;
                    }
                }
            }
        }

        private static void CopyWatchFileInTarget()
        {
            // SearchFor the marker

            var commentPrefix = Settings.Default.commentPrefix ?? "; ";
            var markerStart = $"{commentPrefix} ---- DynHostsStart";
            var markerEnd = $"{commentPrefix} ---- DynHostsEnd";

            var content = File.ReadAllText(TargetFile);
            if (content.IndexOf(markerStart, StringComparison.Ordinal) >= 0)
            {
                // Remove the content 

                var markerStartRegex = Regex.Escape(markerStart);
                var markerEndRegex = Regex.Escape(markerEnd);
                var regex = new Regex("(\r?\n)*" + markerStartRegex + ".*" + markerEndRegex + "(\r?\n)*",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                var matches = regex.Match(content);
                content = regex.Replace(content, "");
            }

            var watchFileContent = File.ReadAllText(WatchFile);
            content = string.Format("{0}{1}{1}{2}{1}{3}{1}{4}{1}", content, Environment.NewLine, markerStart,
                watchFileContent, markerEnd);
            File.WriteAllText(TargetFile, content);
        }


        private static void EnsureElevated()
        {
            if (!IsRunningAsAdmin())
            {
                Elevate();
            }
            else
            {
                Console.WriteLine("Started with admin rights!");
            }
        }

        private static bool Elevate()
        {
            Console.WriteLine("Restarting elevated...");
            var selfProc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Assembly.GetExecutingAssembly().Location,
                Arguments = startupArgs !=null ? string.Join(" ", startupArgs) : "",
                Verb = "runas"
            };
            try
            {
                if (!RequestAdminRights)
                    throw new Exception("Failed to restart. Please restart with administrator rights.");
                Process.Start(selfProc);
                return true;
            }
            catch
            {
                Errors.Add("Failed to restart. Please restart with administrator rights.");
                return false;
            }
        }

        internal static bool IsRunningAsAdmin()
        {
            var principle = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principle.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}