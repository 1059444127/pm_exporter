using Pm.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Service
{
    /// <summary>
    /// Helper for use executable service file for start/stop/restart windows service
    /// </summary>
    public class ServiceHelper
    {
        private readonly IPmLogger _log;

        private readonly IPmServiceMetadata _metadata;

        public ServiceHelper(IPmServiceMetadata metadata, IPmLogger log)
        {
            _log = log;
            _metadata = metadata;
        }

        public void RunConsoleAdmin(string param)
        {
            Console.WriteLine("".PadLeft(50, '-'));
            Console.WriteLine("The commands for a windows service admin...");

            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Command is cann't be empty");
                Console.WriteLine("".PadLeft(50, '-'));
                return;
            }

            // если мы не администраторы
            if (!IsAdminRole())
            {
                Console.WriteLine("This action have to executing on Administrato level");// ????? refraze
                Console.WriteLine("Запрос прав администратора...");
                // перезапускаем сами себя передавая параметр
                // то есть после запуска мы должны попатьс в тот же метод
                // но уже админом
                Elevate(new[] { param });
                Console.WriteLine("".PadLeft(50, '-'));
                return;
            }

            try
            {
                switch (param.ToLower())
                {
                    case "start":
                        StartService(10000);
                        break;
                    case "restart":
                        RestartService(10000);
                        break;
                    case "stop":
                        StopService(10000);
                        break;
                    case "status":
                        GetStatus();
                        break;
                    case "install":
                        InstallService();
                        break;
                    case "uninstall":
                        UnistallService();
                        break;
                    default:
                        Console.WriteLine("invaild command");
                        break;
                }
            }
            catch (InvalidOperationException ex1)
            {
                Console.WriteLine(ex1.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press Enter key for exit");
                Console.ReadLine();
            }
        }



        #region start, stop, restart, status
        private string StartService(int timeoutMilliseconds)
        {
            Console.WriteLine($"Starting service {_metadata.ServiceName}..");

            var service = new ServiceController(_metadata.ServiceName);
            //try
            //{
            var timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, timeout);

            Console.WriteLine(GetStatusStr());
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            return GetStatusStr();
        }

        private string StopService(int timeoutMilliseconds)
        {
            Console.WriteLine($"Stopping service {_metadata.ServiceName}..");

            var service = new ServiceController(_metadata.ServiceName);
            // try
            // {
            if (service.Status == ServiceControllerStatus.Running)
            {
                var timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                Console.WriteLine(GetStatusStr());
            }
            else
            {
                Console.WriteLine($"Service {_metadata.ServiceName} stopped- '{service.Status}'");
            }
            // }
            // catch (Exception ex)
            // {
            //     _log.LogError(ex);
            // }
            return GetStatusStr();
        }

        private string RestartService(int timeoutMilliseconds)
        {
            Console.WriteLine($"Restarting service {_metadata.ServiceName}..");

            var service = new ServiceController(_metadata.ServiceName);
            // try
            // {
            Console.WriteLine(GetStatusStr());

            int millisec1 = 0;
            TimeSpan timeout;
            if (service.Status == ServiceControllerStatus.Running)
            {
                Console.WriteLine($"Stopping...");
                millisec1 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            Console.WriteLine($"Starting...");
            // count the rest of the timeout
            int millisec2 = Environment.TickCount;
            timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            _log.LogInfo(GetStatusStr());
            // }
            // catch (Exception ex)
            // {
            //     _log.LogError(ex);
            // }
            return GetStatusStr();
        }

        private ServiceControllerStatus GetStatus()
        {
            Console.WriteLine($"Getting service {_metadata.ServiceName} status...");
            var service = new ServiceController(_metadata.ServiceName);
            Console.WriteLine($"Статус службы {_metadata.ServiceName} - '{service.Status}'");
            return service.Status;
        }

        private string GetStatusStr()
        {
            var result = "";
            try
            {
                var service = new ServiceController(_metadata.ServiceName);
                result = service.Status.ToString();
            }
            catch (Exception ex)
            {
                _log.LogError(ex);
                result = ex.Message;
            }
            return result;
        }
        #endregion

        #region Install, Uninstall

        private void InstallService()
        {
            var proc = Process.GetCurrentProcess();

            Console.WriteLine($"Устанавливаем службу {_metadata.ServiceName}..");
            Console.WriteLine($" - размещение {proc.MainModule.FileName}..");

            ExecInstallUtil(new[] { proc.MainModule.FileName });
        }

        private void UnistallService()
        {
            var proc = Process.GetCurrentProcess();

            Console.WriteLine($"Удаляем службу {_metadata.ServiceName}..");
            Console.WriteLine($" - размещение {proc.MainModule.FileName}..");

            ExecInstallUtil(new[] { proc.MainModule.FileName, "/u" });
        }

        /// <summary>
        /// Метод находит подходящую на данном компе версию InstallUtil 
        /// и вызывает её с укзанными аргументами
        /// </summary>
        /// <param name="args"></param>
        private void ExecInstallUtil(string[] args)
        {
            try
            {
                Console.WriteLine("".PadLeft(50, '-'));
                Console.WriteLine("Начало работы Mcs.InstallUtil");
                Console.WriteLine("Поиск подходящего InstallUtil....");

                var seachFrameworkDir = Environment.GetEnvironmentVariable("windir");

                if (Environment.Is64BitOperatingSystem)
                    seachFrameworkDir = seachFrameworkDir + "\\Microsoft.NET\\Framework";
                else
                    seachFrameworkDir = seachFrameworkDir + "\\Microsoft.NET\\Framework";

                string[] actualDotNetDirArr = Directory.GetDirectories(
                       seachFrameworkDir
                     , "v4*"
                     , SearchOption.AllDirectories);
                AppDomain ad = AppDomain.CurrentDomain;
                string actualDotNetDir = "";
                string actualInstallUtil = "";
                string execParams = "";
                bool isDone = false;
                foreach (var dir in actualDotNetDirArr)
                {
                    actualDotNetDir = dir;
                    if (args.Contains("/debug"))
                        Console.WriteLine(" ->" + dir);
                    if (File.Exists(actualDotNetDir + "\\InstallUtil.exe"))
                    {
                        isDone = true;
                        actualInstallUtil = actualDotNetDir + "\\InstallUtil.exe";
                        Console.WriteLine("Найден файл " + actualInstallUtil);
                        break;
                    }
                }

                if (!isDone)
                {
                    Console.WriteLine("Файл InstallUtil.exe не найден");
                    Console.ReadLine();
                    return;
                }

                foreach (var p in args)
                {
                    if (p != "/debug")
                        execParams += p + " ";
                }

                var pi = new ProcessStartInfo();
                pi.FileName = actualInstallUtil;
                pi.Arguments = execParams;
                pi.UseShellExecute = false;
                pi.RedirectStandardOutput = true;
                pi.CreateNoWindow = true;
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    pi.Verb = "runas";
                }

                Console.WriteLine("Выполняем '" + actualInstallUtil + " " + execParams + "'");
                var proc = Process.Start(pi);
                using (System.IO.StreamWriter file
                    = new System.IO.StreamWriter(@"Mcs.InstallUtil.log", true))
                {
                    file.WriteLine("");
                    file.WriteLine("=================================================================================");
                    file.WriteLine("run '" + actualInstallUtil + " " + execParams + "'");
                    file.WriteLine(DateTime.Now);
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        file.WriteLine(line);
                        Console.WriteLine(line);
                    }
                    file.WriteLine("");
                    file.WriteLine("=================================================================================");
                }
                Console.WriteLine("".PadLeft(50, '-'));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Работа программы Mcs.InstallUtil завершена");
        }
        #endregion


        /// <summary>
        /// Запуск этой же программы но с запросом администратора
        /// и передачей ей тех-же аргументов
        /// </summary>
        /// <param name="args"></param>
        private static void Elevate(string[] args)
        {
            var proc = Process.GetCurrentProcess();

            var argsStr = args.Aggregate(string.Empty, (current, s) => current + (s + " "));

            var newStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = proc.MainModule.FileName,//Application.ExecutablePath,
                Verb = "runas",
                Arguments = argsStr
            };

            try
            {
                var newProcess = new Process();

                newProcess.StartInfo = newStartInfo;

                newProcess.Start();

                newProcess.WaitForExit();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to elevate!");
                Console.WriteLine(ex.Message);
            }
        }

        private bool IsAdminRole()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
