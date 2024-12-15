// <copyright file="DatabaseRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TheIsland.Data.Entities;

    public interface IEntityRepository<TValue> where TValue : DatabaseEntity, new()
    {
        /// <summary>
        /// Adds the item asynchronous.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<double> AddAsync(TValue item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the items asynchronous.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<double[]> AddAsync(IEnumerable<TValue> items, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all the items asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;IEnumerable&lt;TValue&gt;&gt;.</returns>
        Task<IEnumerable<TValue>> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the items with the specified keys asynchronous.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;IEnumerable&lt;TValue&gt;&gt;.</returns>
        Task<IEnumerable<TValue>> GetAsync(IEnumerable<double> keys, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the item with the specified key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;TValue&gt;.</returns>
        Task<TValue> GetAsync(double key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the item asynchronous.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> RemoveAsync(TValue item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the item with the specified key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> RemoveAsync(double key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the items with the specified keys asynchronous.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Double&gt;.</returns>
        Task<bool> RemoveAsync(IEnumerable<double> keys, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the items asynchronous.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Double&gt;.</returns>
        Task<bool> RemoveAsync(IEnumerable<TValue> items, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the items asynchronous.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Double&gt;.</returns>
        Task<bool> UpdateAsync(IEnumerable<TValue> items, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the item asynchronous.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> UpdateAsync(TValue item, CancellationToken cancellationToken = default);
    }
}
