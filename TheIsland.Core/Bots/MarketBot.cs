// <copyright file="MarketBot.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Bots
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Backend;
    using BotLib.Generated;
    using BotLib.Utils;
    using Microsoft.Extensions.Logging;
    using NQ;
    using TheIsland.Core.Settings;

    public interface IMarketBot : IBotClient
    {
        ConcurrentDictionary<ulong, double> BuyPrices { get; }

        ConcurrentBag<ulong> ResellItems { get; }

        ConcurrentDictionary<ulong, double> MarketBudgetMultiplier { get; }

        Task SaveDictionaries();

        Task LoadDictionaries();

        Task<MarketStorageInfoEx> GetMarketContainerContents(ulong marketId);

        Task<List<string>> BuyStuff(ulong planetId);

        Task<List<string>> SellStuff(ulong marketId, ulong itemType, long unitPrice, long quantity);

        Task CancelBotOrders(ulong marketId);

        long GetItemPrice(ulong marketId, ulong itemType);

        long GetSellPrice(ulong marketId, ulong itemType, decimal inputPrice = 0);

        void SetItemMultiplier(ulong marketId, string itemType, double value);

        void SetItemMultiplierRecursive(ulong marketId, string itemType, double value);

        void SetItemSellMultiplier(ulong marketId, string itemType, double value);

        void SetItemSellMultiplierRecursive(ulong marketId, string itemType, double value);
    }

    public class MarketBot : BotClient, IMarketBot
    {
        public ConcurrentDictionary<ulong, double> BuyPrices { get; private set; } = new ConcurrentDictionary<ulong, double>();

        public ConcurrentDictionary<ulong, double> MarketBudgetMultiplier { get; private set; } = new ConcurrentDictionary<ulong, double>();

        internal ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> MarketItemMultiplier { get; private set; } = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>>();

        internal ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> MarketItemSellMultiplier { get; private set; } = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>>();

        public ConcurrentBag<ulong> ResellItems { get; private set; } = new ConcurrentBag<ulong>();

        private readonly MarketBotConfig _marketBotConfig;

        private readonly DualUniverseSettings _settings;

        public MarketBot(MarketBotConfig marketBotConfig, DualUniverseSettings settings, ILogger<IMarketBot> logger) : base(settings.MarketBot, logger)
        {
            this._marketBotConfig = marketBotConfig;
            this._settings = settings;
            this.InitializeItemPurchasing();
            this.LoadDictionaries().GetAwaiter().GetResult();
        }

        public async Task<List<string>> BuyStuff(ulong parent)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            logMessage("Pinging Server (confirming connection)");
            await this.BotConnectionTest().ConfigureAwait(false);

            logMessage("Good to start");
            List<ulong> itemTypes = this.BuyPrices.Keys.ToList();

            logMessage(@$"Items to Buy ({itemTypes.Count()})");

            if (itemTypes.Count() == 0)
            {
                logMessage(@$"Reinit BuyPrices");
                this.InitializeItemPurchasing();
                itemTypes = this.BuyPrices.Keys.ToList();
                logMessage(@$"Items to Buy ({itemTypes.Count()})");
            }

            // get all markets on alioth (ALioths ID (Construct) is 2)
            MarketList ml = await this.Bot.Req.MarketGetList(parent).ConfigureAwait(false);

            logMessage(@$"Found Markets ({ml.markets.Count()})");

            // loop over each market
            foreach (MarketInfo? mkt in ml.markets)
            {
                logMessage(@$"Starting Market '{mkt.name}'");

                List<ulong> doneIds = new List<ulong>();
                int itemsPurchase = 0;

                // get my orders for this market
                MarketOrders orders = await this.Bot.Req.MarketSelectItem(
                    new MarketSelectRequest
                    {
                        marketIds = new List<ulong> { mkt.marketId },
                        itemTypes = itemTypes,
                    }).ConfigureAwait(false);

                logMessage(@$"Found orders ({orders.orders.Count()})");

                // loop over all my orders in this market
                foreach (MarketOrder? order in orders.orders)
                {
                    if (order.ownerName == "Bot" || order.ownerName == this.Settings.PlayerName)
                    {
                        // most likely a seeded order. skip
                        continue;
                    }

                    //gets the difference in time between now and the expiration date of the order.
                    TimeSpan diff = order.expirationDate.ToDateTime() - DateTime.UtcNow;

                    if (diff.TotalDays > this._marketBotConfig.DaysToWaitBeforeExpiration)
                    {
                        // logMessage(@$"Skipping order ({order.orderId}) because {diff.TotalDays} days remaining!");
                        // over 24hrs remaining, let others try to buy first.
                        continue;
                    }

                    double budget = this.GetItemPrice(mkt.marketId, order.itemType);

                    logMessage(@$"Using Budget {budget} on {order.itemType} ({order.orderId}@{order.unitPrice})");

                    if (order.unitPrice > budget)
                    {
                        logMessage(@$"Skipping order ({order.orderId}) because over priced!");

                        // over priced, don't touch;
                        continue;
                    }

                    Currency wallet = await this.Bot.Req.GetWallet().ConfigureAwait(false);

                    if (wallet != null)
                    {
                        logMessage(@$"Wallet has {wallet.amount} and need {Math.Abs(order.buyQuantity) * order.unitPrice}!");
                    }

                    MarketOrders boughtItems = await this.Bot.Req.MarketInstantOrder(
                        new MarketRequest
                        {
                            marketId = order.marketId,
                            itemType = order.itemType,
                            buyQuantity = Math.Abs(order.buyQuantity),
                            unitPrice = order.unitPrice,
                        }).ConfigureAwait(false);
                    itemsPurchase++;
                }

                logMessage(@$"Purchased {itemsPurchase} orders @ '{mkt.name}'!");
            }

            return log;
        }

        public async Task<MarketStorageInfoEx> GetMarketContainerContents(ulong marketId)
        {
            await this.BotConnectionTest().ConfigureAwait(false);

            return await this.Bot.Req.MarketContainerGetMyContent(new MarketSelectRequest
            {
                marketIds = new List<ulong> { marketId },
                itemTypes = new List<ulong> { this.Bot.GameplayBank.GetDefinition<NQutils.Def.BaseItem>().Id },
            }).ConfigureAwait(false);
        }

        public async Task CancelBotOrders(ulong marketId)
        {
            await this.BotConnectionTest().ConfigureAwait(false);

            MarketOrders orders = await this.Bot.Req.MarketGetMyOrders(
                new MarketSelectRequest
                {
                    marketIds = new List<ulong> { marketId },
                    ownerId = this.Bot.AsPlayerId(),
                }).ConfigureAwait(false);

            foreach (MarketOrder? order in orders.orders)
            {
                await this.Bot.Req.MarketCancelOrder(order).ConfigureAwait(false);
            }
        }

        public async Task<List<string>> SellStuff(ulong marketId, ulong itemType, long unitPrice, long quantity)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            logMessage("Pinging Server (confirming connection)");
            await this.BotConnectionTest().ConfigureAwait(false);

            logMessage(@$"Selling {itemType} @ {marketId} for this {unitPrice / 100}");

            MarketOrder order = await this.Bot.Req.MarketPlaceOrder(new MarketRequest
            {
                marketId = marketId,
                source = MarketRequestSource.FROM_MARKET_CONTAINER,
                itemType = itemType,
                buyQuantity = -quantity,
                expirationDate = DateTime.Now.AddDays(30).ToNQTimePoint(),
                unitPrice = unitPrice,
            }).ConfigureAwait(false);

            logMessage(@$"Listed {order.itemType} @ {order.marketId} for this {order.unitPrice / 100} ({order.buyQuantity})");
            return log;
        }

        public long GetItemPrice(ulong marketId, ulong itemType)
        {
            if (!this.BuyPrices.TryGetValue(Convert.ToUInt64(itemType), out double botPurchasePrice))
            {
                botPurchasePrice = 0;
            }
            else
            {
                botPurchasePrice /= 100;
            }

            double finalMarketItemMultiplier = 0;
            if (this.MarketItemMultiplier.ContainsKey(marketId))
            {
                ConcurrentDictionary<ulong, double> marketItemMultiplierDictionary = this.MarketItemMultiplier[marketId];

                marketItemMultiplierDictionary.TryGetValue(itemType, out finalMarketItemMultiplier);
            }

            if (finalMarketItemMultiplier == 0)
            {
                if (this.MarketItemMultiplier.TryGetValue(0, out ConcurrentDictionary<ulong, double>? marketData))
                {
                    if (!marketData.TryGetValue(itemType, out finalMarketItemMultiplier))
                    {
                        finalMarketItemMultiplier = 1;
                    }
                }
                else
                {
                    finalMarketItemMultiplier = 1;
                }
            }

            if (this.MarketBudgetMultiplier.TryGetValue(marketId, out double marketMultiplier))
            {
                botPurchasePrice *= marketMultiplier;
            }

            return (long)Math.Ceiling(botPurchasePrice) * 100;
        }

        public long GetSellPrice(ulong marketId, ulong itemType, decimal inputPrice = 0)
        {
            inputPrice = inputPrice != 0 ? inputPrice : this.GetItemPrice(marketId, itemType) / 100;

            double finalMarketItemMultiplier = 0;
            if (this.MarketItemSellMultiplier.ContainsKey(marketId))
            {
                ConcurrentDictionary<ulong, double> marketItemMultiplierDictionary = this.MarketItemMultiplier[marketId];

                marketItemMultiplierDictionary.TryGetValue(itemType, out finalMarketItemMultiplier);
            }

            if (finalMarketItemMultiplier == 0)
            {
                if (this.MarketItemSellMultiplier.TryGetValue(0, out ConcurrentDictionary<ulong, double>? marketData))
                {
                    if (!marketData.TryGetValue(itemType, out finalMarketItemMultiplier))
                    {
                        finalMarketItemMultiplier = 1;
                    }
                }
                else
                {
                    finalMarketItemMultiplier = 1;
                }
            }

            return (long)Math.Ceiling((inputPrice * (decimal)finalMarketItemMultiplier) * (decimal)this._marketBotConfig.MarketMarkUp) * 100;
        }

        public void SetItemMultiplier(ulong marketId, string itemType, double value)
        {
            value = Math.Clamp(value, .1, 10);
            this.SetDictionaryMultiplier(marketId, itemType, value, this.MarketItemMultiplier);
            this.SaveDictionaries().GetAwaiter().GetResult();
        }

        public void SetItemSellMultiplier(ulong marketId, string itemType, double value)
        {
            value = Math.Clamp(value, 1, 10);
            this.SetDictionaryMultiplier(marketId, itemType, value, this.MarketItemSellMultiplier);
            this.SaveDictionaries().GetAwaiter().GetResult();
        }

        public void SetItemMultiplierRecursive(ulong marketId, string itemType, double value)
        {
            value = Math.Clamp(value, .1, 10);
            this.SetDictionaryMultiplierRecursive(marketId, itemType, value, this.MarketItemMultiplier);
            this.SaveDictionaries().GetAwaiter().GetResult();
        }

        public void SetItemSellMultiplierRecursive(ulong marketId, string itemType, double value)
        {
            value = Math.Clamp(value, 1, 10);
            this.SetDictionaryMultiplierRecursive(marketId, itemType, value, this.MarketItemSellMultiplier);
            this.SaveDictionaries().GetAwaiter().GetResult();
        }

        private void SetDictionaryMultiplier(ulong marketId, string itemType, double value, ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> inputDictionary)
        {
            IGameplayBank bank = this.Bot.GameplayBank;
            IGameplayDefinition? entry = bank.GetDefinition(itemType);

            if (entry == null)
            {
                return;
            }

            if (entry.GetChildren().Any())
            {
                // most likely a category, just continue
                return;
            }

            inputDictionary
                .GetOrAdd(marketId, (key) => new ConcurrentDictionary<ulong, double>())
                .AddOrUpdate(entry.Id, value, (key, oldValue) => value);
        }

        private void SetDictionaryMultiplierRecursive(ulong marketId, string itemType, double value, ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> inputDictionary)
        {
            IGameplayBank bank = this.Bot.GameplayBank;
            IGameplayDefinition? baseEntry = bank.GetDefinition(itemType);

            if (baseEntry == null)
            {
                return;
            }

            IEnumerable<ulong> childrenIds = baseEntry.GetChildrenIdsRecursive();
            foreach (ulong childId in childrenIds)
            {
                IGameplayDefinition? entry = bank.GetDefinition(childId);

                if (entry == null)
                {
                    continue;
                }

                if (entry.GetChildren().Any())
                {
                    // most likely a category, just continue
                    continue;
                }

                inputDictionary
                    .GetOrAdd(marketId, (key) => new ConcurrentDictionary<ulong, double>())
                    .AddOrUpdate(childId, value, (key, oldValue) => value);
            }
        }

        public async Task SaveDictionaries()
        {
            void ToDictionary(ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> input, Dictionary<ulong, Dictionary<ulong, double>> output)
            {
                foreach (KeyValuePair<ulong, ConcurrentDictionary<ulong, double>> entry in input)
                {
                    output.Add(entry.Key, new Dictionary<ulong, double>(entry.Value.ToArray()));
                }
            }

            try
            {
                Dictionary<ulong, Dictionary<ulong, double>> item = new Dictionary<ulong, Dictionary<ulong, double>>();
                Dictionary<ulong, Dictionary<ulong, double>> itemSell = new Dictionary<ulong, Dictionary<ulong, double>>();

                Dictionary<ulong, double> market = new Dictionary<ulong, double>(this.MarketBudgetMultiplier.ToArray());
                ToDictionary(this.MarketItemMultiplier, item);
                ToDictionary(this.MarketItemSellMultiplier, itemSell);

                await File.WriteAllTextAsync(@$"{this._settings.ConfigPath}/marketBudgetMultiplier.json", System.Text.Json.JsonSerializer.Serialize(item)).ConfigureAwait(false);
                await File.WriteAllTextAsync(@$"{this._settings.ConfigPath}/marketItemMultiplier.json", System.Text.Json.JsonSerializer.Serialize(item)).ConfigureAwait(false);
                await File.WriteAllTextAsync(@$"{this._settings.ConfigPath}/marketItemSellMultiplier.json", System.Text.Json.JsonSerializer.Serialize(itemSell)).ConfigureAwait(false);
                return;
            }
            catch
            {
                return;
            }
        }

        public async Task LoadDictionaries()
        {
            void ToDictionary(Dictionary<ulong, Dictionary<ulong, double>> input, ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> output)
            {
                output.Clear();

                foreach (KeyValuePair<ulong, Dictionary<ulong, double>> entry in input)
                {
                    ConcurrentDictionary<ulong, double> tempDict = new ConcurrentDictionary<ulong, double>();
                    foreach (KeyValuePair<ulong, double> nestedEntry in entry.Value)
                    {
                        tempDict.TryAdd(nestedEntry.Key, nestedEntry.Value);
                    }

                    output.TryAdd(entry.Key, tempDict);
                }
            }

            try
            {
                string textMarketBudgetMultiplier = await File.ReadAllTextAsync(@$"{this._settings.ConfigPath}/marketBudgetMultiplier.json").ConfigureAwait(false);
                string textMarketItemMultiplier = await File.ReadAllTextAsync(@$"{this._settings.ConfigPath}/marketItemMultiplier.json").ConfigureAwait(false);
                string textMarketItemSellMultiplier = await File.ReadAllTextAsync(@$"{this._settings.ConfigPath}/marketItemSellMultiplier.json").ConfigureAwait(false);

                Dictionary<ulong, double>? dictionaryMarketBudgetMultiplier = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, double>>(textMarketBudgetMultiplier);
                Dictionary<ulong, Dictionary<ulong, double>>? dictionaryMarketItemMultiplier = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, Dictionary<ulong, double>>>(textMarketItemMultiplier);
                Dictionary<ulong, Dictionary<ulong, double>>? dictionaryMarketItemSellMultiplier = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, Dictionary<ulong, double>>>(textMarketItemSellMultiplier);

                if (dictionaryMarketBudgetMultiplier != null)
                {
                    this.MarketBudgetMultiplier.Clear();

                    foreach (KeyValuePair<ulong, double> entry in dictionaryMarketBudgetMultiplier)
                    {
                        this.MarketBudgetMultiplier.TryAdd(entry.Key, entry.Value);
                    }
                }

                if (dictionaryMarketItemMultiplier != null)
                {
                    ToDictionary(dictionaryMarketItemMultiplier, this.MarketItemMultiplier);
                }

                if (dictionaryMarketItemSellMultiplier != null)
                {
                    ToDictionary(dictionaryMarketItemSellMultiplier, this.MarketItemSellMultiplier);
                }
            }
            catch
            {
                return;
            }
        }

        private void InitializeItemPurchasing()
        {
            IGameplayBank bank = this.Bot.GameplayBank;

            foreach ((string key, double value) in this._marketBotConfig.BuyPrices)
            {
                IGameplayDefinition? entry = bank.GetDefinition(key);

                if (entry == null)
                {
                    continue;
                }

                if (entry.GetChildren().Any())
                {
                    // most likely a category, just continue
                    continue;
                }

                this.BuyPrices.TryAdd(entry.Id, value * 100);
            }

            // iterate over recursive prices
            foreach ((string key, double value) in this._marketBotConfig.BuyRecursivePrices)
            {
                IGameplayDefinition? baseEntry = bank.GetDefinition(key);

                if (baseEntry == null)
                {
                    continue;
                }

                IEnumerable<ulong> childrenIds = baseEntry.GetChildrenIdsRecursive();
                foreach (ulong childId in childrenIds)
                {
                    IGameplayDefinition? entry = bank.GetDefinition(childId);

                    if (entry == null)
                    {
                        continue;
                    }

                    if (entry.GetChildren().Any())
                    {
                        // most likely a category, just continue
                        continue;
                    }

                    this.BuyPrices.TryAdd(childId, value * 100);
                }
            }

            // iterate over recursive prices
            foreach (string key in this._marketBotConfig.OnlyResellItemsRecursive)
            {
                IGameplayDefinition? baseEntry = bank.GetDefinition(key);

                if (baseEntry == null)
                {
                    continue;
                }

                IEnumerable<ulong> childrenIds = baseEntry.GetChildrenIdsRecursive();
                foreach (ulong childId in childrenIds)
                {
                    IGameplayDefinition? entry = bank.GetDefinition(childId);

                    if (entry == null)
                    {
                        continue;
                    }

                    if (entry.GetChildren().Any())
                    {
                        // most likely a category, just continue
                        continue;
                    }

                    this.ResellItems.Add(childId);
                }
            }
        }
    }
}