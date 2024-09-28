// <copyright file="DualMarketTransactionRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;

    public class DualMarketTransactionRepository : EntityRepository<DualMarketTransaction>
    {
        public DualMarketTransactionRepository(PostgresSettings settings) : base(settings, settings.DualDatabase)
        {
        }

        public async Task<IEnumerable<DualMarketTransaction>> GetAllAfterIdAsync(double id)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualMarketTransaction>("SELECT * FROM public.market_order WHERE owner_id not IN (1,7,3,2) and id > @Id;", new { Id = id }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<DualMarketTransaction>> GetAllActiveAsync(double marketId, double itemId)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                if (marketId == 0)
                {
                    return await databaseConnection.QueryAsync<DualMarketTransaction>("SELECT * FROM public.market_order WHERE completion_date is null AND item_type_id = @ItemId;", new { ItemId = itemId }).ConfigureAwait(false);
                }
                else
                {
                    return await databaseConnection.QueryAsync<DualMarketTransaction>("SELECT * FROM public.market_order WHERE completion_date is null AND item_type_id = @ItemId AND market_id = @MarketId;", new { MarketId = marketId, ItemId = itemId }).ConfigureAwait(false);
                }
            }
        }

        public async Task<IEnumerable<DualMarketTransaction>> GetAllByPlayerAndMarket(double marketId, double ownerId)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualMarketTransaction>("SELECT * FROM public.market_order where market_id = @MarketId and owner_id = @OwnerId;", new { MarketId = marketId, OwnerId = ownerId }).ConfigureAwait(false);
            }
        }
    }
}
