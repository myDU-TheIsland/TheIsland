// <copyright file="PlayerMarketBuyLimitRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using System.Data.Common;
    using Dapper;
    using Dapper.Contrib.Extensions;
    using TheIsland.Data.Entities;
    using TheIsland.Data.PostgreSQL.Settings;
    using TheIsland.Data.Repositories;

    public class PlayerMarketBuyLimitRepository : NpgsqlEntityRepository<PlayerMarketBuyLimit>, IPlayerMarketBuyLimitRepository
    {
        public PlayerMarketBuyLimitRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }

        public override async Task<double> AddAsync(PlayerMarketBuyLimit item, CancellationToken cancellationToken = default)
        {
            PlayerMarketBuyLimit timeframe = IPlayerMarketBuyLimitRepository.GetTimeframe();

            item.start_time = timeframe.start_time;
            item.end_time = timeframe.end_time;

            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.InsertAsync(item).ConfigureAwait(false);
            }
        }

        public async Task<PlayerMarketBuyLimit?> GetByPlayerIdAsync(double playerId, double marketId, double itemId, CancellationToken cancellationToken = default)
        {
            PlayerMarketBuyLimit timeframe = IPlayerMarketBuyLimitRepository.GetTimeframe();

            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryFirstOrDefaultAsync<PlayerMarketBuyLimit?>(
                    "SELECT * FROM public.player_market_buy_limit WHERE player_id = @playerId AND market_id = @marketId AND item_id = @itemId AND start_time = @startTime AND end_time = @endTime",
                    new
                    {
                        playerId,
                        marketId,
                        itemId,
                        startTime = timeframe.start_time,
                        endTime = timeframe.end_time,
                    }).ConfigureAwait(false);
            }
        }
    }
}
