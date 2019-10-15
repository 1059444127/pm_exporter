using Pm.Log;
using Pm.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pm_mssql_exporter
{
    class Program
    {
        private static IPmLogWriter _logWriter;

        private static IPmLogger _logHandler;

//        System.Collections.Generic.List<Pm.MsSql.CustomMetricSett>

        static void Main(string[] args)
        {
            try
            {
                _logWriter = new Log4NetLogger();

                // перенаправляем все события LogHandler-ов в класс который будет писать логи
                PmLogger.StaticCatchLogFunc = _logWriter.CatchLogMessage;

                _logWriter.InitWriter();

                // журнал
                _logHandler = new PmLogger("Instance");

                // класс в котором реализован необходимый функционал
                IPmServiceInstance serviceInstance = new PmMssqlExporterServiceInstance();

                // инициализация основной службы
                serviceInstance.Init(_logHandler);


                // служба McsWinServiceBase->ServiceBase
                // которая может работать как служба и как консольное приложение
                var serviceProgram = new PmMssqlExporterService(serviceInstance, _logHandler);

                // запуск с передачей аргументов
                serviceProgram.RunMain(args);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // отметка о завершении работы
            _logWriter?.CloseWriter();
        }
    }
}
