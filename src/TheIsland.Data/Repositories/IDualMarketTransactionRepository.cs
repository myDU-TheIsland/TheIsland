// <copyright file="IDualMarketTransactionRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IDualMarketTransactionRepository : IEntityRepository<DualMarketTransaction>
    {
        Task<IEnumerable<DualMarketTransaction>> GetAllAfterIdAsync(double id);

        Task<IEnumerable<DualMarketTransaction>> GetAllActiveAsync(double marketId, double itemId);

        Task<int> DisableBotSeedOrders(double marketId);

        Task<int> EnableBotSeedOrders(double marketId);
    }
}
