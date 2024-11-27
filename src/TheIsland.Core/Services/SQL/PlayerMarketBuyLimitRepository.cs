// <copyright file="PlayerMarketBuyLimitRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Dapper.Contrib.Extensions;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class PlayerMarketBuyLimitRepository : EntityRepository<PlayerMarketBuyLimit>
    {
        public PlayerMarketBuyLimitRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }

        public static PlayerMarketBuyLimit GetTimeframe()
        {
            DateTime now = DateTime.Now;
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, now.AddHours(-(now.Hour % 3)).Hour, 0, 0, DateTimeKind.Local).ToLocalTime();
            DateTime endTime = startTime.AddHours(3).AddSeconds(-1).ToLocalTime();

            return new PlayerMarketBuyLimit { start_time = startTime, end_time = endTime };
        }

        public override async Task<double> AddAsync(PlayerMarketBuyLimit item, CancellationToken cancellationToken = default)
        {
            PlayerMarketBuyLimit timeframe = GetTimeframe();

            item.start_time = timeframe.start_time;
            item.end_time = timeframe.end_time;

            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.InsertAsync(item).ConfigureAwait(false);
            }
        }

        public async Task<PlayerMarketBuyLimit?> GetByPlayerIdAsync(double playerId, double marketId, double itemId, CancellationToken cancellationToken = default)
        {
            PlayerMarketBuyLimit timeframe = GetTimeframe();

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
