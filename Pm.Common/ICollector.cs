using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Common
{
    public interface ICollector
    {
        void CollectAndSerialize(ref StringBuilder sb, CancellationToken cToken);

        string Name { get; }

        string Help { get; }
    }
}
