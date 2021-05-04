using System.ComponentModel;
using System.ServiceProcess;

namespace DynHosts
{
    [RunInstaller(true)]
    public sealed class ServiceInstallerProcess : ServiceProcessInstaller
    {
        public ServiceInstallerProcess()
        {
            Account = ServiceAccount.NetworkService;
        }
    }

}
