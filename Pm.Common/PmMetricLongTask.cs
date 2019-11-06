using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Common
{
    public class PmMetricLongTask: PmLongTask
    {
        protected readonly CollectorRegistry CollectorRegistry;

        public Func<bool> IsAllOkFunc = () => false;

        public event EventHandler OnErrorWhenCollect;

        protected StringBuilder sb = new StringBuilder(2048);

        public PmMetricLongTask(CollectorRegistry collectorRegistry, int intervalSeconds) :base(intervalSeconds)
        {
            CollectorRegistry = collectorRegistry;
        }

        public bool CollectData(ref StringBuilder _sb)
        {
            try
            {
                _sb.Clear();

                CollectorRegistry.CollectAndExportAsText(ref _sb, CancelToken);

                return true;
            }
            catch (Exception ex)
            {
                ErrorWhenCollect();

                Log.LogError(ex);

                Trace.WriteLine(string.Format("Error while collect: {0}", ex));

                return false;
            }
        }

        public void ErrorWhenCollect()
        {
            OnErrorWhenCollect?.Invoke(this, new EventArgs());
        }
    }
}
