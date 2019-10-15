using Pm.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Common
{
    public class PmLongTask
    {
        protected IPmLogger Log;

        private CancellationTokenSource _cts;

        private CancellationToken _ct;
        public CancellationToken CancelToken => _ct;

        private Task _task;

        private readonly TimeSpan _stepInterval;

        System.Threading.Timer _wakeupTimer = null;

        private EventWaitHandle _wh = new AutoResetEvent(false);


        public PmLongTask(int intervalSeconds = 60)
        {
            _stepInterval = TimeSpan.FromSeconds(intervalSeconds);
        }


        public virtual Task Start()
        {
            _wakeupTimer = new System.Threading.Timer(WakeupTick, null, new TimeSpan(0, 0, 10), _stepInterval);

            _task = Task.Run((async () => await WorkLoop()));

            return _task;
        }
        public virtual void Stop()
        {
            _cts.Cancel();
            _wh.Reset();

            try
            {
                _task.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }
        }

        protected virtual void WakeupTick(object state)
        {
            _wh.Set();
        }
        public virtual async Task WorkLoop()
        {
            _cts = new CancellationTokenSource();
            _ct = _cts.Token;

            while (true)
            {
                if (_cts.IsCancellationRequested)
                    break;

                try
                {
                   await WorkStep(_ct);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Trace.WriteLine(string.Format("Error in MetricPusher: {0}", ex));
                }

                if (_cts.IsCancellationRequested)
                    break;

                _wh.WaitOne();

            }
        }

        protected virtual async Task WorkStep(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

    }
}
