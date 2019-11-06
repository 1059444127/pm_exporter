using Pm.Common;
using Pm.Log;
using Pm.MsSql;
using Pm.MsSql.Collector;
using Pm.Service;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace pm_mssql_exporter
{
    public class PmMssqlExporterServiceInstance : PmLongTask, IPmServiceInstance
    {
        private PmMetricServer _pmMetricServer;
        private PmMetricPusher _pmMetricPusher;
        private CollectorRegistry _collectorRegistry;

        private IPmLogger _logger;

        public PmMssqlExporterServiceInstance(int intervalSeconds = 60) : base(intervalSeconds)
        {
        }

        public IPmServiceMetadata ServiceMetadata => new PmMssqlExporterServiceMetadata();

        public bool IsWorking { get; set; }

        public bool HandleCommand(string cmd)
        {
            if (string.Equals(cmd, "q", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Instance. Stop user command...");
            }

            if (string.Equals(cmd, "test", StringComparison.OrdinalIgnoreCase))
            {
                //var client = new VtsLoginClient();
                //var str = client.GetBar();
                //Task.Run(async () =>
                //{
                //    var str2 = await client.GetFooAsync(15);

                //});

            }

            return true;
        }

        private bool _isSqlConnectionOk = false;
        private bool _isInitiated = false;
        private bool _isErrorWhenCollect = false;


        public void Init(IPmLogger logger)
        {
            _logger = logger;
        }

        public void Start(string[] args)
        {
            _logger.LogInfo("Starting...");

            IsWorking = true;

            _isSqlConnectionOk = IsSqlConnectionOk();
            _isInitiated = false;

            base.Start();

            _logger.LogInfo("Service started");
        }

        public override void Stop()
        {
            _pmMetricPusher?.Stop();
            _pmMetricServer?.Stop();

            base.Stop();
        }

        FastLocker _workStepLocker = new FastLocker();

        protected virtual async Task WorkStep(CancellationToken ct)
        {
            // на шаге "основного" цикла необходимо проверять
            // наличие подключения к SQL серверу
            // в случае если его нет, или была SQL ошибка при collect метрик
            // нужно отключать коллектор и проверять подключение до тех пор
            // пока оно не заработает


            // если есть ошибка при сборе метрик
            // то это ошибка подключения, нужно переподключаться
            if (_isErrorWhenCollect)
            {
                _isErrorWhenCollect = false;
                _isSqlConnectionOk = false;
            }

            if (_isSqlConnectionOk)
            {
                // если подключение к SQL серверу работает

                // если сервер/пушер не инициализированны
                if (!_isInitiated)
                {
                    // остановим
                    _pmMetricPusher?.Stop();
                    _pmMetricServer?.Stop();

                    // инициализируем
                    InitServerAndPusher();

                    // запустим
                    _pmMetricPusher?.Start();
                    _pmMetricServer?.Start();
                }
            }
            else
            {

                // остановим
                _pmMetricPusher?.Stop();
                _pmMetricServer?.Stop();

                // если с подключением к SQL проблема
                // проверим подключение еще раз
                _isSqlConnectionOk = IsSqlConnectionOk();

                // сбросим марке ошибки
                _isErrorWhenCollect = false;

                _isInitiated = false;

                // а на следующем тике если ОК попадем в блок выше
            }

        }

        private void InitServerAndPusher()
        {
            var sett = new Properties.Settings();
            sett.Reload();

            _collectorRegistry?.Stop();
            _collectorRegistry?.Clear();

            _collectorRegistry = new CollectorRegistry();

            // если указали auto - то нужно определить имя инстанса из ConnStr
            // если нет - то берем то что указали
            var instanceName = GetInstanceName(sett.InstanceName, PmMssqlSett.ConnectionString);

            try
            {
                if (sett.IsEnabledPusher)
                {
                    _logger.LogInfo($"Init metric pusher:");
                    _logger.LogInfo($"  - PushGatewayUri : {sett.PushGatewayUri}");
                    _logger.LogInfo($"  - JobName        : {sett.JobName}");
                    _logger.LogInfo($"  - InstanceName   : {instanceName}");
                    _logger.LogInfo($"  - PushIntervalSec: {sett.PushIntervalSec}");

                    if (_pmMetricPusher != null) _pmMetricPusher.OnErrorWhenCollect -= ErrorWhenCollectEventHandler;

                    _pmMetricPusher = new PmMetricPusher(_collectorRegistry, sett.PushGatewayUri, sett.JobName, instanceName, sett.PushIntervalSec);

                    _pmMetricPusher.OnErrorWhenCollect += ErrorWhenCollectEventHandler;
                    _pmMetricPusher.IsAllOkFunc = () => _isSqlConnectionOk && _isInitiated;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка инициализации MetricPusher");
                _logger.LogError(ex);
                _pmMetricPusher = null;
            }



            try
            {
                if (sett.IsEnabledServer)
                {
                    _logger.LogInfo($"Init metric server:");
                    _logger.LogInfo($"  - ServerUri : {sett.ServerUri}");
                    _pmMetricServer = new PmMetricServer(sett.ServerUri);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка инициализации MetricServer");
                _logger.LogError(ex);
                _pmMetricServer = null;
            }


            if (_pmMetricServer == null && _pmMetricPusher == null)
                throw new Exception("Server and pusher is not initialized. Work cannot be continued");


            if (sett.IsEnabledConnectionInfo)
            {
                _logger.LogInfo($"Init connection info collector...");
                var gaugeConnInfo = new MsSqlGaugeConnections();
                _collectorRegistry.GetOrAdd(gaugeConnInfo);
            }

            if (sett.IsEnableOsPerfCounters)
            {
                _logger.LogInfo($"Init OsPerfomanceCounter collectors...");
                var perfCounterSett = GetOsPerfCounterSett(sett.OsPerformanceCounters);
                var gaugePerfCounter = new MsSqlDmOsPerformanceCounters(perfCounterSett);
                _collectorRegistry.GetOrAdd(gaugePerfCounter);
                foreach (var s in perfCounterSett)
                    _logger.LogInfo($"  - {s.Name}");
            }

            if (sett.IsEnableCustomMetrics && sett.CustomMetrics != null)
            {
                _logger.LogInfo($"Init custom collectors...");
                foreach (var m in sett.CustomMetrics.Where(c => c.IsEnabled))
                {
                    var customCollector = new MsSqlCustomMetric(m.Type, m.Name, m.Help, m.Sql);
                    _collectorRegistry.GetOrAdd(customCollector);
                    _logger.LogInfo($"  - {m.Name}");
                }
            }

            _isInitiated = true;
        }

        private void ErrorWhenCollectEventHandler(object sender, EventArgs e)
        {
            _isErrorWhenCollect = true;
        }

        private string GetInstanceName(string instanceName, string connStr)
        {
            if (string.Equals(instanceName, "auto", StringComparison.OrdinalIgnoreCase))
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    return conn.DataSource;
                }
            }
            else
            {
                return instanceName;
            }
        }

        private bool IsSqlConnectionOk()
        {
            try
            {
                var sett = new Properties.Settings();
                sett.Reload();

                using (var conn = new SqlConnection(sett.ConnStr))
                {
                    conn.Open();

                    PmMssqlSett.ConnectionString = sett.ConnStr;
                }
                return true;
            }
            catch (Exception ex)
            {
                PmMssqlSett.ConnectionString = "";
                return false;
            }
        }
        private MsSqlDmOsPerformanceCounters.SettItem[] GetOsPerfCounterSett(StringCollection osPerformanceCounters)
        {
            var result = new MsSqlDmOsPerformanceCounters.SettItem[osPerformanceCounters.Count];

            var i = 0;

            foreach (var s in osPerformanceCounters)
            {
                // gauge; mssql_cache_hit_ratio; Cache Hit Ratio; _Total; Plan Cache
                var arr = s.Split(';');

                var type = "";
                var name = "";

                var object_name_like = "";
                var counter_name = "";
                var instance_name = "";

                if (arr.Length > 0) type = arr[0];
                if (arr.Length > 1) name = arr[1];
                if (arr.Length > 2) counter_name = arr[2];
                if (arr.Length > 3) instance_name = arr[3];
                if (arr.Length > 4) object_name_like = arr[4];

                result[i] = new MsSqlDmOsPerformanceCounters.SettItem(type, name, counter_name, instance_name, object_name_like);

                i++;
            }

            return result;
        }

        private void LoadSett()
        {

        }
    }
}
