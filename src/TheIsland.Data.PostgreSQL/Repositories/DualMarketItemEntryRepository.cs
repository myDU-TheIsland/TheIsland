// <copyright file="DualMarketItemEntryRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using TheIsland.Data.Entities;
    using TheIsland.Data.PostgreSQL.Settings;
    using TheIsland.Data.Repositories;

    public class DualMarketItemEntryRepository : NpgsqlEntityRepository<DualMarketItemEntry>, IDualMarketItemEntryRepository
    {
        public DualMarketItemEntryRepository(PostgresSettings settings) : base(settings, settings.DualDatabase)
        {
        }

        public async Task<IEnumerable<DualMarketItemEntry>> GetByPlayerIdAsync(double playerId, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualMarketItemEntry>("SELECT market_item_entry.id, market_item_entry.quantity, market_item_entry.item_type_id, market_item_entry.market_id, market_item_entry.order_id, market_item_entry.owner_id FROM public.market_item_entry INNER JOIN public.ownership ON ownership.player_id = @playerId AND ownership.id = market_item_entry.owner_id", new { playerId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<DualMarketItemEntry>> GetByPlayerIdByMarketIdAsync(double playerId, double marketId, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualMarketItemEntry>("SELECT market_item_entry.id, market_item_entry.quantity, market_item_entry.item_type_id, market_item_entry.market_id, market_item_entry.order_id, market_item_entry.owner_id FROM public.market_item_entry INNER JOIN public.ownership ON ownership.player_id = @playerId AND ownership.id = market_item_entry.owner_id WHERE market_id = @marketId", new { playerId, marketId }).ConfigureAwait(false);
            }
        }
    }
}
