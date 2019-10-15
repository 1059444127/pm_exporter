using Pm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Counter
{
    public class Histogram : CollectorCommon, ICollector
    {
        public Histogram(string name, string label) : base(name, label)
        {
        }

        public void CollectAndSerialize(ref StringBuilder sb, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
    }
}
