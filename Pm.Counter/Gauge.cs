using Pm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Counter
{
    public class Gauge : CollectorCommon, ICollector
    {
        private ThreadSafeDouble _value;

        public void Inc(double increment = 1)
        {

        }
        public void Set(double val)
        {

        }
        public void Dec(double decrement = 1)
        {

        }
        public double Value { get; }

        public Gauge(string name, string label) : base(name, label)
        {
        }

        public void CollectAndSerialize(ref StringBuilder sb, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
    }
}
