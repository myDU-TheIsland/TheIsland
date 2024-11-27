// <copyright file="MarketBuyLimitRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class MarketBuyLimitRepository : EntityRepository<MarketBuyLimit>
    {
        public MarketBuyLimitRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }
    }
}
