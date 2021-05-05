using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynHosts.Properties;

namespace DynHosts.Classes
{
    internal class RunArguments
    {
        public string[] startupArgs = new string[0];
        public bool RequestAdminRights = false;
        public bool StopSignaled = false;
        public string WatchFile { get; set; }
        public string TargetFile { get; set; }
        public DateTime LastChangeTime = new DateTime();
        public string Command { get; set; }
        public bool AsService = false;
        public bool FullDateTimeLog { get; set; }

        public RunArguments()
        {
            WatchFile = Settings.Default.watchFile;
            TargetFile = Settings.Default.targetFile;
            FullDateTimeLog = Settings.Default.logWithFullDateTime;
        }

        public void ProcessCliArguments(string[] args)
        {
            startupArgs = args;

            Log.WriteLine(Program.HelpInfoHeader, false);
            Log.WriteLine();


            var index = 0;
            while (index < args.Length)
            {
                var arg = args[index];
                switch (arg)
                {
                    case "install":
                    case "uninstall":
                    case "start":
                    case "stop":
                    case "restart":
                    case "run":
                        Command = arg;
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
        }

        public string CliArguments()
        {
            return string.Join(" ", startupArgs ?? new string[0]);
        }
    }
}