// <copyright file="StoreService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System.Threading.Tasks;
    using Backend;
    using Backend.Business;
    using Backend.Database;
    using Microsoft.Extensions.DependencyInjection;
    using NQ;
    using NQ.Interfaces;
    using NQutils.Sql;
    using Orleans;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services.Blueprint;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Settings;

    public interface IStoreService : IAppService
    {
        Task<StoreItem[]> GetItemList(bool includeInactive = false);

        Task<StoreItem> GetItem(double itemId);

        Task<StoreItem> CreateItem(StoreItem item);

        Task<StoreItem> EditItem(StoreItem item);
    }

    public class StoreService : IStoreService
    {
        private readonly IGeneralBot _bot;
        private readonly ISql _sql;
        private readonly IGameplayBank _gameplayBank;
        private readonly IDataAccessor _dataAccessor;
        private readonly IClusterClient _orleans;
        private readonly DualUniverseSettings _settings;
        private readonly StoreItemRepository _storeItemRepository;

        private Task ConnectionTest() => this._bot.BotConnectionTest();

        public StoreService(StoreItemRepository storeItemRepository, DualUniverseSettings settings, IGeneralBot bot)
        {
            this._storeItemRepository = storeItemRepository;
            this._bot = bot;
            this._sql = bot.ServiceProvider.GetRequiredService<ISql>();
            this._gameplayBank = bot.ServiceProvider.GetRequiredService<IGameplayBank>();
            this._dataAccessor = bot.ServiceProvider.GetRequiredService<IDataAccessor>();
            this._orleans = bot.Orleans;
            this._settings = settings;
        }

        public async Task<StoreItem[]> GetItemList(bool includeInactive = false)
        {
            var results = (await this._storeItemRepository.GetAsync().ConfigureAwait(false)).ToArray();

            if (includeInactive)
            {
                return results;
            }

            return results.Where(item => item.is_active = true).ToArray();
        }

        public Task<StoreItem> GetItem(double itemId)
        {
            return this._storeItemRepository.GetAsync(itemId);
        }

        public async Task<StoreItem> CreateItem(StoreItem item)
        {
            item.id = await this._storeItemRepository.AddAsync(item).ConfigureAwait(false);

            return item;
        }

        public async Task<StoreItem> EditItem(StoreItem item)
        {
            await this._storeItemRepository.UpdateAsync(item).ConfigureAwait(false);

            return item;
        }
    }
}
