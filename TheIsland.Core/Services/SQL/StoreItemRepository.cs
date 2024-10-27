// <copyright file="StoreItemRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Duplicate rules overlapping")]
    public class StoreItemRepository(PostgresSettings settings) : EntityRepository<StoreItemEntity>(settings, settings.Database)
    {
    }
}
