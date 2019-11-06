using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Common
{
    public class FastLocker
    {
        public object Tag;

        const long Locked = 1;

        const long Unlocked = 0;

        private long _locker = 0;

        private int _waitStepDefault = 10;

        public FastLocker()
        {

        }


        public async Task<bool> TryLockAsync(int waitMs, CancellationToken ct)
        {
            var cn = 1;
            var waitStep = _waitStepDefault;

            GetStepCount(ref cn, ref waitStep, ref waitMs);


            while (cn > 0)
            {
                if (Interlocked.CompareExchange(ref _locker, Locked, Unlocked) == Unlocked)
                    return true;

                await Task.Delay(waitStep, ct);

                cn--;
            }

            return false;
        }

        public async Task<bool> TryLockAsync(int waitMs)
        {
            var cn = 1;
            var waitStep = _waitStepDefault;

            GetStepCount(ref cn, ref waitStep, ref waitMs);

            while (cn > 0)
            {
                if (Interlocked.CompareExchange(ref _locker, Locked, Unlocked) == Unlocked)
                    return true;

                await Task.Delay(waitStep);

                cn--;
            }

            return false;
        }

        public bool TryLock(int waitMs)
        {
            var cn = 1;
            var waitStep = _waitStepDefault;

            GetStepCount(ref cn, ref waitStep, ref waitMs);

            while (cn > 0)
            {
                if (Interlocked.CompareExchange(ref _locker, Locked, Unlocked) == Unlocked)
                    return true;

                Thread.Sleep(waitStep);

                cn--;
            }

            return false;
        }

        /// <summary>
        /// Дожидаемся пока блокировка стоит (но блокировку при этом не ставим).
        /// Механизм нужен для того чтоб пока стоит блокировка - код внутри блока не начинал вполняться
        /// </summary>
        /// <param name="waitMs"></param>
        /// <returns></returns>
        public async Task<bool> TryOverStepAsync(int waitMs)
        {
            var cn = 1;
            var waitStep = _waitStepDefault;

            GetStepCount(ref cn, ref waitStep, ref waitMs);

            while (cn > 0 && _locker == Locked)
            {
                await Task.Delay(waitStep);
                cn--;
            }

            return _locker == Unlocked;
        }

        private void GetStepCount(ref int cn, ref int waitStep, ref int waitTotal)
        {
            if (waitTotal < waitStep)
            {
                waitStep = waitTotal;
                cn = 1;
            }
            else
            {
                cn = (int)(waitTotal / waitStep);
            }

            if (waitStep == 0)
                waitStep = 5;

            if (cn <= 0)
                cn = 1;
        }

        public void ReleaseLock()
        {
            Volatile.Write(ref _locker, Unlocked);
        }
    }
}
