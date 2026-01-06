using System;
using System.Threading;

namespace com.superneko.medlay.Core
{
    internal static class LongId
    {
        static long counter = 0;

        public static long Generate()
        {
            return DateTime.Now.Ticks * 1000_000 + (counter % 1000) * 1000 + Thread.CurrentThread.ManagedThreadId % 1000;
        }
    }
}
