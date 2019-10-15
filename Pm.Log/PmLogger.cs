using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Log
{
    public delegate void LogMsgDelegate(PmLogMsgItem item);
    public class PmLogger: IPmLogger
    {
        public bool IsLogEnabled { get; set; } = true;

        public LogLevel[] LogLevelFilter { get; set; } = new[] { LogLevel.All };

        public static LogMsgDelegate StaticCatchLogFunc;


        public LogMsgDelegate CatchLogFunc { get; set; } = (item) => StaticCatchLogFunc?.Invoke(item);

        /// <summary>
        /// NetbiosMachineName\UserName
        /// </summary>
        public string CurrentUserName =
            $"{Environment.MachineName}/{(!string.IsNullOrWhiteSpace(Environment.UserDomainName) ? Environment.UserDomainName : Environment.UserName)}";


        public PmLogger(string loggerKey)
        {
            _loggerKey = loggerKey;
        }


        protected void LogBase(
              DateTime logDateTime
            , LogLevel logLevel
            , string logMessage
            , string logStackTrace
            , string logObjType
            , string logObjKey
            , object logObject
            , Exception ex
            , string logProc
            , int logActionId)
        {
            if (!IsLogEnabled) return;

            if (LogLevelFilter.Contains(LogLevel.All) || LogLevelFilter.Contains(logLevel))
            {
                CatchLogFunc?.Invoke(new PmLogMsgItem()
                {
                    LogDatetime = logDateTime
                    ,
                    LogLevel = logLevel
                    ,
                    LogUser = CurrentUserName
                    ,
                    LogProc = logProc
                    ,
                    LogMessage = logMessage
                    ,
                    LogStackTrace = logStackTrace
                    ,
                    LogObjType = logObjType
                    ,
                    LogObjKey = logObjKey
                    ,
                    LogObject = logObject
                    ,
                    Ex = ex
                    ,
                    LoggerKey = LoggerKey
                    ,
                    LogActionId = logActionId
                });
            }
        }

        ////////////////////////////////////////////////

        private string _loggerKey;
        public string LoggerKey
        {
            get => _loggerKey;
            set => _loggerKey = value;
        }


        public void LogDebug(string logMessage, [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, LogLevel.Debug, logMessage, null, null, null, null, null, logProc, logActionId);
        }

        public void LogInfo(string logMessage, [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, LogLevel.Info, logMessage, null, null, null, null, null, logProc, logActionId);
        }

        public void LogWarn(string logMessage, [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, LogLevel.Warn, logMessage, null, null, null, null, null, logProc, logActionId);
        }

        public void LogForObj(string logMessage, string logObjType, string logObjKey, object logObject
            , [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, LogLevel.Debug, logMessage, null, logObjType, logObjKey, logObject, null, logProc, logActionId);
        }

        public void LogForObj(LogLevel level, string logMessage, string logObjType, string logObjKey, object logObject,
            [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, level, logMessage, null, logObjType, logObjKey, logObject, null, logProc, logActionId);
        }

        public void LogError(string logMessage, [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, LogLevel.Error, logMessage, null, null, null, null, null, logProc, logActionId);
        }

        public void LogError(Exception ex, [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, LogLevel.Error, ex.Message, ex.StackTrace, null, null, null, ex, logProc, logActionId);


            if (ex.InnerException != null)
            {
                LogBase(DateTime.Now, LogLevel.Error, ex.InnerException.Message, ex.InnerException.StackTrace, null, null, null, ex.InnerException, logProc + ". InnerException", logActionId);
            }
            // TODO: обработка спец ошибок, типа сетевых, EF - которые
            // содержат доп классы с детальной информацией
        }

        public void LogFatal(string logMessage, [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, LogLevel.Fatal, logMessage, null, null, null, null, null, logProc, logActionId);
        }

        public void LogFatal(Exception ex, [System.Runtime.CompilerServices.CallerMemberName] string logProc = null, int logActionId = 0)
        {
            GetMethodNameFromFrame((new System.Diagnostics.StackTrace(1))?.GetFrame(0), ref logProc);

            LogBase(DateTime.Now, LogLevel.Fatal, ex.Message, ex.StackTrace, null, null, null, ex, logProc, logActionId);


            if (ex.InnerException != null)
            {
                LogBase(DateTime.Now, LogLevel.Error, ex.InnerException.Message, ex.InnerException.StackTrace, null, null, null, ex, logProc + ". InnerException", logActionId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetMethodNameFromFrame(System.Diagnostics.StackFrame frame, ref string logProc)
        {
            if (!string.IsNullOrWhiteSpace(logProc))
                return;

            if (frame == null)
                return;

            var method = frame.GetMethod();

            if (method == null) return;

            var name = method.Name;

            if (string.Equals(name, "MoveNext"))
            {
                var fullAsyncName = method.DeclaringType?.Name;
                logProc = GetMethodNameFromAsync(ref fullAsyncName);
            }
            else if (method.Name.StartsWith("<"))
            {
                logProc = GetMethodNameFromAsync(ref name);
            }
            else
            {
                logProc = method.Name;
            }

            if (string.IsNullOrWhiteSpace(logProc))
            {

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetMethodNameFromAsync(ref string asyncDeclaringTypeName)
        {
            if (string.IsNullOrWhiteSpace(asyncDeclaringTypeName))
                return null;
            var len = asyncDeclaringTypeName.Length;
            var idx1 = asyncDeclaringTypeName.IndexOf("<");
            var idx2 = asyncDeclaringTypeName.IndexOf(">", idx1);

            if (idx1 < idx2 && idx1 >= 0 && idx2 > 0 && idx1 < len && idx2 < len)
            {
                var s = asyncDeclaringTypeName.Substring(idx1 + 1, idx2 - idx1 - 1);

                return s;
            }
            else
            {
                return null;
            }
        }
    }
}
