// <copyright file="FactoryLedgerRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Interfaces;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;

    public class FactoryLedgerRepository : NpgsqlEntityRepository<FactoryLedgerEntry>, IFactoryLedgerRepository
    {
        public FactoryLedgerRepository(IDatabaseSettings settings) : base(settings, settings.Database)
        {
        }

        public async Task<IEnumerable<FactoryLedgerEntry>> GetAllEntriesByItemIdsAsync(double[] itemIds, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<FactoryLedgerEntry>("SELECT * FROM public.factory_ledger WHERE item_id = ANY(@itemIds)", new { itemIds }).ConfigureAwait(false);
            }
        }
    }
}
