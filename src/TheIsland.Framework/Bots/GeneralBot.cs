// <copyright file="GeneralBot.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Framework.Bots
{
    using System.Collections.Generic;
    using Backend;
    using Backend.Database;
    using BotLib.Generated;
    using Microsoft.Extensions.Logging;
    using NQ;
    using TheIsland.Core.Settings;
    using TheIsland.Core.ThreadSafe;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;

    public interface IGeneralBot : IBotClient
    {
        #region Admin Functions
        Task<List<string>> GiveTalentPoints(double amount);

        Task<List<string>> GiveAllQuanta(double amount, string note, double hours = 1);

        Task<List<string>> RespecEntireCategoryForAllPlayers(string category);
        #endregion
    }

    public class GeneralBot : BotClient, IGeneralBot
    {
        private readonly IDualPlayerRepository _dualPlayerRepository;

        private readonly DualUniverseSettings _dualUniverseSettings;

        private readonly IDualWalletRepository _dualWalletRepository;

        public GeneralBot(
            IDualWalletRepository dualWalletRepository,
            IDualPlayerRepository dualPlayerRepository,
            DualUniverseSettings settings,
            ILogger<IGeneralBot> logger) : base(settings.WebsiteBot, logger)
        {
            this._dualUniverseSettings = settings;
            this._dualPlayerRepository = dualPlayerRepository;
            this._dualWalletRepository = dualWalletRepository;
        }

        public override Task BotConnectionTestAsync()
        {
            return ThreadSafeExecution.ThreadExecution(nameof(GeneralBot), this.InternalBotConnectionTestAsync);
        }

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
                IEnumerable<DualPlayer> players = await this._dualPlayerRepository.GetAsync().ConfigureAwait(false);

                await this.BotConnectionTestAsync().ConfigureAwait(false);

                foreach (DualPlayer player in players)
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

        public async Task<List<string>> GiveAllQuanta(double amount, string note, double hours = 1)
        {
            List<string> log = new List<string>();

            hours = hours * -1;

            DateTime startTime = DateTime.Now;
            DateTime pastHour = startTime.AddHours(hours);

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            try
            {
                double giveToPlayer = amount * 100;

                IEnumerable<DualPlayer> players = await this._dualPlayerRepository.GetAsync().ConfigureAwait(false);

                await this.BotConnectionTestAsync().ConfigureAwait(false);

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

                foreach (DualPlayer player in players)
                {
                    if (player.admin || player.is_bot)
                    {
                        logMessage(@$"Skipping user '{player.display_name}', is a bot or admin!");
                        continue;
                    }

                    var getPrevTransactions = await this._dualWalletRepository.GetAllTransactionsBetween(this.Bot.PlayerId, player.id).ConfigureAwait(false);
                    var transactions = getPrevTransactions.Where(item => item.time > pastHour && (item.amount == giveToPlayer || item.amount == -giveToPlayer)).ToArray();

                    if (transactions.Count() > 0)
                    {
                        if (transactions.Count() > 1)
                        {
                            var deduct = (transactions.Count() - 1) * amount;
                            var playerWallet = await this._dualPlayerRepository.GetAsync(player.id).ConfigureAwait(false);
                            await this.DataAccessor.PlayerWalletUpdateAsync(Convert.ToUInt64(player.id), Convert.ToInt64(playerWallet.wallet - deduct)).ConfigureAwait(false);
                            logMessage(@$"Deducting from user '{player.display_name}', got multiple payments.");
                        }

                        //transaction in the last hour
                        logMessage(@$"Skipping user '{player.display_name}', w allet has transaction in the last hour({hours}).");
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

                    await Task.Delay(100).ConfigureAwait(false);
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

        public async Task<List<string>> RespecEntireCategoryForAllPlayers(string category)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            IGameplayDefinition? talentGroups = this.Bot.GameplayBank.GetDefinition("TalentGroup");
            IGameplayDefinition? talentEntry = this.Bot.GameplayBank.GetDefinition("Talent");

            if (talentEntry == null || talentGroups == null)
            {
                logMessage(@$"Couldnt get the Talent or TalentGroup definitions");
                return log;
            }

            string[] subGroups = talentGroups.GetChildren().Where(item => item.GetStaticProperty("group").stringValue == category).Select(item => item.Name).ToArray();
            logMessage(@$"Found {subGroups.Length} subgroups");
            IGameplayDefinition[] subItems = talentEntry.GetChildren().Where(item => subGroups.Contains(item.GetStaticProperty("group").stringValue)).ToArray();
            logMessage(@$"Found {subItems.Length} actual talents to reset");
            IEnumerable<DualPlayer> players = await this._dualPlayerRepository.GetAsync().ConfigureAwait(false);

            foreach (DualPlayer player in players)
            {
                if (player.admin || player.is_bot)
                {
                    logMessage(@$"Skipping user '{player.display_name}', is a bot or admin!");
                    continue;
                }

                foreach (IGameplayDefinition subItem in subItems)
                {
                    logMessage(@$"Resetting '{player.display_name}' for {subItem.Name}!");
                    await this.DataAccessor.PlayerTalentRespecAsync(Convert.ToUInt64(player.id), subItem.Id).ConfigureAwait(false);
                }
            }

            return log;
        }
        #endregion
    }
}