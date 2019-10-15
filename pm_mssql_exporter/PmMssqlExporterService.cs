using Pm.Log;
using Pm.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace pm_mssql_exporter
{
    public class PmMssqlExporterService : ServiceCommon
    {
        public PmMssqlExporterService(IPmServiceInstance serviceInstance, IPmLogger log) : base(serviceInstance, log)
        {
            base.InitializeComponent();
        }
    }


    /// <summary>
    /// Description of windows service
    /// </summary>
    public class PmMssqlExporterServiceMetadata : IPmServiceMetadata
    {
        string IPmServiceMetadata.ServiceName => "pm_mssql_exporter";
        string IPmServiceMetadata.ServiceDisplayName => "Metric exporter for MSSQL";
        string IPmServiceMetadata.ServiceDescription => "MSSQL metric exporter for prometheus";
        ServiceAccount IPmServiceMetadata.Account => ServiceAccount.NetworkService;
        string IPmServiceMetadata.AppName => "pm_mssql_exporter";
    }

    /// <summary>
    /// For use InstallUtil
    /// </summary>
    [RunInstaller(true)]
    public class UpdateServiceInstaller : WinServiceInstaller
    {
        protected override IPmServiceMetadata GetServiceMetadata()
        {
            return new PmMssqlExporterServiceMetadata();
        }
    }
}
