using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Counter
{
    public struct ThreadSafeLong
    {
        private long _value;

        public ThreadSafeLong(long value)
        {
            _value = value;
        }

        public long Value
        {
            get
            {
                return Interlocked.Read(ref _value);
            }
            set
            {
                Interlocked.Exchange(ref _value, value);
            }
        }

        public void Add(long increment)
        {
            while (true)
            {
                long initialValue = _value;
                long computedValue = initialValue + increment;

                //Compare exchange will only set the computed value if it is equal to the expected value
                //It will always return the the value of _value prior to the exchange (whether it happens or not)
                //So, only exit the loop if the value was what we expected it to be (initialValue) at the time of exchange otherwise another thread updated and we need to try again.
                if (initialValue == Interlocked.CompareExchange(ref _value, computedValue, initialValue))
                    return;
            }
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            if (obj is ThreadSafeLong)
                return Value.Equals(((ThreadSafeLong)obj).Value);

            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
