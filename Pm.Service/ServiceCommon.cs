using Pm.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Service
{
    public class ServiceCommon : ServiceBase
    {
        /// <summary>
        /// Журнал
        /// </summary>
        private readonly IPmLogger _log;

        /// <summary>
        /// Реализация функционала службы
        /// </summary>
        private readonly IPmServiceInstance _serviceInstance;

        /// <summary>
        /// Вспомогательный класс для работы службы в консольном режиме
        /// </summary>
        private readonly ConsoleHelper _consoleHelper;


        public ServiceCommon(IPmServiceInstance serviceInstance, IPmLogger log)
        {
            InitializeComponent();
            this.ServiceName = serviceInstance.ServiceMetadata.ServiceName;
            _serviceInstance = serviceInstance;
            _log = log;
            _consoleHelper = new ConsoleHelper(serviceInstance, log);
            AutoLog = false;
        }

        /// <summary>
        /// Входная точка. MAIN.
        /// Вызывается из Program;
        /// </summary>
        /// <param name="args"></param>
        public void RunMain(string[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                {
                    // без аргументов, значит приложение запускают как службу
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        this
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                else
                {
                    // аргументы есть - значит консольное приложение
                    switch (args[0])
                    {
                        case "console":
                            _consoleHelper.RunConsole(args);
                            break;
                        default:
                            _consoleHelper.RunConsoleAdmin(args[0]);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        protected override void OnStart(string[] args)
        {
            try
            {
                EventLog.WriteEntry($"OnStart", EventLogEntryType.Information);

                EventLog.WriteEntry($"Запуск службы {_serviceInstance.ServiceMetadata.ServiceName}", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }

            _serviceInstance.Start(args);
            var dtStart = DateTime.Now;
            var dtWait = DateTime.Now - dtStart;
            while (!_serviceInstance.IsWorking && dtWait.TotalSeconds < 120)
            {
                Thread.Sleep(300);
                dtWait = DateTime.Now - dtStart;
            }

            if (dtWait.TotalSeconds >= 120)
            {
                EventLog.WriteEntry($"Долгая попытка старта - {dtWait.TotalSeconds} сек", EventLogEntryType.Error);
            }

            EventLog.WriteEntry($"Служба {_serviceInstance.ServiceMetadata.ServiceName} запущена", EventLogEntryType.Information);

        }

        protected override void OnStop()
        {
            try
            {
                EventLog.WriteEntry($"OnStop", EventLogEntryType.Information);

                EventLog.WriteEntry($"Остановка службы {_serviceInstance.ServiceMetadata.ServiceName}", EventLogEntryType.Information);

                _serviceInstance?.Stop();

                EventLog.WriteEntry($"Служба {_serviceInstance.ServiceMetadata.ServiceName} остановлена", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }


        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        public void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.ServiceName = "Service1";
        }

        #endregion
    }
}
