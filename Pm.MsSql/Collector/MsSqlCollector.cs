using Pm.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.MsSql.Collector
{
    public abstract class MsSqlCollector : ICollector
    {
        public string Type { get; }
        public string Name { get; }

        public string Help { get; }

        public virtual string GetSqlQuery() => "";

        protected virtual void CollectFromReader(SqlDataReader reader, ref double counter, ref StringBuilder sb, CancellationToken cToken)
        {
            if (reader.FieldCount != 2)
                return;
            while (reader.Read())
            {
                
                var paramTitle = reader.GetName(0)?.ToString();
                var paramName = reader.GetString(0)?.Trim('\r', '\n', '\t', ' ').Replace("\\", "\\\\");
                var paramValue = int.Parse(reader[1]?.ToString() ?? "0");

                sb.Append($"{Name} {{{paramTitle}=\"{paramName}\"}} {paramValue}\n");
                counter += paramValue;
            }
        }

        public virtual void CollectAndSerialize(ref StringBuilder sb, CancellationToken cToken)
        {
            using (var conn = new SqlConnection(PmMssqlSett.ConnectionString))
            {
                conn.Open();
                var source = conn.DataSource;
                var sql = GetSqlQuery();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    var reader = cmd.ExecuteReader();

                    sb.Append($"# HELP {Name} {Help}\n");
                    sb.Append($"# TYPE {Name} {Type}\n");
                    var cnAll = 0.0;

                    CollectFromReader(reader, ref cnAll, ref sb, cToken);

                    if(cnAll > -1)
                        sb.Append($"{Name} {cnAll}\n");
                }
            }
        }

        public MsSqlCollector(string type, string name, string help)
        {
            Type = type;
            Name = name;
            Help = help;
        }
    }
}
