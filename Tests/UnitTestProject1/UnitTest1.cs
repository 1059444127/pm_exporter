using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pm.Common;
using Pm.Counter;
using Pm.MsSql;
using Pm.MsSql.Collector;


namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var counter = new Counter("test_counter", "TestCounter");

            var gauge1 = new MsSqlGaugeConnections();

            var settArr = new []{ 
                  new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_table_lock_escalations_per_sec ","Table Lock Escalations/sec  ","","")
                , new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_buffer_cache_hit_ratio         ","Buffer cache hit ratio      ","","")
                , new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_average_latch_wait_time_ms     ","Average Latch Wait Time (ms)","","")
                , new MsSqlDmOsPerformanceCounters.SettItem("counter", "mssql_activation_errors_total        ","Activation Errors Total     ","","")

                , new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_average_wait_time_ms           ","Average Wait Time (ms)      ","_Total","")
                , new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_lock_timeouts_per_sec          ","Lock Timeouts/sec           ","_Total","")
                , new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_lock_wait_time_ms              ","Lock Wait Time (ms)         ","_Total","")
                , new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_number_of_deadlocks_per_sec    ","Number of Deadlocks/sec     ","_Total","")
                , new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_cache_hit_ratio                ","Cache Hit Ratio             ","_Total","Plan Cache ")
                , new MsSqlDmOsPerformanceCounters.SettItem("gauge  ", "mssql_transactions_per_sec           ","Transactions/sec            ","_Total","")
            };

            var gauge2 = new MsSqlDmOsPerformanceCounters(settArr);

            var collectorRegister = new CollectorRegistry();

            collectorRegister.GetOrAdd(counter);
            collectorRegister.GetOrAdd(gauge1);
            collectorRegister.GetOrAdd(gauge2);


            var pushServer = new PmMetricServer("http://localhost:7000/");

            var metricPusher = new PmMetricPusher(collectorRegister, "http://localhost:9091/metrics", "testJob7","srv-1", 5);

            // pushServer.Start("http://localhost:7000/");

            metricPusher.Start();

            var rnd = new Random();
            for (var i = 0; i < 60; i++)
            {                
                counter.Inc(rnd.NextDouble() * 10);
                Thread.Sleep(1000);
            }

            metricPusher.Stop();

            pushServer.Stop();
        }
    }
}
