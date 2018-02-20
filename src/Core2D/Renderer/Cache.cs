﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;

namespace Core2D.Renderer
{
    /// <summary>
    /// Generic key value cache.
    /// </summary>
    /// <typeparam name="TKey">The input type.</typeparam>
    /// <typeparam name="TValue">The output type.</typeparam>
    public class Cache<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        private IDictionary<TKey, TValue> _storage;
        private Action<TValue> _dispose;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dispose">The dispose action.</param>
        public Cache(Action<TValue> dispose = null)
        {
            _dispose = dispose;
            _storage = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Creates a new <see cref="Cache{TKey, TValue}"/> instance.
        /// </summary>
        /// <param name="dispose">The dispose action.</param>
        /// <returns>The new instance of the <see cref="Cache{TKey, TValue}"/> class.</returns>
        public static Cache<TKey, TValue> Create(Action<TValue> dispose = null)
        {
            return new Cache<TKey, TValue>(dispose);
        }

        /// <summary>
        /// Resets cache storage.
        /// </summary>
        public void Reset()
        {
            if (_storage != null)
            {
                if (_dispose != null)
                {
                    foreach (var data in _storage)
                    {
                        _dispose(data.Value);
                    }
                }
                _storage.Clear();
            }
            _storage = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Sets or adds new value to storage.
        /// </summary>
        /// <param name="key">The key object.</param>
        /// <param name="value">The value object.</param>
        public void Set(TKey key, TValue value)
        {
            if (_storage.ContainsKey(key))
            {
                _storage[key] = value;
            }
            else
            { 
                _storage.Add(key, value);
            }
        }

        /// <summary>
        /// Gets value from storage.
        /// </summary>
        /// <param name="key">The key object.</param>
        /// <returns>The value from storage.</returns>
        public TValue Get(TKey key)
        {
            if (_storage.TryGetValue(key, out TValue data))
            {
                return data;
            }
            return default(TValue);
        }
    }
}
