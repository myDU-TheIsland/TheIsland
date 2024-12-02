// <copyright file="ILinkTokenRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface ILinkTokenRepository : IEntityRepository<LinkToken>
    {
        Task<LinkToken?> FindByPlayerId(double playerId, CancellationToken cancellationToken = default);

        Task<LinkToken?> FindByToken(string token, CancellationToken cancellationToken = default);
    }
}
