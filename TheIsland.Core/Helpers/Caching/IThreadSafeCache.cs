// <copyright file="IThreadSafeCache.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Helpers.Caching
{
    /// <summary>
    /// Interface IThreadSafeCacheLookup.
    /// </summary>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    public interface IThreadSafeCacheLookup<TValue>
    {
        /// <summary>
        /// Threads the safe lookup.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="lookup">The lookup.</param>
        /// <returns>Task&lt;TValue&gt;.</returns>
        Task<TValue?> ThreadSafeLookup(string key, Func<Task<TValue?>> lookup);
    }
}
