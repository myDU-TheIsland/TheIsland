// <copyright file="MarketService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using StackExchange.Redis;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Helpers;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Settings;
    using static TheIsland.Core.Helpers.RandomHelpers;

    public class MarketService : IAppService
    {
        private readonly DualMarketTransactionRepository _dualMarketTransactionRepository;
        private readonly DualMarketRepository _dualMarketRepository;
        private readonly DualWalletRepository _dualWalletRepository;
        private readonly MarketBotConfig _marketBotConfig;
        private readonly IMarketBot _marketBot;

        public MarketService(
            DualMarketRepository dualMarketRepository,
            DualMarketTransactionRepository dualMarketTransactionRepository,
            DualWalletRepository dualWalletRepository,
            MarketBotConfig marketBotConfig,
            IMarketBot dualClient,
            IDatabase redisDatabase)
        {
            this._dualMarketRepository = dualMarketRepository;
            this._dualMarketTransactionRepository = dualMarketTransactionRepository;
            this._dualWalletRepository = dualWalletRepository;
            this._marketBotConfig = marketBotConfig;
            this._marketBot = dualClient;
        }

        public async Task<List<string>> CancelAllBotOrders()
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            IEnumerable<DualMarket> markets = await this._dualMarketRepository.GetAsync().ConfigureAwait(false);

            foreach (DualMarket market in markets)
            {
                logMessage($@"Processing Market {market.name} ({market.id})");
                await this._marketBot.CancelBotOrdersAsync(Convert.ToUInt64(market.id)).ConfigureAwait(false);
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

            IEnumerable<DualMarket> markets = await this._dualMarketRepository.GetAsync().ConfigureAwait(false);

            foreach (DualMarket market in markets)
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
                IEnumerable<DualWalletTransaction> walletsTransaction = await this._dualWalletRepository.GetAllBotTransactionOnMarket(marketId).ConfigureAwait(false);
                logMessage($@"Found {walletsTransaction.Count()} wallet transactions for market {marketId}");

                IEnumerable<Tuple<double, double, double>> groupByItemId = walletsTransaction
                    .Where(item => this._marketBot.ResellItems.Contains(Convert.ToUInt64(item.item_id)))
                    .GroupBy(item => item.item_id)
                    .Select(item => new Tuple<double, double, double>(item.Key, item.Sum(c => c.quantity), item.Sum(c => c.amount)));

                logMessage($@"Grouped like items together and got {groupByItemId.Count()} results!");

                logMessage($@"Fetching Container Contents for market {marketId} with these items {System.Text.Json.JsonSerializer.Serialize(this._marketBot.ResellItems)}!");

                NQ.MarketStorageInfoEx containerContents = await this._marketBot.GetMarketContainerContentsAsync(Convert.ToUInt64(marketId)).ConfigureAwait(false);

                logMessage($@"Container has {containerContents.slots.Count()}!");
                logMessage($@"Container contents {System.Text.Json.JsonSerializer.Serialize(containerContents.slots)}!");

                Dictionary<ulong, long> prices = new Dictionary<ulong, long>();

                foreach (Tuple<double, double, double>? item in groupByItemId)
                {
                    ulong itemId = Convert.ToUInt64(item.Item1);
                    double totalSpent = Math.Abs(item.Item3) / 100;
                    double totalBought = item.Item2;
                    long avgPer = (long)(totalSpent / totalBought);

                    logMessage($@"Processing has '{itemId}' ({totalSpent}/{totalBought}) with avg price '{avgPer}'!");

                    prices.Add(itemId, avgPer);
                }

                foreach (NQ.MarketStorageSlotEx? slot in containerContents.slots)
                {
                    ulong itemId = slot.itemAndQuantity.item.type;

                    if (!slot.purchased)
                    {
                        continue;
                    }

                    if (!this._marketBot.ResellItems.Contains(itemId))
                    {
                        continue;
                    }

                    if (!prices.ContainsKey(itemId))
                    {
                        //skip this item
                        continue;
                    }

                    long marketQty = slot.itemAndQuantity.quantity.value;
                    double avgPer = this._marketBot.GetSellPrice(Convert.ToUInt64(marketId), itemId, prices[itemId]);

                    if (avgPer < this._marketBot.GetSellPrice(Convert.ToUInt64(marketId), itemId))
                    {
                        avgPer = this._marketBot.GetSellPrice(Convert.ToUInt64(marketId), itemId);
                    }

                    logMessage($@"Selling item  {itemId} @ {avgPer}, quantity {marketQty}!");
                    log.AddRange(await this._marketBot.SellStuffAsync(Convert.ToUInt64(marketId), itemId, Convert.ToInt64(avgPer), marketQty).ConfigureAwait(false));
                }
            }
            catch (Exception exception)
            {
                log.Add(@$"Error: {exception}");
            }

            return log;
        }

        public async Task<IEnumerable<MarketStatistics>> GetDailyStats(double itemId, double marketId = -1)
        {
            IEnumerable<MarketStatistics> results = await this._dualWalletRepository.GetDailyStats(itemId, marketId).ConfigureAwait(false);

            IEnumerable<DualMarket> markets = await this._dualMarketRepository.GetAsync().ConfigureAwait(false);

            foreach (MarketStatistics item in results)
            {
                item.market_name = markets.FirstOrDefault(market => market.id == item.market_id)?.name ?? string.Empty;
            }

            return results;
        }

        public async Task<IEnumerable<MarketTransaction>> GetAllActiveAsync(double marketId, double itemId)
        {
            IEnumerable<DualMarketTransaction> results = await this._dualMarketTransactionRepository.GetAllActiveAsync(marketId, itemId).ConfigureAwait(false);

            return results.Select(item => item.ToMarketTransaction());
        }

        public async Task<List<string>> HotTimeEvent()
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            try
            {
                Random random = new Random();
                Dictionary<double, double> markets = new Dictionary<double, double>();

                foreach (double item in this._marketBotConfig.HotTimeMarkets)
                {
                    markets.Add(item, 1);
                }

                ulong chosenMarket = Convert.ToUInt64(random.Pick(markets));
                logMessage($@"Chose Market : {chosenMarket}");
                double rate = random.Pick(this._marketBotConfig.HotTimeMargins);
                logMessage($@"With Margin : {rate}");

                // get old market id
                KeyValuePair<ulong, double>[] configuredMarkets = this._marketBot.MarketBudgetMultiplier.ToArray();
                Dictionary<ulong, DualMarket> actualMarkets = (await this._dualMarketRepository.GetAsync().ConfigureAwait(false)).ToDictionary(key => Convert.ToUInt64(key.id), value => value);

                logMessage($@"Markets : {JsonSerializer.Serialize(actualMarkets)}");

                log.AddRange(await this.FixHotTimeMarkets().ConfigureAwait(false));

                // disable seeded orders
                await this._dualMarketTransactionRepository.EnableBotSeedOrders(chosenMarket).ConfigureAwait(false);

                ulong constuctId = Convert.ToUInt64(actualMarkets[chosenMarket].construct_id);
                logMessage($@"Construct Id = {constuctId}");

                logMessage($@"Adding new market to MarketBudgetMultiplier");
                this._marketBot.MarketBudgetMultiplier.TryAdd(chosenMarket, rate);

                logMessage($@"Saving Config");
                await this._marketBot.SaveDictionariesAsync().ConfigureAwait(false);

                string name = await this._marketBot.GetConstructNameAsync(constuctId).ConfigureAwait(false);
                logMessage($@"Found '{name}'");

                string newName = $@"[!][{rate}] {name}";
                logMessage($@"Renaming '{name}' to '{newName}'");
                await this._marketBot.SetConstructNameAsync(constuctId, newName).ConfigureAwait(false);
                logMessage($@"New Name '{name}'");
                return log;
            }
            catch (Exception exception)
            {
                logMessage(@$"Error: {exception}");
                return log;
            }
        }

        private async Task<List<string>> FixHotTimeMarkets()
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            List<ulong> configuredMarkets = this._marketBotConfig.HotTimeMarkets.Select(Convert.ToUInt64).ToList();
            Dictionary<ulong, DualMarket> actualMarkets = (await this._dualMarketRepository.GetAsync().ConfigureAwait(false)).ToDictionary(key => Convert.ToUInt64(key.id), value => value);

            foreach (ulong market in configuredMarkets)
            {
                ulong constuctId = Convert.ToUInt64(actualMarkets[market].construct_id);
                string oldMarketName = await this._marketBot.GetConstructNameAsync(constuctId).ConfigureAwait(false) ?? string.Empty;

                if (oldMarketName.StartsWith("[!]"))
                {
                    logMessage($@"Got to rename old market '{oldMarketName}'");
                    this._marketBot.MarketBudgetMultiplier.TryRemove(market, out _);

                    string constructName = oldMarketName;
                    int index = constructName.IndexOf(']', 3);
                    constructName = constructName.Substring(index + 1).Trim();

                    logMessage($@"Renaming '{oldMarketName}' to '{constructName}'");
                    await this._marketBot.SetConstructNameAsync(constuctId, constructName).ConfigureAwait(false);

                    //re-seed market
                    await this._dualMarketTransactionRepository.EnableBotSeedOrders(market).ConfigureAwait(false);
                }
            }

            return log;
        }
    }
}
