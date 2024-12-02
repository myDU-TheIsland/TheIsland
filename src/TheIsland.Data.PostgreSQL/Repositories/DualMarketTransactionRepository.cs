// <copyright file="DualMarketTransactionRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Data.Entities;
    using TheIsland.Data.PostgreSQL.Settings;
    using TheIsland.Data.Repositories;

    public class DualMarketTransactionRepository : NpgsqlEntityRepository<DualMarketTransaction>, IDualMarketTransactionRepository
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

        public async Task<int> DisableBotSeedOrders(double marketId)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.ExecuteAsync("UPDATE public.market_order SET completion_date = @DateTime where market_id = @MarketId and owner_id = @OwnerId;", new { MarketId = marketId, OwnerId = 3, @DateTime = DateTime.UtcNow.AddDays(-3) }).ConfigureAwait(false);
            }
        }

        public async Task<int> EnableBotSeedOrders(double marketId)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.ExecuteAsync("UPDATE public.market_order SET completion_date = null where market_id = @MarketId and owner_id = @OwnerId;", new { MarketId = marketId, OwnerId = 3 }).ConfigureAwait(false);
            }
        }
    }
}
