// <copyright file="MarketBot.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Bots
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Backend;
    using BotLib.Generated;
    using BotLib.Utils;
    using Microsoft.Extensions.Logging;
    using NQ;
    using NQ.Interfaces;
    using StackExchange.Redis;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Helpers;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Settings;

    public interface IMarketBot : IBotClient
    {
        ConcurrentDictionary<ulong, double> BuyPrices { get; }

        ConcurrentDictionary<ulong, double> MarketBudgetMultiplier { get; }

        ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> MarketItemMultiplier { get;  }

        ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> MarketItemSellMultiplier { get;  }

        ConcurrentBag<ulong> ResellItems { get; }

        #region General Functions
        List<ItemEntry> GetMarketHierarchy(bool forceRefresh = false);

        Dictionary<double, string> GetListOfSellableItems();

        List<KeyValuePair<string, double>> GetAllItems();

        List<ItemEntry> GetAllItemsMarketEntries();

        object GetConfigs();

        Task SaveDictionariesAsync();

        Task LoadDictionariesAsync();
        #endregion

        Task<MarketStorageInfoEx> GetMarketContainerContentsAsync(ulong marketId);

        Task<List<string>> BuyStuffAsync(ulong planetId);

        Task<List<string>> SellStuffAsync(ulong marketId, ulong itemType, long unitPrice, long quantity);

        Task CancelBotOrdersAsync(ulong marketId);

        Task<double> GetItemLimitAsync(double itemId);

        Task<PlayerMarketBuyLimit?> GetPlayerItemLimitAsync(double playerId, double marketId, double itemId);

        long GetItemPrice(ulong marketId, ulong itemType);

        long GetSellPrice(ulong marketId, ulong itemType, decimal inputPrice = 0);

        Task SetItemMultipliersAsync(ulong marketId, string itemType, double value);

        Task SetItemMultiplierRecursiveAsync(ulong marketId, string itemType, double value);

        Task SetItemSellMultiplierAsync(ulong marketId, string itemType, double value);

        Task SetItemSellMultiplierRecursiveAsync(ulong marketId, string itemType, double value);
    }

    public class MarketBot : BotClient, IMarketBot
    {
        public ConcurrentDictionary<ulong, double> BuyPrices { get; private set; } = new ConcurrentDictionary<ulong, double>();

        public ConcurrentDictionary<ulong, double> MarketBudgetMultiplier { get; private set; } = new ConcurrentDictionary<ulong, double>();

        public ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> MarketItemMultiplier { get; private set; } = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>>();

        public ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> MarketItemSellMultiplier { get; private set; } = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>>();

        public ConcurrentBag<ulong> ResellItems { get; private set; } = new ConcurrentBag<ulong>();

        private List<ItemEntry> _marketEntries { get; set; } = new List<ItemEntry>();

        private ConcurrentDictionary<double, string> ItemsForSale { get; set; } = new ConcurrentDictionary<double, string>();

        private ConcurrentBag<ItemEntry> ItemsForSaleME { get; set; } = new ConcurrentBag<ItemEntry>();

        private readonly MarketBotConfig _marketBotConfig;
        private readonly DualUniverseSettings _settings;
        private readonly DualPlayerRepository _dualPlayerRepository;
        private readonly MarketBuyLimitRepository _marketBuyLimitRepository;
        private readonly PlayerMarketBuyLimitRepository _playerMarketBuyLimitRepository;
        private readonly FactoryLedgerRepository _factoryLedgerRepository;
        private readonly DualMarketItemEntryRepository _dualMarketItemEntryRepository;

        public MarketBot(
            DualPlayerRepository dualPlayerRepository,
            MarketBuyLimitRepository marketBuyLimitRepository,
            PlayerMarketBuyLimitRepository playerMarketBuyLimitRepository,
            FactoryLedgerRepository factoryLedgerRepository,
            DualMarketItemEntryRepository dualMarketItemEntryRepository,
            MarketBotConfig marketBotConfig,
            DualUniverseSettings settings,
            ILogger<IMarketBot> logger) : base(settings.MarketBot, logger)
        {
            this._dualPlayerRepository = dualPlayerRepository;
            this._marketBuyLimitRepository = marketBuyLimitRepository;
            this._playerMarketBuyLimitRepository = playerMarketBuyLimitRepository;
            this._factoryLedgerRepository = factoryLedgerRepository;
            this._dualMarketItemEntryRepository = dualMarketItemEntryRepository;

            this._marketBotConfig = marketBotConfig;
            this._settings = settings;
            this.InitializeItemPurchasing();
            this.LoadDictionariesAsync().GetAwaiter().GetResult();
        }

        public override Task BotConnectionTestAsync()
        {
            return ThreadSafeExecution.ThreadExecution(nameof(MarketBot), this.InternalBotConnectionTestAsync);
        }

        #region General Functions
        public List<ItemEntry> GetAllItemsMarketEntries()
        {
            if (this.ItemsForSaleME.Count == 0)
            {
                this.GetMarketHierarchy(true);
            }

            return this.ItemsForSaleME.ToList();
        }

        public List<ItemEntry> GetMarketHierarchy(bool forceRefresh = false)
        {
            if (this._marketEntries.Count != 0 && !forceRefresh)
            {
                return new List<ItemEntry>();
            }

            List<ItemEntry> output = new List<ItemEntry>();

            IGameplayBank bank = this.Bot.GameplayBank;

            foreach (ulong headerId in this._settings.MarketHeaderIds)
            {
                IGameplayDefinition? baseEntry = bank.GetDefinition(headerId);

                if (baseEntry == null)
                {
                    continue;
                }

                ItemEntry marketEntry = new ItemEntry()
                {
                    ParentId = 0,
                    ParentName = string.Empty,
                    Id = headerId,
                    Name = baseEntry.Name,
                    DisplayName = baseEntry.LocalizedProperties.FirstOrDefault(item => item.Name == "displayName").Translation.ToString() ?? string.Empty,
                };

                IEnumerable<IGameplayDefinition> childrenObjects = baseEntry.GetChildren();

                if (childrenObjects != null && childrenObjects.Any())
                {
                    ulong[] childrenIds = childrenObjects.Select(item => item.Id).ToArray();

                    marketEntry.Children = this.GetChildren(childrenIds, bank);
                }

                output.Add(marketEntry);
            }

            this._marketEntries = new List<ItemEntry>(output);
            return output;
        }

        public List<KeyValuePair<string, double>> GetAllItems()
        {
            var allItems = this.Bot.GameplayBank.GetDefinitions();
            return allItems.Select(item => new KeyValuePair<string, double>(item.Name, item.Id)).ToList();
        }

        /// <summary>
        /// Gets a list of all sellible items on the market.
        /// </summary>
        /// <returns></returns>
        public Dictionary<double, string> GetListOfSellableItems()
        {
            if (this.ItemsForSale.Count == 0)
            {
                this.GetMarketHierarchy();
            }

            return this.ItemsForSale.ToDictionary();
        }
        #endregion

        public async Task<List<string>> BuyStuffAsync(ulong parent)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            var timeframe = PlayerMarketBuyLimitRepository.GetTimeframe();

            logMessage("Pinging Server (confirming connection)");
            await this.BotConnectionTestAsync().ConfigureAwait(false);
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
            MarketList marketList = await this.Bot.Req.MarketGetList(parent).ConfigureAwait(false);

            logMessage(@$"Found Markets ({marketList.markets.Count()})");

            // loop over each market
            foreach (MarketInfo? market in marketList.markets)
            {
                logMessage(@$"Starting Market '{market.name}'");

                var marketId = Convert.ToDouble(market.marketId);

                List<ulong> doneIds = new List<ulong>();
                int itemsPurchase = 0;

                // get my orders for this market
                MarketOrders orders = await this.Bot.Req.MarketSelectItem(
                    new MarketSelectRequest
                    {
                        marketIds = new List<ulong> { market.marketId },
                        itemTypes = itemTypes,
                    }).ConfigureAwait(false);

                logMessage(@$"Found orders ({orders.orders.Count()})");

                // loop over all my orders in this market
                foreach (MarketOrder? order in orders.orders)
                {
                    if (order.ownerId.IsOrg())
                    {
                        // we don't buy from orgs.
                        continue;
                    }

                    var playerId = Convert.ToDouble(order.ownerId.playerId);
                    var itemId = Convert.ToDouble(order.itemType);
                    ItemEntry itemData = this.GetAllItemsMarketEntries().First(item => item.Id == itemId);
                    double marketLimit = await this.GetItemLimitAsync(itemId).ConfigureAwait(false);

                    logMessage(@$"This item ({itemData.Name}) limit is {marketLimit}!");
                    if (await this._dualPlayerRepository.IsBotByPlayerId(playerId).ConfigureAwait(false))
                    {
                        // we don't purchase from bots
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

                    PlayerMarketBuyLimit? getPlayersLimit = await this.GetPlayerItemLimitAsync(playerId, marketId, itemId).ConfigureAwait(false);

                    if (getPlayersLimit == null)
                    {
                        throw new Exception("Somehow market or player is a negative number. you broke something bad.");
                    }

                    if (getPlayersLimit.quantity >= marketLimit)
                    {
                        //skipping this, player over purchase limit.
                        logMessage(@$"Skipping order ({order.orderId}) because {getPlayersLimit.quantity} greater then limit({marketLimit})!");
                        continue;
                    }

                    double budget = this.GetItemPrice(market.marketId, order.itemType);

                    logMessage(@$"Using Budget {budget} on {order.itemType} ({order.orderId}@{order.unitPrice})");

                    if (order.unitPrice > budget)
                    {
                        logMessage(@$"Skipping order ({order.orderId}) because over priced!");

                        // over priced, don't touch;
                        continue;
                    }

                    Currency wallet = await this.Bot.Req.GetWallet().ConfigureAwait(false);

                    long buyQuantity = Math.Abs(order.buyQuantity);
                    long allowedQuantity = Convert.ToInt64(marketLimit - getPlayersLimit.quantity);

                    if (buyQuantity > allowedQuantity)
                    {
                        logMessage(@$"More items then limit {buyQuantity} > {allowedQuantity}!");

                        //more avail then limit.
                        buyQuantity = allowedQuantity;
                    }

                    long totalCost = buyQuantity * order.unitPrice;

                    if (wallet != null && wallet.amount > totalCost)
                    {
                        logMessage(@$"Wallet has {wallet.amount} and need {totalCost}!");
                    }

                    MarketOrders boughtItems = await this.Bot.Req.MarketInstantOrder(
                        new MarketRequest
                        {
                            itemOwner = order.ownerId,
                            marketId = order.marketId,
                            itemType = order.itemType,
                            buyQuantity = buyQuantity,
                            unitPrice = order.unitPrice,
                        }).ConfigureAwait(false);
                    itemsPurchase++;
                    getPlayersLimit.quantity += buyQuantity;

                    if (getPlayersLimit.id == 0)
                    {
                        await this._playerMarketBuyLimitRepository.AddAsync(getPlayersLimit).ConfigureAwait(false);
                    }
                    else
                    {
                        await this._playerMarketBuyLimitRepository.UpdateAsync(getPlayersLimit).ConfigureAwait(false);
                    }

                    await this._factoryLedgerRepository.AddAsync(new FactoryLedgerEntry
                    {
                        item_id = Convert.ToDouble(order.itemType),
                        quantity = buyQuantity,
                        price = totalCost,
                    }).ConfigureAwait(false);

                    var marketBox = await this._dualMarketItemEntryRepository.GetByPlayerIdByMarketIdAsync(this.Bot.PlayerId.id, order.marketId).ConfigureAwait(false);
                    var marketBoxID = marketBox.FirstOrDefault(item => item.item_type_id == order.itemType)?.id ?? 0;

                    //delete the ore
                    await this._dualMarketItemEntryRepository.RemoveAsync(marketBoxID).ConfigureAwait(false);
                }

                logMessage(@$"Purchased {itemsPurchase} orders @ '{market.name}'!");
            }

            return log;
        }

        public async Task<MarketStorageInfoEx> GetMarketContainerContentsAsync(ulong marketId)
        {
            await this.BotConnectionTestAsync().ConfigureAwait(false);

            return await this.Bot.Req.MarketContainerGetMyContent(new MarketSelectRequest
            {
                marketIds = new List<ulong> { marketId },
                itemTypes = new List<ulong> { this.Bot.GameplayBank.GetDefinition<NQutils.Def.BaseItem>().Id },
            }).ConfigureAwait(false);
        }

        public async Task CancelBotOrdersAsync(ulong marketId)
        {
            await this.BotConnectionTestAsync().ConfigureAwait(false);

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

        public async Task<List<string>> SellStuffAsync(ulong marketId, ulong itemType, long unitPrice, long quantity)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            logMessage("Pinging Server (confirming connection)");
            await this.BotConnectionTestAsync().ConfigureAwait(false);

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

        public async Task<double> GetItemLimitAsync(double itemId)
        {
            List<MarketBuyLimit> marketLimits = (await this._marketBuyLimitRepository.GetAsync().ConfigureAwait(false)).OrderBy(item => item.id).ToList();
            ItemEntry? itemData = this.GetAllItemsMarketEntries().FirstOrDefault(item => item.Id == itemId);

            if (itemData == null)
            {
                return 0;
            }

            return marketLimits.FirstOrDefault(item => item.filter == itemData.Name || item.filter == itemData.ParentName || item.filter == itemData.GrandParentName)?.quantity ?? 10000;
        }

        public async Task<PlayerMarketBuyLimit?> GetPlayerItemLimitAsync(double playerId, double marketId, double itemId)
        {
            if (playerId == -1 || marketId == -1)
            {
                return null;
            }

            return
                await this._playerMarketBuyLimitRepository.GetByPlayerIdAsync(playerId, marketId, itemId).ConfigureAwait(false)
                ?? new PlayerMarketBuyLimit
                {
                    item_id = itemId,
                    market_id = marketId,
                    player_id = playerId,
                };
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

            return (long)Math.Ceiling(botPurchasePrice * finalMarketItemMultiplier) * 100;
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

        public Task SetItemMultipliersAsync(ulong marketId, string itemType, double value)
        {
            value = Math.Clamp(value, .1, 10);
            this.SetDictionaryMultiplier(marketId, itemType, value, this.MarketItemMultiplier);
            return this.SaveDictionariesAsync();
        }

        public Task SetItemSellMultiplierAsync(ulong marketId, string itemType, double value)
        {
            value = Math.Clamp(value, 1, 10);
            this.SetDictionaryMultiplier(marketId, itemType, value, this.MarketItemSellMultiplier);
            return this.SaveDictionariesAsync();
        }

        public Task SetItemMultiplierRecursiveAsync(ulong marketId, string itemType, double value)
        {
            value = Math.Clamp(value, .1, 10);
            this.SetDictionaryMultiplierRecursive(marketId, itemType, value, this.MarketItemMultiplier);
            return this.SaveDictionariesAsync();
        }

        public Task SetItemSellMultiplierRecursiveAsync(ulong marketId, string itemType, double value)
        {
            value = Math.Clamp(value, 1, 10);
            this.SetDictionaryMultiplierRecursive(marketId, itemType, value, this.MarketItemSellMultiplier);
            return this.SaveDictionariesAsync();
        }

        public object GetConfigs()
        {
            void ToDictionary(ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, double>> input, Dictionary<ulong, Dictionary<ulong, double>> output)
            {
                foreach (KeyValuePair<ulong, ConcurrentDictionary<ulong, double>> entry in input)
                {
                    output.Add(entry.Key, new Dictionary<ulong, double>(entry.Value.ToArray()));
                }
            }

            Dictionary<ulong, Dictionary<ulong, double>> item = new Dictionary<ulong, Dictionary<ulong, double>>();
            Dictionary<ulong, Dictionary<ulong, double>> itemSell = new Dictionary<ulong, Dictionary<ulong, double>>();
            Dictionary<ulong, double> market = new Dictionary<ulong, double>(this.MarketBudgetMultiplier.ToArray());

            ToDictionary(this.MarketItemMultiplier, item);
            ToDictionary(this.MarketItemSellMultiplier, itemSell);

            return new { MarketBudgetMultiplier = market, MarketItemMultiplier = item, MarketItemSellMultiplier = itemSell };
        }

        public async Task SaveDictionariesAsync()
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

                await File.WriteAllTextAsync(@$"{this._settings.ConfigPath}/marketBudgetMultiplier.json", System.Text.Json.JsonSerializer.Serialize(market)).ConfigureAwait(false);
                await File.WriteAllTextAsync(@$"{this._settings.ConfigPath}/marketItemMultiplier.json", System.Text.Json.JsonSerializer.Serialize(item)).ConfigureAwait(false);
                await File.WriteAllTextAsync(@$"{this._settings.ConfigPath}/marketItemSellMultiplier.json", System.Text.Json.JsonSerializer.Serialize(itemSell)).ConfigureAwait(false);
                return;
            }
            catch
            {
                return;
            }
        }

        public async Task LoadDictionariesAsync()
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

                return;
            }
            catch
            {
                return;
            }
        }

        #region Private Functions
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

        private List<ItemEntry> GetChildren(ulong[] children, IGameplayBank bank)
        {
            List<ItemEntry> output = new List<ItemEntry>();

            foreach (ulong childId in children)
            {
                IGameplayDefinition? baseEntry = bank.GetDefinition(childId);

                if (baseEntry == null)
                {
                    continue;
                }

                string displayName = baseEntry.LocalizedProperties?.FirstOrDefault(item => item.Name == "displayName").Translation?.ToString() ?? string.Empty;
                string parentDisplayName = baseEntry.Parent.LocalizedProperties?.FirstOrDefault(item => item.Name == "displayName").Translation?.ToString() ?? string.Empty;
                string grandparentDisplayName = baseEntry.Parent.Parent.LocalizedProperties?.FirstOrDefault(item => item.Name == "displayName").Translation?.ToString() ?? string.Empty;

                bool hidden = baseEntry.GetStaticPropertyOpt("hidden")?.boolValue ?? false;
                string size = baseEntry.GetStaticPropertyOpt("scale")?.stringValue ?? string.Empty;
                long tier = baseEntry.GetStaticPropertyOpt("level")?.intValue ?? 0;

                if (string.IsNullOrEmpty(displayName) || hidden)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(size))
                {
                    size = $@" {size.ToUpper()}";
                }

                ItemEntry marketEntry = new ItemEntry()
                {
                    GrandParentId = baseEntry.Parent.Parent.Id,
                    GrandParentName = baseEntry.Parent.Parent.Name,
                    Type = grandparentDisplayName,
                    ParentId = baseEntry.Parent.Id,
                    ParentName = baseEntry.Parent.Name,
                    SubType = parentDisplayName,
                    Id = childId,
                    Name = baseEntry.Name,
                    Size = size.Trim(),
                    Tier = tier,
                    DisplayName = $@"{displayName}{size}",
                };

                IEnumerable<IGameplayDefinition> childrenObjects = baseEntry.GetChildren();

                if (childrenObjects != null && childrenObjects.Any())
                {
                    ulong[] childrenIds = childrenObjects.Select(item => item.Id).ToArray();

                    marketEntry.Children = this.GetChildren(childrenIds, bank);
                }

                if (marketEntry.Children.Count() == 0)
                {
                    this.ItemsForSale.TryAdd(marketEntry.Id, marketEntry.DisplayName);
                    this.ItemsForSaleME.Add(marketEntry);
                }

                output.Add(marketEntry);
            }

            return output;
        }
        #endregion
    }
}