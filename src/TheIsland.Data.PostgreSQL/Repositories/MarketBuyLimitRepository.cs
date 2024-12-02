// <copyright file="MarketBuyLimitRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using TheIsland.Data.Entities;
    using TheIsland.Data.PostgreSQL.Settings;
    using TheIsland.Data.Repositories;

    public class MarketBuyLimitRepository : NpgsqlEntityRepository<MarketBuyLimit>, IMarketBuyLimitRepository
    {
        public MarketBuyLimitRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }
    }
}
