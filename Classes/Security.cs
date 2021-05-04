using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using static System.Security.SecurityManager;

namespace DynHosts.Classes
{
    internal class Security
    {
        public RunArguments RunArguments { get; set; }

        public Security(RunArguments runArguments)
        {
            RunArguments = runArguments ?? throw new Exception("Missing value for RunArguments");
        }

        public void EnsureElevated()
        {
            if (!IsRunningAsAdmin())
                Elevate();
            else
                Log.WriteLine("Started with admin rights!");
        }

        public bool Elevate()
        {
            Log.WriteLine("Restarting elevated...");
            var selfProc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Assembly.GetExecutingAssembly().Location,
                Arguments = RunArguments.CliArguments(),
                Verb = "runas"
            };
            try
            {
                if (!RunArguments.RequestAdminRights)
                    throw new Exception("Failed to restart. Please restart with administrator rights.");
                Process.Start(selfProc);
                return true;
            }
            catch (Exception exception)
            {
                Log.Errors.Add(exception.Message);
                return false;
            }
        }

        public bool IsRunningAsAdmin()
        {
            var principle = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principle.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public bool HasWritPermissionOnTarget(string targetFile)
        {
            if (!File.Exists(targetFile)) throw new Exception("File " + targetFile + " does NotFiniteNumberException exist!");

            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, targetFile);

            // todo: fix!
            if (!IsGranted(writePermission)) EnsureElevated();

            return true;
        }
    }
}