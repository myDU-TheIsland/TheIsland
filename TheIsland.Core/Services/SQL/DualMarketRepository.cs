// <copyright file="DualMarketRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;

    public class DualMarketRepository : EntityRepository<DualMarket>
    {
        public DualMarketRepository(PostgresSettings settings) : base(settings, settings.DualDatabase)
        {
        }
    }
}
