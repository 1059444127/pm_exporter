using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.MsSql.Collector
{
    public class MsSqlDmOsPerformanceCounters : MsSqlCollector
    {
        SettItem[] _settArr;

        string _sql = "";
        public class SettItem
        {
            public string Type;
            public string Name;
            public string Help;
            
            public string Instance;
            public string InstObject;

            public SettItem(string type, string name, string help, string instance, string instObject)
            {
                Type = type?.Trim();
                Name = name?.Trim();
                Help = help?.Trim();
                Instance = instance?.Trim();
                InstObject = instObject?.Trim();
            }
        }
        public MsSqlDmOsPerformanceCounters(SettItem[] paramArr) : base("", "", "")
        {
            _settArr = paramArr;

            _sql = "select counter_name, cntr_value from sys.dm_os_performance_counters ";


            var groupByInstance = _settArr
                .GroupBy(c => new { c.Instance, c.InstObject })
                .Select(c => c.Key);

            var k = "\r\nwhere";

            foreach (var inst in groupByInstance)
            {

                var x = string.IsNullOrWhiteSpace(inst.InstObject) ? "" :$"and object_name like '%{inst.InstObject}%'";
                _sql += $"{k}( instance_name = '{inst.Instance}' {x} and counter_name in (";

                var m = "  ";
                foreach (var item in _settArr.Where(c => c.Instance == inst.Instance &&  c.InstObject == inst.InstObject ))
                {
                    _sql += $"\r\n    {m}'{item.Help}'";
                    m = ", ";
                }

                _sql += $"))";

                k = "\r\nor";
            }

            _sql += "\r\norder by counter_name";

        }

        public override string GetSqlQuery() => _sql;

        public override void CollectAndSerialize(ref StringBuilder sb, CancellationToken cToken)
        {
            using (var conn = new SqlConnection(PmMssqlSett.ConnectionString))
            {
                conn.Open();
                var source = conn.DataSource;
                var sql = GetSqlQuery();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var counter = reader["counter_name"]?.ToString()?.Trim();
                        var value = reader["cntr_value"]?.ToString()?.Trim();
                        var item = _settArr.FirstOrDefault(c => string.Equals(c.Help, counter));
                        var s = string.IsNullOrWhiteSpace(item.InstObject)?"":$"({item.InstObject})";
                        sb.Append($"# HELP {item.Name} {item.Help} {s}\n");
                        sb.Append($"# TYPE {item.Name} {item.Type}\n");
                        sb.Append($"{item.Name} {value}\n");
                    }
                }
            }
        }

        protected override void CollectFromReader(SqlDataReader reader, ref double counter, ref StringBuilder sb, CancellationToken cToken)
        {
            if (reader.FieldCount != 3)
                return;
            while (reader.Read())
            {
                var paramValue = int.Parse(reader[0]?.ToString() ?? "0");

                sb.Append($"{Name} {paramValue}\n");

                counter += paramValue;
            }
        }
    }

}
