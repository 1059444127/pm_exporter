using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pm.Common
{
    public interface IMetricSource<T>
    {
        void CollectMetrics(ref T[] metrics, ref int count);
    }
}
