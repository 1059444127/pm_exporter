using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Common
{
    public class PmLogMsgItem: ICloneable
    {
        public DateTime LogDatetime;

        public LogLevel LogLevel;

        public string LogUser;

        public string LogProc;

        public string LogMessage;

        public string LogStackTrace;

        public string LogObjType;

        public string LogObjKey;

        public object LogObject;

        public Exception Ex;

        public string LoggerKey;

        public int LogActionId;

        public object Clone()
        {
            return new PmLogMsgItem()
            {
                LogDatetime = LogDatetime
                ,
                LogLevel = LogLevel
                ,
                LogUser = LogUser
                ,
                LogProc = LogProc
                ,
                LogMessage = LogMessage
                ,
                LogStackTrace = LogStackTrace
                ,
                LogObjType = LogObjType
                ,
                LogObjKey = LogObjKey
                ,
                LogObject = LogObject
                ,
                Ex = Ex
                ,
                LoggerKey = LoggerKey
                ,
                LogActionId = LogActionId
            };
        }
    }
}
