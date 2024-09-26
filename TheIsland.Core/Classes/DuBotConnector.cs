// <copyright file="DuBotConnector.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>
namespace TheIsland.Core.Classes
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.Runtime.Internal.Util;
    using Backend;
    using Backend.Business;
    using Backend.Database;
    using BotLib.BotClient;
    using BotLib.Generated;
    using BotLib.Protocols;
    using BotLib.Protocols.Queuing;
    using BotLib.Utils;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Toolkit.HighPerformance;
    using NQ;
    using NQ.Interfaces;
    using NQ.Router;
    using NQutils;
    using NQutils.Sql;
    using Orleans;
    using Serilog;
    using StackExchange.Redis;
    using TheIsland.Core.Helpers;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;
    using static TheIsland.Core.Helpers.MarketTransactionHelpers;
    using static TheIsland.Core.Helpers.ParallelizationHelpers;

    public interface IDUClient
    {
        Client MarketBot { get; }

        Client HelperBot { get; }

        ConcurrentDictionary<ulong, double> BuyPrices { get; }

        ConcurrentDictionary<ulong, double> MarketBudgetMultiplier { get; }

        ConcurrentBag<ulong> ResellItems { get; set; }

        Task SendMessage(ulong who, string message);

        Task<string> ImportBP(ulong playerId, byte[] bp);

        Task<List<string>> BuyStuff(ulong planetId);

        List<MarketEntry> GetMarketHierarchy(bool forceRefresh = false);

        Dictionary<double, string> GetItemsForSale();

        Task<IEnumerable<MarketTransaction>> GetMarketOrders(ulong marketId, ulong itemId);

        MarketBotConfig MarketBotConfig();

        Task<List<string>> GiveAllQuanta(double amount, string note);

        Task<List<string>> GiveTalentPoints(double amount);

        Task<MarketStorageInfoEx> GetMarketContainerContents(ulong marketId);

        Task<List<string>> SellStuff(ulong marketId, ulong itemType, long unitPrice, long quantity);

        Task CancelBotOrders(ulong marketId);
    }

    public class DUClient : IDUClient
    {
        private IDuClientFactory RestDuClientFactory => this.ServiceProvider.GetRequiredService<IDuClientFactory>();

        private IServiceProvider ServiceProvider { get; set; }

        private IClusterClient Orleans { get; set; }

        private IDataAccessor DataAccessor { get; set; }

        private readonly DualUniverseSettings _dualUniverseSettings;
        private readonly MarketBotConfig _marketBotConfig;
        private readonly DualPlayerRepository _dualPlayerRepository;

        private List<MarketEntry> _marketEntries { get; set; } = new List<MarketEntry>();

        public Client HelperBot { get; private set; }

        public Client MarketBot { get; private set; }

        public ConcurrentDictionary<ulong, double> BuyPrices { get; private set; } = new ConcurrentDictionary<ulong, double>();

        public ConcurrentDictionary<ulong, double> MarketBudgetMultiplier { get; private set; } = new ConcurrentDictionary<ulong, double>();

        private ConcurrentDictionary<double, string> ItemsForSale { get; set; } = new ConcurrentDictionary<double, string>();

        public ConcurrentBag<ulong> ResellItems { get; set; } = new ConcurrentBag<ulong>();

        public DUClient(DualUniverseSettings settings, MarketBotConfig marketBotConfig, DualPlayerRepository dualPlayerRepository)
        {
            this._dualPlayerRepository = dualPlayerRepository;
            NQutils.Config.Config.ReadYamlFile("mod", "./dual.yaml");
            this._dualUniverseSettings = settings;
            this._marketBotConfig = marketBotConfig;

            var services = new ServiceCollection();

            //services.RegisterCoreServices();
            var qurl = this._dualUniverseSettings.QueueingURL;

            if (string.IsNullOrEmpty(qurl))
            {
                qurl = "http://queueing:9630";
            }

            services
            .AddSingleton<ISql, Sql>()
            .AddInitializableSingleton<IGameplayBank, GameplayBank>()
            .AddSingleton<ILocalizationManager, LocalizationManager>()
            .AddTransient<IDataAccessor, DataAccessor>()
            .AddOrleansClient("IntegrationTests")
            .AddHttpClient()
            .AddTransient<NQutils.Stats.IStats, NQutils.Stats.FakeIStats>()
            .AddSingleton<IQueuing, RealQueuing>(sp => new RealQueuing(qurl, sp.GetRequiredService<IHttpClientFactory>().CreateClient()))
            .AddSingleton<IDuClientFactory, BotLib.Protocols.GrpcClient.DuClientFactory>();

            var servProvider = services.BuildServiceProvider();
            this.ServiceProvider = servProvider;
            this.ServiceProvider.StartServices().Wait();
            ClientExtensions.SetSingletons(servProvider);
            ClientExtensions.UseFactory(servProvider.GetRequiredService<IDuClientFactory>());
            this.Orleans = this.ServiceProvider.GetRequiredService<IClusterClient>();
            this.DataAccessor = this.ServiceProvider.GetRequiredService<IDataAccessor>();

            this.HelperBot = this.CreateBotUser(settings.WebsiteBot).GetAwaiter().GetResult();
            this.MarketBot = this.CreateBotUser(settings.MarketBot).GetAwaiter().GetResult();

            this.InitializeItemPurchasing();
        }

        public async Task SendMessage(ulong who, string message)
        {
            await this.HelperBotConnectionTest().ConfigureAwait(false);
            await this.HelperBot.Req.ChatMessageSend(new NQ.MessageContent
            {
                channel = new NQ.MessageChannel
                {
                    channel = MessageChannelType.PRIVATE,
                    targetId = who,
                },
                message = message,
            }).ConfigureAwait(false);
        }

        public async Task<List<string>> GiveTalentPoints(double amount)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            try
            {
                var players = await this._dualPlayerRepository.GetAsync().ConfigureAwait(false);

                await this.HelperBotConnectionTest().ConfigureAwait(false);

                foreach (var player in players)
                {
                    if (player.admin || player.is_bot)
                    {
                        logMessage(@$"Skipping user '{player.display_name}', is a bot or admin!");
                        continue;
                    }

                    var response = await this.DataAccessor.PlayerTalentAsync(Convert.ToUInt64(player.id)).ConfigureAwait(false);

                    var currentAvail = response.pointsAcquired - response.pointsSpent;

                    long settingTalentsTo = Convert.ToInt64(currentAvail + amount);

                    logMessage(@$"Setting '{player.display_name}' available talent points to {settingTalentsTo}");

                    await this.DataAccessor.PlayerTalentSetAvailableAsync(Convert.ToUInt64(player.id), settingTalentsTo).ConfigureAwait(false);
                }

                return log;
            }
            catch (Exception exception)
            {
                logMessage(@$"Exception: {exception}!");
                return log;
            }
        }

        public async Task<List<string>> GiveAllQuanta(double amount, string note)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            try
            {
                double giveToPlayer = amount * 100;

                var players = await this._dualPlayerRepository.GetAsync().ConfigureAwait(false);

                await this.HelperBotConnectionTest().ConfigureAwait(false);

                var wallet = await this.MarketBot.Req.GetWallet().ConfigureAwait(false);

                var neededBalance = giveToPlayer * players.Count();

                if (wallet == null)
                {
                    logMessage(@$"Wallet has came back null!");
                    return log;
                }

                if (wallet.amount > neededBalance)
                {
                    logMessage(@$"Wallet has enough to do this transaction.");
                }
                else
                {
                    logMessage(@$"Wallet has {wallet.amount} and needs {neededBalance}!");
                    return log;
                }

                foreach (var player in players)
                {
                    if (player.admin || player.is_bot)
                    {
                        logMessage(@$"Skipping user '{player.display_name}', is a bot or admin!");
                        continue;
                    }

                    string reason = string.IsNullOrEmpty(note) ? "Thank You For Playing" : note;

                    logMessage(@$"Sending Quanta to '{player.display_name}'!");

                    await this.HelperBot.Req.WalletTransferRequest(new WalletTransfer()
                    {
                        amount = Convert.ToUInt64(giveToPlayer),
                        toWallet = new EntityId() { playerId = Convert.ToUInt64(player.id) },
                        fromWallet = new EntityId() { playerId = this.HelperBot.PlayerId },
                        reason = reason,
                    }).ConfigureAwait(false);
                }

                logMessage(@$"GiveAllUsers Complete!");
                return log;
            }
            catch (Exception exception)
            {
                logMessage(@$"Exception: {exception}!");
                return log;
            }
        }

        public async Task<string> ImportBP(ulong playerId, byte[] bp)
        {
            await this.HelperBotConnectionTest().ConfigureAwait(false);
            BlueprintId blueprintId = 0;
            try
            {
                blueprintId = await this.DataAccessor.BlueprintImport(bp, new NQ.EntityId { playerId = playerId }).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                return @$"Failed to import BP. ({exception.ToString()})";
            }

            var blueprintInfo = await this.Orleans.GetBlueprintGrain().GetBlueprintInfo(blueprintId).ConfigureAwait(false);
            var bluepprintModel = await this.ServiceProvider.GetRequiredService<ISql>().Read(blueprintId).ConfigureAwait(false);

            if (bluepprintModel.FreeDeploy && !await this.Orleans.GetPlayerGrain(playerId).IsAdmin().ConfigureAwait(false))
            {
                return "You are not allowed to import free deploy blueprints";
            }

            var pig = this.Orleans.GetInventoryGrain(playerId);
            var blueprintTypeInfo = this.ServiceProvider.GetRequiredService<IGameplayBank>().GetDefinition("Blueprint");

            if (blueprintTypeInfo == null)
            {
                return "System problem. Can't identify blueprint type id";
            }

            var item = new ItemInfo
            {
                type = blueprintTypeInfo.Id,
                id = blueprintId,
            };
            item.properties.Add("name", new PropertyValue { stringValue = blueprintInfo.name });
            item.properties.Add("size", new PropertyValue { intValue = (long)blueprintInfo.size.x });
            item.properties.Add("static", new PropertyValue { boolValue = blueprintInfo.kind != ConstructKind.DYNAMIC });
            item.properties.Add("kind", new PropertyValue { intValue = (int)blueprintInfo.kind });

            await this.DataAccessor.PlayerInventoryGiveAsync(
                    playerId,
                    new ItemAndQuantity
                    {
                        item = item,
                        quantity = 1,
                    }).ConfigureAwait(false);

            return "Blueprint '" + blueprintInfo.name + "' imported and should be in your nano pack.";
        }

        public async Task<List<string>> BuyStuff(ulong parent)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            logMessage("Pinging Server (confirming connection)");
            await this.MarketBotConnectionTest().ConfigureAwait(false);

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
            var ml = await this.MarketBot.Req.MarketGetList(parent).ConfigureAwait(false);

            logMessage(@$"Found Markets ({ml.markets.Count()})");

            // loop over each market
            foreach (var mkt in ml.markets)
            {
                logMessage(@$"Starting Market '{mkt.name}'");
                if (!this.MarketBudgetMultiplier.TryGetValue(mkt.marketId, out double marketMultiplier))
                {
                    marketMultiplier = 1;
                }

                logMessage(@$"Using Multiplier x{marketMultiplier}");

                var doneIds = new List<ulong>();

                // get my orders for this market
                var orders = await this.MarketBot.Req.MarketSelectItem(
                    new MarketSelectRequest
                    {
                        marketIds = new List<ulong> { mkt.marketId },
                        itemTypes = itemTypes,
                    }).ConfigureAwait(false);

                logMessage(@$"Found orders ({orders.orders.Count()})");

                // loop over all my orders in this market
                foreach (var order in orders.orders)
                {
                    if (order.ownerName == "marketbot" || order.ownerName == this._dualUniverseSettings.MarketBot.PlayerName)
                    {
                        // most likely a seeded order. skip
                        continue;
                    }

                    //gets the difference in time between now and the expiration date of the order.
                    var diff = order.expirationDate.ToDateTime() - DateTime.UtcNow;

                    if (diff.TotalDays > this._marketBotConfig.DaysToWaitBeforeExpiration)
                    {
                        logMessage(@$"Skipping order ({order.orderId}) because {diff.TotalDays} days remaining!");

                        // over 24hrs remaining, let others try to buy first.
                        continue;
                    }

                    if (this.BuyPrices.TryGetValue(order.itemType, out double budget))
                    {
                        budget *= marketMultiplier;
                    }

                    logMessage(@$"Using Budget {budget} on {order.itemType} ({order.orderId}@{order.unitPrice})");

                    if (order.unitPrice > budget)
                    {
                        logMessage(@$"Skipping order ({order.orderId}) because over priced!");

                        // over priced, don't touch;
                        continue;
                    }

                    var wallet = await this.MarketBot.Req.GetWallet().ConfigureAwait(false);

                    if (wallet != null)
                    {
                        logMessage(@$"Wallet has {wallet.amount} and need {Math.Abs(order.buyQuantity) * order.unitPrice}!");
                    }

                    var boughtItems = await this.MarketBot.Req.MarketInstantOrder(
                        new MarketRequest
                        {
                            marketId = order.marketId,
                            itemType = order.itemType,
                            buyQuantity = Math.Abs(order.buyQuantity),
                            unitPrice = order.unitPrice,
                        }).ConfigureAwait(false);

                    logMessage(@$"Purchased {boughtItems.orders} orders!");
                }
            }

            return log;
        }

        public async Task<MarketStorageInfoEx> GetMarketContainerContents(ulong marketId)
        {
            await this.MarketBotConnectionTest().ConfigureAwait(false);

            return await this.MarketBot.Req.MarketContainerGetMyContent(new MarketSelectRequest
            {
                marketIds = new List<ulong> { marketId },
                itemTypes = new List<ulong> { this.MarketBot.GameplayBank.GetDefinition<NQutils.Def.BaseItem>().Id },
            }).ConfigureAwait(false);
        }

        public async Task CancelBotOrders(ulong marketId)
        {
            var orders = await this.MarketBot.Req.MarketGetMyOrders(
                new MarketSelectRequest
                {
                    marketIds = new List<ulong> { marketId },
                    ownerId = this.MarketBot.AsPlayerId(),
                }).ConfigureAwait(false);

            foreach (var order in orders.orders)
            {
                await this.MarketBot.Req.MarketCancelOrder(order).ConfigureAwait(false);
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
            await this.MarketBotConnectionTest().ConfigureAwait(false);

            logMessage(@$"Selling {itemType} @ {marketId} for this {unitPrice}");

            var order = await this.MarketBot.Req.MarketPlaceOrder(new MarketRequest
            {
                marketId = marketId,
                source = MarketRequestSource.FROM_MARKET_CONTAINER,
                itemType = itemType,
                buyQuantity = -quantity,
                expirationDate = DateTime.Now.AddDays(3000).ToNQTimePoint(),
                unitPrice = unitPrice * 100,
            }).ConfigureAwait(false);

            logMessage(@$"Listed {order.itemType} @ {order.marketId} for this {order.unitPrice} ({order.buyQuantity})");
            return log;
        }

        /// <summary>
        /// Gets a list of all sellible items on the market.
        /// </summary>
        /// <returns></returns>
        public Dictionary<double, string> GetItemsForSale()
        {
            if (this.ItemsForSale.Count == 0)
            {
                this.GetMarketHierarchy();
            }

            return this.ItemsForSale.ToDictionary();
        }

        public MarketBotConfig MarketBotConfig()
        {
            return this._marketBotConfig;
        }

        public async Task<IEnumerable<MarketTransaction>> GetMarketOrders(ulong marketId, ulong itemId)
        {
            ConcurrentStack<MarketTransaction> output = new ConcurrentStack<MarketTransaction>();

            await this.HelperBotConnectionTest().ConfigureAwait(false);

            ulong[] itemTypes = { itemId };

            var marketList = await this.MarketBot.Req.MarketGetList(0).ConfigureAwait(false);

            await marketList.markets.ForEachAsync(32, async (MarketInfo market) =>
            {
                if (marketId != 0 && market.marketId != marketId)
                {
                    return;
                }

                var orders = await this.MarketBot.Req.MarketSelectItem(
                    new MarketSelectRequest
                    {
                        marketIds = new List<ulong> { market.marketId },
                        itemTypes = itemTypes.ToList(),
                    }).ConfigureAwait(false);

                if (orders.orders.Count > 0)
                {
                    output.PushRange(orders.orders.Select(item => item.ToMarketTransaction()).ToArray());
                }
            }).ConfigureAwait(false);

            return output;
        }

        public List<MarketEntry> GetMarketHierarchy(bool forceRefresh = false)
        {
            if (this._marketEntries.Count != 0 && !forceRefresh)
            {
                return this._marketEntries;
            }

            List<MarketEntry> output = new List<MarketEntry>();

            IGameplayBank bank = this.HelperBot.GameplayBank;

            foreach (var headerId in this._dualUniverseSettings.MarketHeaderIds)
            {
                var baseEntry = bank.GetDefinition(headerId);

                if (baseEntry == null)
                {
                    continue;
                }

                MarketEntry marketEntry = new MarketEntry()
                {
                    Id = headerId,
                    Name = baseEntry.Name,
                    DisplayName = baseEntry.LocalizedProperties.FirstOrDefault(item => item.Name == "displayName").Translation.ToString() ?? string.Empty,
                };

                var childrenObjects = baseEntry.GetChildren();

                if (childrenObjects != null && childrenObjects.Count() > 0)
                {
                    ulong[] childrenIds = childrenObjects.Select(item => item.Id).ToArray();

                    marketEntry.Children = this.GetChildren(childrenIds, bank);
                }

                if (marketEntry.Children.Count() == 0)
                {
                    this.ItemsForSale.TryAdd(marketEntry.Id, marketEntry.DisplayName);
                }

                output.Add(marketEntry);
            }

            this._marketEntries = new List<MarketEntry>(output);
            return output;
        }

        private List<MarketEntry> GetChildren(ulong[] children, IGameplayBank bank)
        {
            List<MarketEntry> output = new List<MarketEntry>();

            foreach (var childId in children)
            {
                var baseEntry = bank.GetDefinition(childId);

                if (baseEntry == null)
                {
                    continue;
                }

                var displayName = baseEntry.LocalizedProperties?.FirstOrDefault(item => item.Name == "displayName").Translation?.ToString() ?? string.Empty;
                bool hidden = baseEntry.GetStaticPropertyOpt("hidden")?.boolValue ?? false;
                string size = baseEntry.GetStaticPropertyOpt("scale")?.stringValue ?? string.Empty;
                if (string.IsNullOrEmpty(displayName) || hidden)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(size))
                {
                    size = $@" {size.ToUpper()}";
                }

                MarketEntry marketEntry = new MarketEntry()
                {
                    Id = childId,
                    Name = baseEntry.Name,
                    DisplayName = $@"{displayName}{size}",
                };

                var childrenObjects = baseEntry.GetChildren();

                if (childrenObjects != null && childrenObjects.Count() > 0)
                {
                    ulong[] childrenIds = childrenObjects.Select(item => item.Id).ToArray();

                    marketEntry.Children = this.GetChildren(childrenIds, bank);
                }

                if (marketEntry.Children.Count() == 0)
                {
                    this.ItemsForSale.TryAdd(marketEntry.Id, marketEntry.DisplayName);
                }

                output.Add(marketEntry);
            }

            return output;
        }

        private Task<Client> CreateBotUser(BotSettings settings)
        {
            LoginInformations pi = LoginInformations.BotLogin(settings.PlayerName, settings.BotUser, settings.BotPassword);
            return Client.FromFactory(this.RestDuClientFactory, pi, allowExising: true);
        }

        private async Task HelperBotConnectionTest()
        {
            try
            {
                await this.HelperBot.Req.ChatMessageSend(new NQ.MessageContent
                {
                    channel = new NQ.MessageChannel
                    {
                        channel = MessageChannelType.PRIVATE,
                        targetId = 2,
                    },
                    message = "Hi From Helper Bot",
                }).ConfigureAwait(false);
            }
            catch (NQutils.Exceptions.BusinessException be) when (be.error.code == NQ.ErrorCode.InvalidSession)
            {
                Console.WriteLine("reconnecting");
                this.HelperBot = await this.CreateBotUser(this._dualUniverseSettings.WebsiteBot).ConfigureAwait(false);
                await Task.Delay(10000).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in mod action: {e}\n{e.StackTrace}");
                await Task.Delay(10000).ConfigureAwait(false);
            }
        }

        private async Task MarketBotConnectionTest()
        {
            try
            {
                await this.MarketBot.Req.ChatMessageSend(new NQ.MessageContent
                {
                    channel = new NQ.MessageChannel
                    {
                        channel = MessageChannelType.PRIVATE,
                        targetId = 2,
                    },
                    message = "Hi From Market Bot",
                }).ConfigureAwait(false);
            }
            catch (NQutils.Exceptions.BusinessException be) when (be.error.code == NQ.ErrorCode.InvalidSession)
            {
                Console.WriteLine("reconnecting");
                this.MarketBot = await this.CreateBotUser(this._dualUniverseSettings.MarketBot).ConfigureAwait(false);
                await Task.Delay(10000).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in mod action: {e}\n{e.StackTrace}");
                await Task.Delay(10000).ConfigureAwait(false);
            }
        }

        private void InitializeItemPurchasing()
        {
            var bank = this.MarketBot.GameplayBank;

            foreach (var (key, value) in this._marketBotConfig.BuyPrices)
            {
                var entry = bank.GetDefinition(key);

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
            foreach (var (key, value) in this._marketBotConfig.BuyRecursivePrices)
            {
                var baseEntry = bank.GetDefinition(key);

                if (baseEntry == null)
                {
                    continue;
                }

                var childrenIds = baseEntry.GetChildrenIdsRecursive();
                foreach (var childId in childrenIds)
                {
                    var entry = bank.GetDefinition(childId);

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
            foreach (var key in this._marketBotConfig.OnlyResellItemsRecursive)
            {
                var baseEntry = bank.GetDefinition(key);

                if (baseEntry == null)
                {
                    continue;
                }

                var childrenIds = baseEntry.GetChildrenIdsRecursive();
                foreach (var childId in childrenIds)
                {
                    var entry = bank.GetDefinition(childId);

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
