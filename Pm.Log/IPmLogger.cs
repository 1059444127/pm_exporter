using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Log
{
    public interface IPmLogger
    {
        bool IsLogEnabled { get; set; }
        LogLevel[] LogLevelFilter { get; set; }

        string LoggerKey { get; set; }

        void LogDebug(string logMessage, string logProc = null, int logActionId = 0);
        void LogInfo(string logMessage, string logProc = null, int logActionId = 0);
        void LogWarn(string logMessage, string logProc = null, int logActionId = 0);

        void LogForObj(string logMessage, string logObjType, string logObjKey, object logObject, string logProc = null, int logActionId = 0);
        void LogForObj(LogLevel level, string logMessage, string logObjType, string logObjKey, object logObject, string logProc = null, int logActionId = 0);

        void LogError(string logMessage, string logProc = null, int logActionId = 0);
        void LogError(Exception ex, string logProc = null, int logActionId = 0);

        void LogFatal(string logMessage, string logProc = null, int logActionId = 0);
        void LogFatal(Exception ex, string logProc = null, int logActionId = 0);
    }
}
