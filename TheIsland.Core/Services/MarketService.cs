// <copyright file="MarketService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NQ;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Helpers;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;

    public class MarketService
    {
        private readonly MarketTransactionRepository _transactionRepository;
        private readonly DualMarketTransactionRepository _dualMarketTransactionRepository;
        private readonly DualMarketRepository _dualMarketRepository;
        private readonly DualWalletRepository _dualWalletRepository;
        private readonly MarketBotConfig _marketBotConfig;
        private readonly IDUClient _dualClient;

        public MarketService(
            MarketTransactionRepository transactionRepository,
            DualMarketRepository dualMarketRepository,
            DualMarketTransactionRepository dualMarketTransactionRepository,
            DualWalletRepository dualWalletRepository,
            MarketBotConfig marketBotConfig,
            IDUClient dualClient)
        {
            this._transactionRepository = transactionRepository;
            this._dualMarketRepository = dualMarketRepository;
            this._dualMarketTransactionRepository = dualMarketTransactionRepository;
            this._dualWalletRepository = dualWalletRepository;
            this._marketBotConfig = marketBotConfig;
            this._dualClient = dualClient;
        }

        public async Task<List<string>> CancelAllBotOrders()
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            var markets = await this._dualMarketRepository.GetAsync().ConfigureAwait(false);

            foreach (var market in markets)
            {
                logMessage($@"Processing Market {market.name} ({market.id})");
                await this._dualClient.CancelBotOrders(Convert.ToUInt64(market.id)).ConfigureAwait(false);
                logMessage($@"Market {market.name} ({market.id}) Complete!");
            }

            return log;
        }

        public async Task<List<string>> SellAllMarketsContainerContents()
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            log.AddRange(await this.CancelAllBotOrders().ConfigureAwait(false));

            var markets = await this._dualMarketRepository.GetAsync().ConfigureAwait(false);

            foreach (var market in markets)
            {
                logMessage($@"Processing Market {market.name} ({market.id})");
                log.AddRange(await this.SellMarketContainerContents(market.id).ConfigureAwait(false));
                logMessage($@"Market {market.name} ({market.id}) Complete!");
            }

            return log;
        }

        public async Task<List<string>> SellMarketContainerContents(double marketId)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            try
            {
                var walletsTransaction = await this._dualWalletRepository.GetAllBotTransactionOnMarket(marketId).ConfigureAwait(false);
                logMessage($@"Found {walletsTransaction.Count()} wallet transactions for market {marketId}");

                var groupByItemId = walletsTransaction.Where(item => this._dualClient.ResellItems.Contains(Convert.ToUInt64(item.item_id))).GroupBy(item => item.item_id).Select(item => new Tuple<double, double, double>(item.Key, item.Sum(c => c.quantity), item.Sum(c => c.amount)));

                logMessage($@"Grouped like items together and got {groupByItemId.Count()} results!");

                logMessage($@"Fetching Container Contents for market {marketId} with these items {System.Text.Json.JsonSerializer.Serialize(this._dualClient.ResellItems)}!");

                var containerContents = await this._dualClient.GetMarketContainerContents(Convert.ToUInt64(marketId)).ConfigureAwait(false);

                logMessage($@"Container has {containerContents.slots.Count()}!");
                logMessage($@"Container contents {System.Text.Json.JsonSerializer.Serialize(containerContents.slots)}!");

                Dictionary<ulong, long> prices = new Dictionary<ulong, long>();

                foreach (var item in groupByItemId)
                {
                    var itemId = Convert.ToUInt64(item.Item1);
                    var totalSpent = Math.Abs(item.Item3) / 100;
                    var totalBought = item.Item2;
                    var avgPer = (long)(totalSpent / totalBought);

                    logMessage($@"Processing has '{itemId}' ({totalSpent}/{totalBought}) with avg price '{avgPer}'!");

                    prices.Add(itemId, avgPer);
                }

                foreach (var slot in containerContents.slots)
                {
                    var itemId = slot.itemAndQuantity.item.type;

                    if (!slot.purchased)
                    {
                        continue;
                    }

                    if (!this._dualClient.ResellItems.Contains(itemId))
                    {
                        continue;
                    }

                    if (!prices.ContainsKey(itemId))
                    {
                        //skip this item
                        continue;
                    }

                    var marketQty = slot.itemAndQuantity.quantity.value;
                    var avgPer = prices[itemId] * this._marketBotConfig.MarketMarkUp;

                    if (avgPer < this._dualClient.BuyPrices[itemId])
                    {
                        avgPer = (this._dualClient.BuyPrices[itemId] / 100) * this._marketBotConfig.MarketMarkUp;
                    }

                    logMessage($@"Selling item  {itemId} @ {avgPer}, quantity {marketQty}!");
                    log.AddRange(await this._dualClient.SellStuff(Convert.ToUInt64(marketId), itemId, Convert.ToInt64(avgPer), marketQty).ConfigureAwait(false));
                }
            }
            catch (Exception exception)
            {
                log.Add(@$"Error: {exception}");
            }

            return log;
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

        public async Task<IEnumerable<MarketTransaction>> GetAllActiveAsync(double marketId, double itemId)
        {
            var results = await this._dualMarketTransactionRepository.GetAllActiveAsync(marketId, itemId).ConfigureAwait(false);

            return results.Select(item => item.ToMarketTransaction());
        }
    }
}
