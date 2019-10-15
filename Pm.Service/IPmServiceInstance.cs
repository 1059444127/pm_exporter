
using Pm.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Service
{
    /// <summary>
    /// Interface which must to inplement service instance class, for interaction with console helper
    /// </summary>
    public interface IPmServiceInstance
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        void Init(IPmLogger logger);

        /// <summary>
        /// description ofr servie
        /// </summary>
        IPmServiceMetadata ServiceMetadata { get; }

        /// <summary>
        /// Working marker
        /// </summary>
        bool IsWorking { get; set; }

        /// <summary>
        /// 
        /// </summary>
        void Start(string[] args);

        /// <summary>
        /// 
        /// </summary>
        void Stop();

        /// <summary>
        /// Console command handler
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        bool HandleCommand(string cmd);
    }
}
