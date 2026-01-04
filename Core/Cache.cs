using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace com.superneko.medlay.Core
{
    internal class Entry<T>
    {
        long lastTouched;
        long lifetime;

        public readonly T Value;

        public Entry(long currentTime, T value, long lifetime = 300) {
            lastTouched = currentTime;
            this.lifetime = lifetime;
            Value = value;
        }

        public bool ExpiredIn(long currentTime) {
            return lastTouched + lifetime < currentTime;
        }

        public void TouchIn(long currentTime) {
            lastTouched = currentTime;
        }
    }

    internal class Cache<K, V> {
        long checkTick = 60;
        long time;
        Dictionary<K, Entry<V>> dictionary;
        Action<V> OnDispose;

        public Cache(Action<V> OnDispose = null) {
            dictionary = new Dictionary<K, Entry<V>>();
            this.OnDispose = OnDispose;
        }

        ~Cache() {
            Clear();
        }

        public V GetOrCalculate(K key, Func<V> calculateValue) {
            if (!dictionary.TryGetValue(key, out Entry<V> entry)) {
                V value = calculateValue();
                dictionary[key] = new Entry<V>(time, value);
                return value;
            }

            dictionary[key].TouchIn(time);
            return entry.Value;
        }

        public void Invalidate(K key) {
            if (dictionary.TryGetValue(key, out Entry<V> entry)) {
                dictionary[key] = new Entry<V>(0, entry.Value);
            }
        }

        public void Tick()
        {
            time++;

            if (time % checkTick != 0) {
                return;
            }

            List<K> keysToRemove = new List<K>();
            foreach (var kvp in dictionary) {
                K key = kvp.Key;
                Entry<V> entry = kvp.Value;
                if (entry.ExpiredIn(time)) {
                    OnDispose?.Invoke(kvp.Value.Value);
                    keysToRemove.Add(key);
                }
            }

            foreach (K key in keysToRemove) {
                dictionary.Remove(key);
            }
        }

        public void Clear() {
            foreach (var kvp in dictionary) {
                K key = kvp.Key;
                OnDispose?.Invoke(kvp.Value.Value);
            }

            dictionary.Clear();
        }
    }
}
