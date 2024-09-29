// <copyright file="MarketBot.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Bots
{
    using System.Collections.Concurrent;
    using Backend;
    using BotLib.Generated;
    using BotLib.Utils;
    using Microsoft.Extensions.Logging;
    using NQ;
    using NQ.Interfaces;
    using TheIsland.Core.Settings;

    public interface IMarketBot : IBotClient
    {
        ConcurrentDictionary<ulong, double> BuyPrices { get; }

        ConcurrentDictionary<ulong, double> MarketBudgetMultiplier { get; }

        ConcurrentBag<ulong> ResellItems { get; set; }

        Task LoadMarketBudgetMultiplier();

        Task SaveMarketBudgetMultiplier();

        Task<MarketStorageInfoEx> GetMarketContainerContents(ulong marketId);

        Task<List<string>> BuyStuff(ulong planetId);

        Task<List<string>> SellStuff(ulong marketId, ulong itemType, long unitPrice, long quantity);

        Task CancelBotOrders(ulong marketId);
    }

    public class MarketBot : BotClient, IMarketBot
    {
        public ConcurrentDictionary<ulong, double> BuyPrices { get; private set; } = new ConcurrentDictionary<ulong, double>();

        public ConcurrentDictionary<ulong, double> MarketBudgetMultiplier { get; private set; } = new ConcurrentDictionary<ulong, double>();

        public ConcurrentBag<ulong> ResellItems { get; set; } = new ConcurrentBag<ulong>();

        private readonly MarketBotConfig _marketBotConfig;

        public MarketBot(MarketBotConfig marketBotConfig, DualUniverseSettings settings, ILogger<IMarketBot> logger) : base(settings.MarketBot, logger)
        {
            this._marketBotConfig = marketBotConfig;
            this.InitializeItemPurchasing();
            this.LoadMarketBudgetMultiplier().GetAwaiter().GetResult();
        }

        public Task SaveMarketBudgetMultiplier()
        {
            try
            {
                Dictionary<ulong, double> copy = new Dictionary<ulong, double>(this.MarketBudgetMultiplier.ToArray());
                return File.WriteAllTextAsync("/config/marketBudgetMultiplier.json", System.Text.Json.JsonSerializer.Serialize(copy));
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public async Task LoadMarketBudgetMultiplier()
        {
            try
            {
                string fileText = await File.ReadAllTextAsync("/config/marketBudgetMultiplier.json").ConfigureAwait(false);
                Dictionary<ulong, double>? dictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, double>>(fileText);

                if (dictionary == null)
                {
                    return;
                }

                this.MarketBudgetMultiplier.Clear();

                foreach (KeyValuePair<ulong, double> entry in dictionary)
                {
                    this.MarketBudgetMultiplier.TryAdd(entry.Key, entry.Value);
                }
            }
            catch
            {
                return;
            }
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
                if (!this.MarketBudgetMultiplier.TryGetValue(mkt.marketId, out double marketMultiplier))
                {
                    marketMultiplier = 1;
                }

                logMessage(@$"Using Multiplier x{marketMultiplier}");

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

                    if (this.BuyPrices.TryGetValue(order.itemType, out double budget))
                    {
                        budget *= marketMultiplier;
                        budget = Math.Ceiling(budget);
                    }

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

            logMessage(@$"Selling {itemType} @ {marketId} for this {unitPrice}");

            MarketOrder order = await this.Bot.Req.MarketPlaceOrder(new MarketRequest
            {
                marketId = marketId,
                source = MarketRequestSource.FROM_MARKET_CONTAINER,
                itemType = itemType,
                buyQuantity = -quantity,
                expirationDate = DateTime.Now.AddDays(30).ToNQTimePoint(),
                unitPrice = unitPrice * 100,
            }).ConfigureAwait(false);

            logMessage(@$"Listed {order.itemType} @ {order.marketId} for this {order.unitPrice} ({order.buyQuantity})");
            return log;
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

                if (entry.GetChildren().Count() != 0)
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

                    if (entry.GetChildren().Count() != 0)
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

                    if (entry.GetChildren().Count() != 0)
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