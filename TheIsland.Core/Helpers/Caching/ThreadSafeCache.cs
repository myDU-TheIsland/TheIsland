// <copyright file="ThreadSafeCache.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Helpers.Caching
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    /// <summary>
    /// Class ThreadSafeCacheLookup.
    /// Implements the <see cref="IThreadSafeCacheLookup{TValue}" />.
    /// </summary>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    /// <seealso cref="IThreadSafeCacheLookup{TValue}" />
    public class ThreadSafeCacheLookup<TValue> : IThreadSafeCacheLookup<TValue>
    {
        private readonly ConcurrentDictionary<string, Task<TValue?>> _workers = new ConcurrentDictionary<string, Task<TValue?>>();

        /// <summary>
        /// Threads the safe lookup.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="lookup">The lookup.</param>
        /// <returns>Task&lt;TValue&gt;.</returns>
        public async Task<TValue?> ThreadSafeLookup(string key, Func<Task<TValue?>> lookup)
        {
            string keyValue = key;

            TValue? content = default;

            while (content == null)
            {
                if (!this._workers.TryGetValue(keyValue, out Task<TValue?>? result))
                {
                    // There is a small race condition here between TryGetValue and TryAdd that might cause the
                    // content to be computed more than once. We don't care about this race as the probability of
                    // happening is very small and the impact is not critical.
                    TaskCompletionSource<TValue?> tcs = new TaskCompletionSource<TValue?>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);

                    this._workers.TryAdd(keyValue, tcs.Task);
                    try
                    {
                        content = await lookup().ConfigureAwait(false);

                        if (content == null)
                        {
                            //exit the while its a null response
                            break;
                        }
                    }
                    catch
                    {
                        content = default;
                        throw;
                    }
                    finally
                    {
                        // Remove the worker task before setting the result.
                        // If the result is null, other threads would potentially
                        // acquire it otherwise.
                        this._workers.TryRemove(keyValue, out _);

                        // Notify all other awaiters to render the content
                        tcs.TrySetResult(content);
                    }
                }
                else
                {
                    content = await result.ConfigureAwait(false);
                }
            }

            return content;
        }
    }
}
