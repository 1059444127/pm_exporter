using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Log
{
    public interface IPmLogWriter
    {
        void InitWriter();

        void CloseWriter();

        /// <summary>
        /// Функция которая будет ловить события от IMcsLogHandler
        /// </summary>
        /// <param name="item"></param>
        void CatchLogMessage(PmLogMsgItem item);
    }
}
