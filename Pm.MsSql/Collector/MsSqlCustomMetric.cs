using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.MsSql.Collector
{
    public class MsSqlCustomMetric : MsSqlCollector
    {
        private string _sql;

        public MsSqlCustomMetric(string type, string metric_name, string help, string sql) : base(type, metric_name, help)
        {
            _sql = sql;
        }

        public override string GetSqlQuery() => _sql;


        protected override void CollectFromReader(SqlDataReader reader, ref double counter, ref StringBuilder sb, CancellationToken cToken)
        {
            // подразумевается что последнее поле - это значение Gauge
            // если 1 поле - то только значение метрики
            // если 2 и более - то первые поля - это параметры, последнее - значение

            var fieldCount = 0;
            string[] fieldNames = null;


            while (reader.Read())
            {
                // за первый проход получим количество столбцов и их названия
                if(fieldNames == null)
                {
                    fieldCount = reader.FieldCount;
                    fieldNames = new string[fieldCount];
                    for (var i = 0; i < fieldCount; i++)
                        fieldNames[i] = reader.GetName(i);
                }

                // значение метрики
                var valueStr = reader[fieldCount - 1]?.ToString() ?? "0";

                

                // если полей более одного - значит есть параметры
                if(fieldCount > 1)
                {
                    sb.Append($"{Name} ");

                    sb.Append($"{{");
                    
                    for(var i = 0; i < fieldCount-1; i++)
                    {
                        var paramStr = reader[i]?.ToString()?.Trim('\r', '\n', '\t', ' ')?.Replace("\\", "\\\\");

                        if (!string.IsNullOrWhiteSpace(paramStr))
                        {
                            if(i > 0) sb.Append($", ");

                            sb.Append($"{fieldNames[i]}=\"{paramStr}\"");
                        }
                    }

                    sb.Append($"}} {valueStr}\n");
                }

                // sb.Append($" {valueStr}\n");

                if(double.TryParse(valueStr, out double d))
                    counter += d;
            }// while
        }
    }
}
