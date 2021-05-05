using System;
using System.Runtime.InteropServices;
using DynHosts.Classes;
using System.ServiceProcess;

namespace DynHosts
{
    internal class Program
    {
        public static RunArguments RunArguments { get; set; }
        public static Watcher Watcher;
        protected static Security security;

        protected static Service selfInstaller;

        public static string HelpInfoHeader = @"Dynamic Hosts file Updater (__VERSION__)
(c) 2021 Bitpatroon";

        public const string HelpInfo = @"__FILE__ [arguments] 

arguments
    run                 Start then dynamic dns host updater
    -w <watchFile>      The file to watch 
    -t <targetFile>     The file to write to
    -ra                 Request Access automatically when not permitted

As a service:
    install             Install application as service
    uninstall           Remove application as service
    start               Starts the service
    stop                Stops the service
    restart             Restarts the service

    help/no argument    This help

__ERROR__
";

        private static void Main(string[] args)
        {
            RunArguments = new RunArguments();
            RunArguments.ProcessCliArguments(args);

            if (!Environment.UserInteractive)
            {
                if (selfInstaller == null) selfInstaller = new Service();

                var servicesToRun = new ServiceBase[]
                {
                    selfInstaller
                };
                ServiceBase.Run(servicesToRun);
            }
            else
            {
                // running as console app
                Start(args);
                Stop();
                Console.WriteLine("Finished.");
            }
        }

        #region program start/stop

        internal static void Start(string[] args)
        {
            try
            {
                security = new Security(RunArguments);
                if (selfInstaller == null) selfInstaller = new Service();


                switch (RunArguments.Command)
                {
                    case "i":
                    case "install":
                        ServiceInstaller.Install(args);
                        return;

                    case "u":
                    case "uninstall":
                        ServiceInstaller.Uninstall(args);
                        return;

                    case "start":
                        selfInstaller.StartService();
                        return;
                    case "stop":
                        selfInstaller.StopService();
                        return;
                    case "restart":
                        selfInstaller.StopService();
                        selfInstaller.StartService();
                        return;
                    case "run":
                        Run();
                        Log.WriteLine("Finished");
                        return;
                }
            }
            catch (Exception exception)
            {
                Log.Errors.Add(exception.Message);
            }

            Log.DisplayHelp();
        }

        internal static void Stop()
        {
            RunArguments.StopSignaled = true;
        }

        #endregion

        internal static void Run()
        {
            Watcher = new Watcher {RunArguments = RunArguments};
            Watcher.Run();
        }
    }
}