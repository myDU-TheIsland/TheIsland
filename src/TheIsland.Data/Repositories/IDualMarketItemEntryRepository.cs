// <copyright file="IDualMarketItemEntryRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IDualMarketItemEntryRepository : IEntityRepository<DualMarketItemEntry>
    {
        Task<IEnumerable<DualMarketItemEntry>> GetByPlayerIdAsync(double playerId, CancellationToken cancellationToken = default);

        Task<IEnumerable<DualMarketItemEntry>> GetByPlayerIdByMarketIdAsync(double playerId, double marketId, CancellationToken cancellationToken = default);
    }
}
