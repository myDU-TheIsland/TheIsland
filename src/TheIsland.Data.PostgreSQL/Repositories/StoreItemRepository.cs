// <copyright file="StoreItemRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using TheIsland.Data.Entities;
    using TheIsland.Data.PostgreSQL.Settings;
    using TheIsland.Data.Repositories;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Duplicate rules overlapping")]
    public class StoreItemRepository(PostgresSettings settings) : NpgsqlEntityRepository<StoreItemEntity>(settings, settings.Database), IStoreItemRepository
    {
    }
}
