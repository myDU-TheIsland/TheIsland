// <copyright file="IDualWalletRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IDualWalletRepository : IEntityRepository<DualWalletTransaction>
    {
        Task<IEnumerable<DualWalletTransaction>> GetAllBotTransactionOnMarket(double marketId, double entity_id = 43453);

        Task<IEnumerable<MarketStatistics>> GetDailyStats(double item, double marketId = -1);
    }
}
