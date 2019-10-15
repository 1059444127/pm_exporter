using Pm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Counter
{
    public class Counter : CollectorCommon, ICollector
    {
        private ThreadSafeDouble _value;

        public void Inc(double increment = 1) => _value.Add(increment);

        public void CollectAndSerialize(ref StringBuilder sb, CancellationToken cToken)
        {
            sb.Append($"# HELP {Name} {Help}\n");
            sb.Append($"# TYPE {Name} counter\n");
            sb.Append($"{Name} {_value.ToString()}\n");
        }

        public Counter(string name, string label):base(name,label)
        {

        }
    }
}
