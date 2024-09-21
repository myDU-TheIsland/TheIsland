// <copyright file="MarketService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Services.SQL.Entities;

    public class MarketService
    {
        private readonly MarketTransactionRepository _transactionRepository;
        private readonly DualMarketRepository _dualMarketRepository;

        public MarketService(MarketTransactionRepository transactionRepository, DualMarketRepository dualMarketRepository)
        {
            this._transactionRepository = transactionRepository;
            this._dualMarketRepository = dualMarketRepository;
        }

        public async Task<IEnumerable<MarketStatistics>> GetHourlyStats(double itemId)
        {
            var results = await this._transactionRepository.GetHourlyStats(itemId).ConfigureAwait(false);

            var markets = await this._dualMarketRepository.GetAsync().ConfigureAwait(false);

            foreach (var item in results)
            {
                item.market_name = markets.FirstOrDefault(market => market.id == item.market_id)?.name ?? string.Empty;
            }

            return results;
        }
    }
}
