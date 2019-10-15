using Pm.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Service
{
    /// <summary>
    /// Class helper for run win service as console application
    /// </summary>
    public class ConsoleHelper
    {
        /// <summary> </summary>
        private readonly IPmLogger _log;

        /// <summary> </summary>
        private readonly IPmServiceInstance _serviceInstance;

        /// <summary> </summary>
        private string ServiceName => _serviceInstance.ServiceMetadata.ServiceName;

        /// <summary> </summary>
        private readonly ServiceHelper _winServiceHelper;

        /// <summary>working marker</summary>
        private bool _doWork;



        public ConsoleHelper(IPmServiceInstance serviceInstance, IPmLogger log)
        {
            _log = log;
            _serviceInstance = serviceInstance;
            _winServiceHelper = new ServiceHelper(serviceInstance.ServiceMetadata, log);
        }


        public static void WriteLine()
        {
            Console.WriteLine(@"---------------------------------------------------------------------");
        }

        public void RunConsole(string[] args)
        {
            WriteLine();

            Console.WriteLine($"Running service '{_serviceInstance.ServiceMetadata.AppName}' in console...");
            _doWork = true;

            _serviceInstance.Start(args);

            var st = DateTime.Now;

            // waiting while service have not been running
            while (!_serviceInstance.IsWorking)
            {
                Thread.Sleep(100);
                if ((DateTime.Now - st).TotalSeconds > 20)
                    throw new Exception("Service have not been started for 20 sec");
            }

            var cmd = "";

            WriteLine();
            Console.WriteLine(@"Service running in console, input command:");

            // read command loop
            while (_doWork)
            {
                var cmdStr = Console.ReadLine()?.Trim();

                _doWork = _serviceInstance.HandleCommand(cmdStr);

                Console.WriteLine(@"-------------------------------------------------------------");

                if (!_doWork || string.Equals(cmdStr, "q", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(@"Stopping...");
                    _doWork = false;
                    _serviceInstance.Stop();
                }
            }//while

            // waiting unitl all threads stop
            var dtStartWait = DateTime.Now;
            while (_serviceInstance.IsWorking && (DateTime.Now - dtStartWait).TotalSeconds < 120)
            {
                Thread.Sleep(200);
            }

            Console.WriteLine(@"Service stopped. Press any key for exit.");
            Console.ReadKey();
        }

        public void RunConsoleAdmin(string param)
        {
            _winServiceHelper.RunConsoleAdmin(param);
        }
    }
}
