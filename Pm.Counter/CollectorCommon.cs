using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Counter
{
    /// <summary>
    /// Счетчик.
    /// </summary>
    public abstract class CollectorCommon
    {
        public virtual string Name { get; }

        public virtual string Help{get;}
        public CollectorCommon(string name, string label)
        {
            Name = name;
            Help = label;
        }
    }
}
