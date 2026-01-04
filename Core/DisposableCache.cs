using System;

namespace com.superneko.medlay.Core
{
    internal class DisposableCache<K, V> : Cache<K, V> where V : IDisposable
    {
        public DisposableCache() : base(DisposeValue) { }

        static void DisposeValue(V value) {
            value.Dispose();
        }
    }
}
