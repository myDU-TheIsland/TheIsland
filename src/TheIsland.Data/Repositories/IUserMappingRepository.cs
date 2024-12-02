// <copyright file="IUserMappingRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IUserMappingRepository : IEntityRepository<UserMapping>
    {
        Task<IEnumerable<UserMapping>> FindByDiscordId(string discordId, CancellationToken cancellationToken = default);
    }
}
