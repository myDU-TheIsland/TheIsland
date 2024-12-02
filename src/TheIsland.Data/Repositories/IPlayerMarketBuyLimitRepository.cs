// <copyright file="IPlayerMarketBuyLimitRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IPlayerMarketBuyLimitRepository : IEntityRepository<PlayerMarketBuyLimit>
    {
        Task<PlayerMarketBuyLimit?> GetByPlayerIdAsync(double playerId, double marketId, double itemId, CancellationToken cancellationToken = default);

        public static PlayerMarketBuyLimit GetTimeframe()
        {
            DateTime now = DateTime.Now;
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, now.AddHours(-(now.Hour % 3)).Hour, 0, 0, DateTimeKind.Local).ToLocalTime();
            DateTime endTime = startTime.AddHours(3).AddSeconds(-1).ToLocalTime();

            return new PlayerMarketBuyLimit
            {
                start_time = startTime,
                end_time = endTime,
            };
        }
    }
}
