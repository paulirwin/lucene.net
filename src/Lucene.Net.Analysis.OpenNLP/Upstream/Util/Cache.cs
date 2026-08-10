/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using J2N.Collections.Generic;

namespace Opennlp.Tools.Util
{
    /// <summary>
    /// Provides fixed size, pre-allocated, least recently used replacement cache.
    /// <para/>
    /// LUCENENET: upstream extends java.util.LinkedHashMap and overrides its
    /// <c>removeEldestEntry</c> hook to bound the size. J2N's
    /// <see cref="LinkedDictionary{TKey, TValue}"/> preserves insertion order but
    /// has no such hook, so the eviction is performed explicitly on insert.
    /// <para/>
    /// Note that upstream constructs the map in insertion-order mode (not
    /// access-order), so the eviction policy is really first-in-first-out; that
    /// behavior is preserved here.
    /// </summary>
    internal class Cache<K, V> : LinkedDictionary<K, V>
    {
        private readonly int capacity;

        public Cache(int capacity)
        {
            this.capacity = capacity;
        }

        private void EvictIfNecessary()
        {
            // LUCENENET: mirrors "return this.size() > this.capacity" in
            // removeEldestEntry, which evicts at most one entry per insertion.
            if (this.Count > this.capacity)
            {
                foreach (K eldest in this.Keys)
                {
                    this.Remove(eldest);
                    break;
                }
            }
        }

        public new V this[K key]
        {
            // LUCENENET: Java's Map.get() returns null for an absent key, whereas the
            // .NET indexer throws KeyNotFoundException. Callers ported from Java rely
            // on the null-return behavior to detect a cache miss.
            get => base.TryGetValue(key, out V value) ? value : default;
            set
            {
                base[key] = value;
                EvictIfNecessary();
            }
        }

        public new void Add(K key, V value)
        {
            base.Add(key, value);
            EvictIfNecessary();
        }

        /// <summary>
        /// LUCENENET: an instance method that takes precedence over the
        /// <c>Put</c> extension method on <see cref="IDictionary{TKey, TValue}"/>,
        /// which would otherwise assign through the interface indexer and bypass
        /// the eviction above.
        /// </summary>
        public V Put(K key, V value)
        {
            base.TryGetValue(key, out V oldValue);
            base[key] = value;
            EvictIfNecessary();
            return oldValue;
        }

        // LUCENENET specific method from HashMap in Java
        public V ComputeIfAbsent(K key, Func<K, V> func)
        {
            if (this.TryGetValue(key, out V value))
            {
                return value;
            }
            else
            {
                V newValue = func(key);
                this[key] = newValue;
                return newValue;
            }
        }
    }
}
