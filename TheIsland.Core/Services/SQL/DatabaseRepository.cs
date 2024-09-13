// <copyright file="DatabaseRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper.Contrib.Extensions;
    using Npgsql;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;

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

    public class EntityRepository<TValue> : IEntityRepository<TValue> where TValue : DatabaseEntity, new()
    {
        protected PostgresSettings Settings { get; }

        protected string DatabaseName { get; }

        public EntityRepository(PostgresSettings settings, string databaseName)
        {
            this.Settings = settings;
            this.DatabaseName = databaseName;
        }

        public virtual DbConnection GetConnection()
        {
            return new NpgsqlConnection($@"{this.Settings.GetConnectionString()}Database={this.DatabaseName};");
        }

        public virtual async Task<double> AddAsync(TValue item, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.InsertAsync(item).ConfigureAwait(false);
            }
        }

        public virtual async Task<double[]> AddAsync(IEnumerable<TValue> items, CancellationToken cancellationToken = default)
        {
            List<double> output = new List<double>();

            // loop and get the ids
            foreach (TValue item in items)
            {
                output.Add(await this.AddAsync(item, cancellationToken).ConfigureAwait(false));
            }

            return output.ToArray();
        }

        public virtual async Task<IEnumerable<TValue>> GetAsync(CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.GetAllAsync<TValue>().ConfigureAwait(false);
            }
        }

        public virtual async Task<IEnumerable<TValue>> GetAsync(IEnumerable<double> keys, CancellationToken cancellationToken = default)
        {
            if (keys == null)
            {
                throw new Exception("Keys can not be null");
            }

            using (DbConnection databaseConnection = this.GetConnection())
            {
                return (await databaseConnection.GetAllAsync<TValue>().ConfigureAwait(false)).Where(item => Enumerable.Contains<double>(keys, item.id)).ToArray();
            }
        }

        public virtual async Task<TValue> GetAsync(double key, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.GetAsync<TValue>(key).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> RemoveAsync(TValue item, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.DeleteAsync(item).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> RemoveAsync(double key, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                TValue itemToDelete = new TValue { id = key };
                return await databaseConnection.DeleteAsync(itemToDelete).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> RemoveAsync(IEnumerable<double> keys, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                TValue[] itemsToDelete = keys.Select(key => new TValue { id = key }).ToArray();
                return await databaseConnection.DeleteAsync(itemsToDelete).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> RemoveAsync(IEnumerable<TValue> items, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.DeleteAsync(items).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> UpdateAsync(IEnumerable<TValue> items, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.UpdateAsync(items).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> UpdateAsync(TValue item, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.UpdateAsync(item).ConfigureAwait(false);
            }
        }
    }
}
