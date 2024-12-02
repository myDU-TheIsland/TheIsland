// <copyright file="IDualPlayerRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IDualPlayerRepository : IEntityRepository<DualPlayer>
    {
        Task<bool> IsBotByPlayerId(double playerId, CancellationToken cancellationToken = default);

        Task<DualPlayer?> FindByDisplayName(string displayName, CancellationToken cancellationToken = default);

        Task UpdateWallet(double playerId, double amount, CancellationToken cancellationToken = default);
    }
}
