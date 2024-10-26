// <copyright file="TalentPointService.cs" company="Paul Layne">
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

    public interface ITalentPointService : IAppService
    {
    }

    public class TalentPointService : ITalentPointService
    {
        private readonly IGeneralBot _bot;
        private readonly ISql _sql;
        private readonly IGameplayBank _gameplayBank;
        private readonly IDataAccessor _dataAccessor;
        private readonly IClusterClient _orleans;
        private readonly DualUniverseSettings _settings;

        private Task ConnectionTest() => this._bot.BotConnectionTest();

        public TalentPointService(DualUniverseSettings settings, IGeneralBot bot)
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
