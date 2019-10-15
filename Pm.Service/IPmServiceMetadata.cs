using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Service
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPmServiceMetadata
    {
        /// <summary>
        /// Short service name - for windows, for example 'MyPrometheusExporter'
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Short service name which will be in windows service list, for example ''
        /// </summary>
        string ServiceDisplayName { get; }

        /// <summary>
        /// Detail info for windows service list
        /// </summary>
        string ServiceDescription { get; }

        /// <summary>
        /// 
        /// </summary>
        ServiceAccount Account { get; }

        string AppName { get; }
    }
}
