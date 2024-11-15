// <copyright file="CraftedItemRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using Dapper.Contrib.Extensions;
    using Npgsql;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class CraftedItemRepository : EntityRepository<CraftedItem>
    {
        public CraftedItemRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }
    }
}
