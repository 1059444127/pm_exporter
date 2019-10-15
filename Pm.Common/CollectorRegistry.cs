using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Common
{
    /// <summary>
    /// Список счетчиков
    /// </summary>
    public class CollectorRegistry: PmLongTask
    {
        private readonly ConcurrentBag<Action> _beforeCollectCallbacks = new ConcurrentBag<Action>();

        private readonly ConcurrentDictionary<string, ICollector> _collectors = new ConcurrentDictionary<string, ICollector>();


        public CollectorRegistry(int intervalSeconds = 60):base(intervalSeconds)
        {

        }

        /// <summary>
        /// Добавить счетчик
        /// </summary>
        /// <param name="collector"></param>
        public void GetOrAdd(ICollector collector)
        {
            var collectorToUse = _collectors.GetOrAdd(collector.Name, collector);
        }

        public void AddBeforeCollectCallback(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _beforeCollectCallbacks.Add(callback);
        }

        /// <summary>
        /// Собрать данные в строку со всех счетчиков, перед этим вызываем методы callback.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="cancel"></param>
        public void CollectAndExportAsText(ref StringBuilder sb, CancellationToken cancel)
        {
            foreach (var callback in _beforeCollectCallbacks)
                callback();

            foreach (var collector in _collectors.Values)
                collector.CollectAndSerialize(ref sb, cancel);
        }

        protected override async Task WorkStep(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
