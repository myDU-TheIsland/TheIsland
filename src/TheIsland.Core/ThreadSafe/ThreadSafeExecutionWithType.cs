// <copyright file="ThreadSafeExecutionWithType.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.ThreadSafe
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    public class ThreadSafeExecutionWithType<TValue>
    {
        private static readonly ConcurrentDictionary<string, Task<TValue?>> _workers = new ConcurrentDictionary<string, Task<TValue?>>();

        /// <summary>
        /// Threads the safe execution.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="lookup">The lookup.</param>
        /// <returns>Task&lt;TValue&gt;.</returns>
        public static async Task<TValue?> ThreadSafeExecution(string key, Func<Task<TValue?>> lookup)
        {
            TValue? content = default;

            if (!_workers.TryGetValue(key, out Task<TValue?>? result))
            {
                // There is a small race condition here between TryGetValue and TryAdd that might cause the
                // content to be computed more than once. We don't care about this race as the probability of
                // happening is very small and the impact is not critical.
                TaskCompletionSource<TValue?> tcs = new TaskCompletionSource<TValue?>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);

                _workers.TryAdd(key, tcs.Task);
                try
                {
                    content = await lookup().ConfigureAwait(false);
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
                    _workers.TryRemove(key, out _);

                    // Notify all other awaiters to render the content
                    tcs.TrySetResult(content);
                }
            }
            else
            {
                await result.ConfigureAwait(false);
            }

            return content;
        }
    }
}
