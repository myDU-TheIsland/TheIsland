// <copyright file="GeneralBot.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Bots
{
    using System.Collections.Concurrent;
    using Backend;
    using Backend.Database;
    using BotLib.Generated;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NQ;
    using NQ.Interfaces;
    using NQutils.Sql;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Settings;

    public interface IGeneralBot : IBotClient
    {
        #region User Functions
        Task<string> ImportBP(ulong playerId, byte[] bp);
        #endregion

        #region General Functions
        List<MarketEntry> GetMarketHierarchy(bool forceRefresh = false);

        Dictionary<double, string> GetListOfSellableItems();
        #endregion

        #region Admin Functions
        Task<List<string>> GiveTalentPoints(double amount);

        Task<List<string>> GiveAllQuanta(double amount, string note);
        #endregion
    }

    public class GeneralBot : BotClient, IGeneralBot
    {
        private readonly DualPlayerRepository _dualPlayerRepository;

        private readonly DualUniverseSettings _dualUniverseSettings;

        private List<MarketEntry> _marketEntries { get; set; } = new List<MarketEntry>();

        private ConcurrentDictionary<double, string> ItemsForSale { get; set; } = new ConcurrentDictionary<double, string>();

        public GeneralBot(DualPlayerRepository dualPlayerRepository, DualUniverseSettings settings, ILogger<IGeneralBot> logger) : base(settings.WebsiteBot, logger)
        {
            this._dualUniverseSettings = settings;
            this._dualPlayerRepository = dualPlayerRepository;
        }

        #region General Functions
        public List<MarketEntry> GetMarketHierarchy(bool forceRefresh = false)
        {
            if (this._marketEntries.Count != 0 && !forceRefresh)
            {
                return this._marketEntries;
            }

            List<MarketEntry> output = new List<MarketEntry>();

            IGameplayBank bank = this.Bot.GameplayBank;

            foreach (ulong headerId in this._dualUniverseSettings.MarketHeaderIds)
            {
                IGameplayDefinition? baseEntry = bank.GetDefinition(headerId);

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

                IEnumerable<IGameplayDefinition> childrenObjects = baseEntry.GetChildren();

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

        #region User Functions

        public async Task<string> ImportBP(ulong playerId, byte[] bp)
        {
            await this.BotConnectionTest().ConfigureAwait(false);
            BlueprintId blueprintId = 0;
            try
            {
                blueprintId = await this.DataAccessor.BlueprintImport(bp, new EntityId { playerId = playerId }).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                return @$"Failed to import BP. ({exception.ToString()})";
            }

            BlueprintProperties blueprintInfo = await this.Orleans.GetBlueprintGrain().GetBlueprintInfo(blueprintId).ConfigureAwait(false);
            BlueprintModel bluepprintModel = await this.ServiceProvider.GetRequiredService<ISql>().Read(blueprintId).ConfigureAwait(false);

            if (bluepprintModel.FreeDeploy && !await this.Orleans.GetPlayerGrain(playerId).IsAdmin().ConfigureAwait(false))
            {
                return "You are not allowed to import free deploy blueprints";
            }

            IInventoryGrain pig = this.Orleans.GetInventoryGrain(playerId);
            IGameplayDefinition? blueprintTypeInfo = this.ServiceProvider.GetRequiredService<IGameplayBank>().GetDefinition("Blueprint");

            if (blueprintTypeInfo == null)
            {
                return "System problem. Can't identify blueprint type id";
            }

            ItemInfo item = new ItemInfo
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
        #endregion

        #region Admin Functions
        public async Task<List<string>> GiveTalentPoints(double amount)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            try
            {
                IEnumerable<Entities.DualPlayer> players = await this._dualPlayerRepository.GetAsync().ConfigureAwait(false);

                await this.BotConnectionTest().ConfigureAwait(false);

                foreach (Entities.DualPlayer player in players)
                {
                    if (player.admin || player.is_bot)
                    {
                        logMessage(@$"Skipping user '{player.display_name}', is a bot or admin!");
                        continue;
                    }

                    PlayerTalentState response = await this.DataAccessor.PlayerTalentAsync(Convert.ToUInt64(player.id)).ConfigureAwait(false);

                    ulong currentAvail = response.pointsAcquired - response.pointsSpent;

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

                IEnumerable<Entities.DualPlayer> players = await this._dualPlayerRepository.GetAsync().ConfigureAwait(false);

                await this.BotConnectionTest().ConfigureAwait(false);

                Currency wallet = await this.Bot.Req.GetWallet().ConfigureAwait(false);

                double neededBalance = giveToPlayer * players.Count();

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

                foreach (Entities.DualPlayer player in players)
                {
                    if (player.admin || player.is_bot)
                    {
                        logMessage(@$"Skipping user '{player.display_name}', is a bot or admin!");
                        continue;
                    }

                    string reason = string.IsNullOrEmpty(note) ? "Thank You For Playing" : note;

                    logMessage(@$"Sending Quanta to '{player.display_name}'!");

                    await this.Bot.Req.WalletTransferRequest(new WalletTransfer()
                    {
                        amount = Convert.ToUInt64(giveToPlayer),
                        toWallet = new EntityId() { playerId = Convert.ToUInt64(player.id) },
                        fromWallet = new EntityId() { playerId = this.Bot.PlayerId },
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
        #endregion

        #region Private Functions

        private List<MarketEntry> GetChildren(ulong[] children, IGameplayBank bank)
        {
            List<MarketEntry> output = new List<MarketEntry>();

            foreach (ulong childId in children)
            {
                IGameplayDefinition? baseEntry = bank.GetDefinition(childId);

                if (baseEntry == null)
                {
                    continue;
                }

                string displayName = baseEntry.LocalizedProperties?.FirstOrDefault(item => item.Name == "displayName").Translation?.ToString() ?? string.Empty;
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

                IEnumerable<IGameplayDefinition> childrenObjects = baseEntry.GetChildren();

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
        #endregion
    }
}