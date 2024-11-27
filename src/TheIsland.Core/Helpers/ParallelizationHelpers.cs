// <copyright file="ParallelizationHelpers.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ParallelizationHelpers
    {
        /// <summary>
        /// The for each async.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="dop">
        /// The dop.
        /// </param>
        /// <param name="body">
        /// The body.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <remarks>Referenced from https://blogs.msdn.microsoft.com/pfxteam/2012/03/05/implementing-a-simple-foreachasync-part-2/.</remarks>
        public static Task ForEachAsync<T>(
            this IEnumerable<T> source, int dop, Func<T, Task> body, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken == default(CancellationToken))
            {
                cancellationToken = CancellationToken.None;
            }

            return Task.WhenAll(
                Partitioner.Create(source)
                    .GetPartitions(dop)
                    .Select(
                        partition => Task.Run(
                            async () =>
                            {
                                using (partition)
                                {
                                    while (partition.MoveNext())
                                    {
                                        if (cancellationToken.IsCancellationRequested)
                                        {
                                            //break the while loop
                                            return;
                                        }

                                        await body(partition.Current).ConfigureAwait(false);
                                    }
                                }
                            }, cancellationToken)));
        }
    }
}
