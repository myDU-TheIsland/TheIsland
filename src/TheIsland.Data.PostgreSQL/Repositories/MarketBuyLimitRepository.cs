// <copyright file="MarketBuyLimitRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using TheIsland.Core.Interfaces;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;

    public class MarketBuyLimitRepository : NpgsqlEntityRepository<MarketBuyLimit>, IMarketBuyLimitRepository
    {
        public MarketBuyLimitRepository(IDatabaseSettings settings) : base(settings, settings.Database)
        {
        }
    }
}
