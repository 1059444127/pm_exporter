using log4net;
using log4net.Config;
using Pm.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Service
{
    /// <summary>
    /// Class which to catch IPmLogger's log message and redirect them to Log4net with formatting
    /// </summary>
    public class Log4NetLogger: IPmLogWriter
    {
        private readonly Dictionary<string, ILog> _dict = new Dictionary<string, ILog>();

        protected ILog GetLoggerByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                key = "NONE";

            if (_dict.ContainsKey(key))
                return _dict[key];

            var newLogger = LogManager.GetLogger(key);
            _dict.Add(key, newLogger);
            return newLogger;
        }

        /// <summary></summary>
        public virtual void CatchLogMessage(PmLogMsgItem item)
        {
            WriteMessage(item);
        }

        StringBuilder _sb = new StringBuilder(2000);

        protected virtual void WriteMessage(PmLogMsgItem item)
        {

            McsLogMessageItemToLog4NetMessage(item, ref _sb);

            var logger = GetLoggerByKey(item.LoggerKey);

            switch (item.LogLevel)
            {
                case LogLevel.Debug:
                    logger.Debug(_sb.ToString());
                    break;
                case LogLevel.Info:
                    logger.Info(_sb.ToString());
                    break;
                case LogLevel.Warn:
                    logger.Warn(_sb.ToString());
                    break;
                case LogLevel.Error:
                    logger.Error(_sb.ToString(), item.Ex);
                    break;
                case LogLevel.Fatal:
                    logger.Fatal(_sb.ToString(), item.Ex);
                    break;
            }
        }

        protected static LogLevel[] ErrorLevels = new[] { LogLevel.Error, LogLevel.Fatal };

        protected int MaxLoggerKeyForIdent = 0;

        protected string PSpace = "";

        /// <summary>
        /// convert PmLogMsgItem to Log4Net format
        /// </summary>
        /// <param name="item"></param>
        /// <param name="sb"></param>
        /// <param name="key"></param>
        protected virtual void McsLogMessageItemToLog4NetMessage(PmLogMsgItem item, ref StringBuilder sb)
        {
            sb.Clear();

            if (item.LoggerKey.Length > MaxLoggerKeyForIdent) MaxLoggerKeyForIdent = item.LoggerKey.Length;

            var currPad = (MaxLoggerKeyForIdent > item.LoggerKey.Length) ? MaxLoggerKeyForIdent - item.LoggerKey.Length : 0;

            _sb.Append($"{PSpace.PadLeft(currPad)}{item.LogMessage} ");

            if (!string.IsNullOrWhiteSpace(item.LogObjType) || !string.IsNullOrWhiteSpace(item.LogObjKey))
                _sb.Append($"\n{item.LogObjType}/{item.LogObjKey}");

            if (item.LogObject != null) _sb.Append($"\n{item.LogObject.ToString()}");
        }


        /// <summary>Initiating of logger. Need call when app starting </summary>
        public virtual void InitWriter()
        {
            XmlConfigurator.Configure();
            var logger = GetLoggerByKey("LOGGER");
            logger.Info("********************************************************************************");
            logger.Info("SERVICE STARTING");
        }

        /// <summary></summary>
        public virtual void CloseWriter()
        {
            var logger = GetLoggerByKey("LOGGER");
            logger.Info("SERVICE FINISHED");
            logger.Info("********************************************************************************");
        }

    }
}
