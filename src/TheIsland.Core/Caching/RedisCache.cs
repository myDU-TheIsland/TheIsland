// <copyright file="RedisCache.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Caching
{
    using System;
    using System.Text.Json;
    using StackExchange.Redis;

    public class RedisCache<TValue>
    {
        private readonly IDatabase _database;

        public RedisCache(IDatabase redisDatabase)
        {
            this._database = redisDatabase;
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="cacheTimeout">The cache timeout.</param>
        public bool Add(string key, TValue item, TimeSpan cacheTimeout)
        {
            cacheTimeout = cacheTimeout.TotalMilliseconds > 0 ? cacheTimeout : TimeSpan.FromSeconds(1);
            return this._database.StringSet(key, JsonSerializer.Serialize(item), cacheTimeout);
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>TValue.</returns>
        public TValue? Get(string key)
        {
            string? result = this._database.StringGet(key);

            return !string.IsNullOrEmpty(result) ? JsonSerializer.Deserialize<TValue>(result) : default;
        }
    }
}