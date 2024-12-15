// <copyright file="IDualMarketOrderRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IDualMarketOrderRepository : IEntityRepository<DualMarketOrder>
    {
        Task<IEnumerable<DualMarketOrder>> GetByMarketIdAsync(double marketId, CancellationToken cancellationToken = default);
    }
}
