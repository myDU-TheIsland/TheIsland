// <copyright file="BuildService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using Backend;
    using Microsoft.Extensions.DependencyInjection;
    using NQ;
    using Orleans.Providers;
    using TheIsland.Core.Bots;

    public class BuildService : IAppService
    {
        private readonly IMarketBot _marketBot;
        private readonly IRecipes _recipes;
        private readonly IGameplayBank _gameplayBank;

        public BuildService(IMarketBot dualClient)
        {
            this._marketBot = dualClient;
            this._gameplayBank = this._marketBot.ServiceProvider.GetRequiredService<IGameplayBank>();
            this._recipes = this._marketBot.ServiceProvider.GetRequiredService<IRecipes>();
        }

        #region ItemCosts
        public async Task PriceItems()
        {
            //price pure
            //price product
            var recipes = await this._recipes.GetAllPretty().ConfigureAwait(false);
            return;
        }
        #endregion
    }
}
