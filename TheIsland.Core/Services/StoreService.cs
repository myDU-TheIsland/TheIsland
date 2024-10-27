// <copyright file="StoreService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System.Threading.Tasks;
    using Backend;
    using Backend.Business;
    using Backend.Database;
    using BotLib.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using NQ;
    using NQutils.Sql;
    using Orleans;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Helpers;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Settings;

    public interface IStoreService : IAppService
    {
        Task<StoreItemEntity[]> GetItemList(bool includeInactive = false);

        Task<StoreItemEntity> GetItem(double itemId);

        Task<StoreItemEntity> CreateItem(StoreItemEntity item);

        Task<StoreItemEntity> EditItem(StoreItemEntity item);

        Task<string> SaveImage(IFormFile image);

        Task<double> GetWalletBalance(double playerId);

        Task<double> GetTalentPoints(double playerId);

        Task<List<string>> PurchaseItem(double playerId, double discordId, double itemId, double quantity);

        double GetTalentPointPercentage(double talentPoints);
    }

    public class StoreService : IStoreService
    {
        private readonly DualPlayerRepository _dualPlayerRepository;

        private readonly IGeneralBot _bot;
        private readonly ISql _sql;
        private readonly IGameplayBank _gameplayBank;
        private readonly IDataAccessor _dataAccessor;
        private readonly IClusterClient _orleans;
        private readonly DualUniverseSettings _settings;
        private readonly StoreItemRepository _storeItemRepository;
        private readonly StorePurchaseHistoryRepository _storePurchaseHistoryRepository;

        private Task ConnectionTest() => this._bot.BotConnectionTest();

        public StoreService(DualPlayerRepository dualPlayerRepository, StorePurchaseHistoryRepository storePurchaseHistoryRepository, StoreItemRepository storeItemRepository, DualUniverseSettings settings, IGeneralBot bot)
        {
            this._dualPlayerRepository = dualPlayerRepository;
            this._storeItemRepository = storeItemRepository;
            this._storePurchaseHistoryRepository = storePurchaseHistoryRepository;

            this._bot = bot;
            this._sql = bot.ServiceProvider.GetRequiredService<ISql>();
            this._gameplayBank = bot.ServiceProvider.GetRequiredService<IGameplayBank>();
            this._dataAccessor = bot.ServiceProvider.GetRequiredService<IDataAccessor>();
            this._orleans = bot.Orleans;
            this._settings = settings;
        }

        public async Task<StoreItemEntity[]> GetItemList(bool includeInactive = false)
        {
            StoreItemEntity[] results = (await this._storeItemRepository.GetAsync().ConfigureAwait(false)).ToArray();

            if (includeInactive)
            {
                return results;
            }

            if (results.Length == 0)
            {
                return Array.Empty<StoreItemEntity>();
            }

            return results.Where(item => item.is_active == true).ToArray();
        }

        public Task<StoreItemEntity> GetItem(double itemId)
        {
            return this._storeItemRepository.GetAsync(itemId);
        }

        public async Task<StoreItemEntity> CreateItem(StoreItemEntity item)
        {
            item.id = await this._storeItemRepository.AddAsync(item).ConfigureAwait(false);

            return item;
        }

        public async Task<StoreItemEntity> EditItem(StoreItemEntity item)
        {
            await this._storeItemRepository.UpdateAsync(item).ConfigureAwait(false);

            return item;
        }

        public async Task<string> SaveImage(IFormFile image)
        {
            if (image == null)
            {
                return string.Empty;
            }

            string fileName = image.FileName;
            string fileExtension = fileName.Split('.').Last();
            string newFileName = $@"{Guid.NewGuid()}.{fileExtension}";
            string filePath = Path.Combine(this._settings.StoreImagePath, newFileName);

            if (!System.IO.Directory.Exists(this._settings.StoreImagePath))
            {
                System.IO.Directory.CreateDirectory(this._settings.StoreImagePath);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                image.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes).ConfigureAwait(false);
            }

            return newFileName;
        }

        public async Task<double> GetTalentPoints(double playerId)
        {
            PlayerTalentState playerState = await this._dataAccessor.PlayerTalentAsync(Convert.ToUInt64(playerId)).ConfigureAwait(false);
            return playerState.pointsAcquired;
        }

        public async Task<List<string>> PurchaseItem(double playerId, double discordId, double itemId, double quantity)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            await this.ConnectionTest().ConfigureAwait(false);

            StoreItemEntity item = await this.GetItem(itemId).ConfigureAwait(false);

            if (item == null)
            {
                logMessage("could not find item to purchase");
                return log;
            }

            if (!item.is_active)
            {
                logMessage("This item is not currently able to be purchased.");
                return log;
            }

            var historyOfPurchase = await this._storePurchaseHistoryRepository.GetPlayerPurchaseHistory(playerId).ConfigureAwait(false);

            double howManyHaveBeenPurchased = historyOfPurchase?.Where(historyItem => historyItem.store_item.id == itemId).Sum(historyItem => historyItem.quantity) ?? 0;

            if (item.limit > 0 && item.limit < howManyHaveBeenPurchased + quantity)
            {
                logMessage(@$"Can not purchase that many {item.name}.");
                return log;
            }

            DualPlayer player = await this._dualPlayerRepository.GetAsync(playerId).ConfigureAwait(false);
            if (player.connected)
            {
                logMessage("You must log out of the game to perform website purchases.");
                return log;
            }

            double total = item.price * quantity;

            if (player.wallet < total)
            {
                logMessage("You do not have enough quanta to finish this purchase.");
                return log;
            }

            if (!await this.DebitPlayerWallet(playerId, total, @$"Purchase of {quantity}x {item.name}").ConfigureAwait(false))
            {
                logMessage("Failed to take quanta for purchase");
                return log;
            }

            StorePurchaseHistory newHistoryRecord = new StorePurchaseHistory()
            {
                quantity = quantity,
                discord_user = discordId,
                player_id = playerId,
                price = total,
                store_item = item.ToStoreItem(),
                log = log,
            };

            try
            {
                //execute actual giving of stuff
                if (item.content.RespecTalentPoints)
                {
                    await this._dataAccessor.PlayerTalentRespecAllAsync(Convert.ToUInt64(playerId)).ConfigureAwait(false);
                }

                if (item.content.Items.Count != 0)
                {
                    foreach (var purchaseItem in item.content.Items)
                    {
                        if (purchaseItem.Key == 0)
                        {
                            continue;
                        }

                        var gamebankItem = this._gameplayBank.GetDefinition(Convert.ToUInt64(purchaseItem.Key));
                        if (gamebankItem == null)
                        {
                            logMessage(@$"Failed to find item {purchaseItem.Key}");
                            continue;
                        }

                        var inventoryItem = new ItemAndQuantity
                        {
                            item = gamebankItem.AsItemInfo(),
                            quantity = Convert.ToInt64(purchaseItem.Value),
                        };

                        await this._dataAccessor.PlayerInventoryGiveAsync(Convert.ToUInt64(playerId), inventoryItem).ConfigureAwait(false);
                        logMessage(@$"Added {purchaseItem.Value}x {purchaseItem.Key} to {playerId} inventory");
                    }
                }

                if (item.content.Skins.Count != 0)
                {
                    foreach (var purchaseSkin in item.content.Skins)
                    {
                        if (purchaseSkin.Key == 0)
                        {
                            continue;
                        }

                        await this._dataAccessor.PlayerGiveSkin(Convert.ToUInt64(playerId), Convert.ToUInt64(purchaseSkin.Key), purchaseSkin.Value).ConfigureAwait(false);
                        logMessage(@$"Added '{purchaseSkin.Key} {purchaseSkin.Value}' to {playerId} inventory");
                    }
                }

                if (item.content.TalentPoints > 0 && item.price != 0)
                {
                    //injectors
                    log.AddRange(await this.GiveTalentPointInjector(playerId, item.content.TalentPoints * quantity).ConfigureAwait(false));
                }
                else if (item.content.TalentPoints > 0 && item.price == 0)
                {
                    PlayerTalentState playerState = await this._dataAccessor.PlayerTalentAsync(Convert.ToUInt64(playerId)).ConfigureAwait(false);
                    if (playerState.pointsAcquired > 40000000)
                    {
                        logMessage(@$"Sorry can't grant free point once you hit 40million talent points.");
                    }
                    else
                    {
                        //one time grant
                        log.AddRange(await this.GiveTalentPointGrant(playerId, item.content.TalentPoints).ConfigureAwait(false));
                    }
                }
            }
            catch (Exception exception)
            {
                logMessage(@$"Error: {exception}");
            }

            await this._storePurchaseHistoryRepository.AddAsync(newHistoryRecord).ConfigureAwait(false);

            return log;
        }

        public double GetTalentPointPercentage(double talentPoints)
        {
            double currentPercentage = 0;

            switch (talentPoints)
            {
                case > 250000000:
                    currentPercentage = .2;
                    break;
                case > 150000000:
                    currentPercentage = .4;
                    break;
                case > 100000000:
                    currentPercentage = .6;
                    break;
                case > 50000000:
                    currentPercentage = .8;
                    break;
                default:
                    currentPercentage = 1;
                    break;
            }

            return currentPercentage;
        }

        internal async Task<List<string>> GiveTalentPointGrant(double playerId, double amount)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            PlayerTalentState playerState = await this._dataAccessor.PlayerTalentAsync(Convert.ToUInt64(playerId)).ConfigureAwait(false);

            ulong currentAvail = playerState.pointsAcquired - playerState.pointsSpent;

            long settingTalentsTo = Convert.ToInt64(currentAvail + amount);

            logMessage(@$"Setting '{playerId}' available talent points to {settingTalentsTo}");

            await this._dataAccessor.PlayerTalentSetAvailableAsync(Convert.ToUInt64(playerId), settingTalentsTo).ConfigureAwait(false);

            return log;
        }

        internal async Task<List<string>> GiveTalentPointInjector(double playerId, double amount)
        {
            List<string> log = new List<string>();

            void logMessage(string input)
            {
                log.Add($@"{DateTime.Now} :: {input}");
            }

            PlayerTalentState playerState = await this._dataAccessor.PlayerTalentAsync(Convert.ToUInt64(playerId)).ConfigureAwait(false);

            double totalPoints = playerState.pointsAcquired;

            double addPoints = 0;

            double step = 50000;

            if (amount % 500000 == 0)
            {
                //large injector
                step = 500000;
            }

            double totalIterations = amount / step;

            logMessage($@"Base TP: {totalPoints} | {this.GetTalentPointPercentage(totalPoints) * 100}%");

            for (var x = 0; x < totalIterations; x++)
            {
                var multi = this.GetTalentPointPercentage(totalPoints + addPoints);
                var addition = step * multi;
                logMessage($@"Adding TP via injector[{x}]: {addition} to {totalPoints} | {multi * 100}% Used");
                addPoints += addition;
            }

            await this.GiveTalentPointGrant(playerId, addPoints).ConfigureAwait(false);

            return log;
        }

        public async Task<double> GetWalletBalance(double playerId)
        {
            return (await this._dualPlayerRepository.GetAsync(playerId).ConfigureAwait(false)).wallet / 100;
        }

        internal async Task<bool> DebitPlayerWallet(double playerId, double amount, string reason)
        {
            await this.ConnectionTest().ConfigureAwait(false);
            DualPlayer player = await this._dualPlayerRepository.GetAsync(playerId).ConfigureAwait(false);

            if (player.connected)
            {
                throw new UnauthorizedAccessException("Can't Access an account that is logged in!");
            }

            double actualDebit = -(amount * 100);

            //take money
            await this._dualPlayerRepository.UpdateWallet(playerId, actualDebit).ConfigureAwait(false);

            var sourceWallet = new EntityId { playerId = Convert.ToUInt64(playerId) };
            await this._sql
                .InsertWalletOperation(
                    sourceWallet,
                    new EntityId { playerId = 10187 },
                    (long)actualDebit,
                    WalletOperationType.Transfer,
                    new WalletOperationDetail
                    {
                        transfer = new WalletOperationTransfer
                        {
                            reason = reason,
                            initiatingPlayer = new NamedEntity
                            {
                                id = sourceWallet,
                                name = "TheIsle Website Store - Purchase",
                            },
                        },
                    })
                .ConfigureAwait(false);

            return true;
        }
    }
}
