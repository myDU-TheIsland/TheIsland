// <copyright file="WalletService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System.Threading.Tasks;
    using Backend;
    using Backend.Business;
    using Microsoft.Extensions.DependencyInjection;
    using NQutils.Sql;
    using Orleans;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Settings;

    public interface IWalletService
    {
    }

    public class WalletService : ITalentPointService
    {
        private readonly IGeneralBot _bot;
        private readonly ISql _sql;
        private readonly IGameplayBank _gameplayBank;
        private readonly IDataAccessor _dataAccessor;
        private readonly IClusterClient _orleans;
        private readonly DualUniverseSettings _settings;

        private Task ConnectionTest() => this._bot.BotConnectionTest();

        public WalletService(DualUniverseSettings settings, IGeneralBot bot)
        {
            this._bot = bot;
            this._sql = bot.ServiceProvider.GetRequiredService<ISql>();
            this._gameplayBank = bot.ServiceProvider.GetRequiredService<IGameplayBank>();
            this._dataAccessor = bot.ServiceProvider.GetRequiredService<IDataAccessor>();
            this._orleans = bot.Orleans;
            this._settings = settings;
        }
    }
}
