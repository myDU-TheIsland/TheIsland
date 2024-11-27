// <copyright file="MarketStockRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class MarketStockRepository : EntityRepository<MarketStock>
    {
        public MarketStockRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }
    }
}
