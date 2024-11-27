// <copyright file="LastReadRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper.Contrib.Extensions;
    using Npgsql;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class LastReadRepository : IAppService
    {
        protected PostgresSettings Settings { get; }

        protected string DatabaseName { get; }

        public LastReadRepository(PostgresSettings settings)
        {
            this.Settings = settings;
            this.DatabaseName = settings.Database;
        }

        public DbConnection GetConnection()
        {
            return new NpgsqlConnection($@"{this.Settings.GetConnectionString()}Database={this.DatabaseName};");
        }

        public async Task<double> AddAsync(LastRead item, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.InsertAsync(item).ConfigureAwait(false);
            }
        }

        public async Task<LastRead> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.GetAsync<LastRead>(key).ConfigureAwait(false);
            }
        }

        public async Task<bool> UpdateAsync(LastRead item, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.UpdateAsync(item).ConfigureAwait(false);
            }
        }
    }
}
