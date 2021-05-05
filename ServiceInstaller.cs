using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using DynHosts.Classes;

namespace DynHosts
{
    [RunInstaller(true)]
    public sealed class ServiceInstaller : System.ServiceProcess.ServiceInstaller
    {
        public ServiceInstaller()
        {
            Description = "Watches a file with hosts and sets the content to another host file that requires system permission to write to";
            DisplayName = "BPN-DynHosts-Updater";
            ServiceName = Service.name + "-v4";
            StartType = System.ServiceProcess.ServiceStartMode.Automatic;
        }
        
        public static void Install(string[] args)
        {
            handleInstallService(false, args);
        }
        
        public static void Uninstall(string[] args)
        {
            handleInstallService(true, args);
        }
        
        
        private static void handleInstallService(bool remove, string[] args)
        {
            try
            {
                using (var inst = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (remove)
                        {
                            Log.Write("Removing the service...");
                            inst.Uninstall(state);
                        }
                        else
                        {
                            Log.Write("Installing the service...");
                            inst.Install(state);
                        }
                        inst.Commit(state);
                        Log.WriteLine("Success");


                        if (!remove)
                        {
                            Log.WriteLine("");
                            Log.WriteLine("NOTICE The service is not yet active");
                            Log.WriteLine("");
                            Log.WriteLine("Please check your configuration before starting. ");
                            Log.WriteLine("To start the service, execute: " );
                            Log.WriteLine(typeof(Program).Assembly.Location + " start");
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.WriteLine("Failed: " + exception.Message);
                        try
                        {
                            Log.WriteLine("Rolling back...");
                            inst.Rollback(state);
                            Log.WriteLine("Rollback was successful");
                        }
                        catch (Exception exceptionRollBack)
                        {
                            Log.WriteLine("Rollback Failed: " + exceptionRollBack.Message);
                        }

                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
            }
        }
    }
}