using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.MsSql.Collector
{
    public class MsSqlGaugeConnections : MsSqlCollector
    {

        public MsSqlGaugeConnections() : base("gauge", "mssql_connections", "Connection list")
        {
        }

        public override string GetSqlQuery() => "select db_name(dbid) as [db], loginame as [login], count(*) as [val] from master.dbo.sysprocesses where Spid <> @@spid group by dbid, loginame order by dbid, loginame";


        protected override void CollectFromReader(SqlDataReader reader, ref double counter, ref StringBuilder sb, CancellationToken cToken)
        {
            if (reader.FieldCount != 3)
                return;
            while (reader.Read())
            {

                var paramLoginTitle = reader.GetName(0)?.ToString();
                var paramLoginValue = reader.GetString(0)?.Trim('\r', '\n', '\t', ' ').Replace("\\", "\\\\");

                var paramDbTitle = reader.GetName(1)?.ToString();
                var paramDbValue = reader.GetString(1)?.Trim('\r', '\n', '\t', ' ').Replace("\\", "\\\\");

                var paramValue = int.Parse(reader[2]?.ToString() ?? "0");

                sb.Append($"{Name} {{{paramDbTitle}=\"{paramDbValue}\", {paramLoginTitle}=\"{paramLoginValue}\"}} {paramValue}\n");

                counter += paramValue;
            }
        }
    }
}
