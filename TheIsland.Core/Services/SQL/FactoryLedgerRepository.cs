// <copyright file="FactoryLedgerRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using Dapper.Contrib.Extensions;
    using Npgsql;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class FactoryLedgerRepository : EntityRepository<FactoryLedgerEntry>
    {
        public FactoryLedgerRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }

        public async Task<IEnumerable<FactoryLedgerEntry>> GetAllEntriesByItemIdsAsync(double[] itemIds, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<FactoryLedgerEntry>("SELECT * FROM public.factory_ledger WHERE item_id IN @itemIds", new { itemIds }).ConfigureAwait(false);
            }
        }
    }
}
