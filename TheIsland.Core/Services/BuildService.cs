// <copyright file="BuildService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using Backend;
    using Backend.Database;
    using Microsoft.Extensions.DependencyInjection;
    using NQ;
    using NQ.Interfaces;
    using NQutils.Def;
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

        public Task ConvertOreIntoPure()
        {
            var pures = this._marketBot.GetAllItemsMarketEntries().Where(item => item.Type == "Pure").ToList();
            foreach (var pure in pures)
            {
            }

            return Task.CompletedTask;
        }

        public async Task<ulong> GetRecipeId(double itemId)
        {
            var recipes = await this._recipes.GetAllRecipes().ConfigureAwait(false);
            var recipe = recipes.FirstOrDefault(item => item.products[0].itemId == Convert.ToUInt64(itemId));
            return recipe?.id ?? 0;
        }

        /// <summary>
        /// Gets the recipe results with talents applied for recipe provided.
        /// </summary>
        /// <param name="recipeId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        /// <remarks>Duplicated from Orleans IndustryUnitGrain.cs.</remarks>
        public async Task<(NQ.Recipe? recipe, ulong batchSize)> CraftItem(ulong recipeId, double playerId = 10000)
        {
            NQ.Recipe? recipe = this._marketBot.Bot.Recipes.FirstOrDefault(item => item.id == recipeId);

            if (recipe == null)
            {
                return (null, 0UL);
            }

            NQ.Recipe res = new NQ.Recipe()
            {
                id = recipe.id,
                time = recipe.time,
            };
            foreach (Ingredient ingredient in recipe.ingredients)
            {
                res.ingredients.Add(new Ingredient()
                {
                    itemId = ingredient.itemId,
                    quantity = ingredient.quantity.Copy(),
                });
            }

            foreach (Ingredient product in recipe.products)
            {
                res.products.Add(new Ingredient()
                {
                    itemId = product.itemId,
                    quantity = product.quantity.Copy(),
                });
            }

            ulong itemId = res.products[0].itemId;

            ITalentGrain? talent = this._marketBot.Orleans.GetTalentGrain(Convert.ToUInt64(playerId));
            GameplayModifiers? mainProductModifiers = await talent.Bonuses(itemId, true).ConfigureAwait(false);
            res.time = (float)EffectSystem.ApplyModifiers((double)res.time, EffectSystem.RegroupModifiers(await talent.Bonuses(itemId, true).ConfigureAwait(false)).GetValueOrDefault<string, GameplayModifiers>("industryEfficiency"));
            if ((double)res.time < 1.0)
            {
                res.time = 1f;
            }

            Dictionary<string, GameplayModifiers> dictionary = EffectSystem.RegroupModifiers(mainProductModifiers);
            res.time = (float)EffectSystem.ApplyModifiers((double)res.time, dictionary.GetValueOrDefault<string, GameplayModifiers>("craftingDurationMul"));
            if ((double)res.time < 1.0)
            {
                res.time = 1f;
            }

            double requiredAdd = EffectSystem.ApplyModifiers(0.0, dictionary.GetValueOrDefault<string, GameplayModifiers>("requiredMaterialAdd"));
            double requiredMul = EffectSystem.ApplyModifiers(1.0, dictionary.GetValueOrDefault<string, GameplayModifiers>("requiredMaterialMul"));

            foreach (var ingredient in res.ingredients)
            {
                ingredient.quantity = (ItemQuantity)(long)(((double)ingredient.quantity.value + requiredAdd) * requiredMul);
            }

            double productAdd = EffectSystem.ApplyModifiers(0.0, dictionary.GetValueOrDefault<string, GameplayModifiers>("productMaterialAdd"));
            double productMul = EffectSystem.ApplyModifiers(1.0, dictionary.GetValueOrDefault<string, GameplayModifiers>("productMaterialMul"));

            foreach (var product in res.products)
            {
                product.quantity = (ItemQuantity)(long)(((double)product.quantity.value + productAdd) * productMul);
            }

            talent = null;
            mainProductModifiers = null;

            res.time *= 1;
            if ((double)res.time < 1.0)
            {
                res.time = 1f;
            }

            long minRecipeTime = (long)this._gameplayBank.GetBaseObject<IndustryConfig>().MinRecipeTime;
            if ((double)res.time >= (double)minRecipeTime)
            {
                return (res, 1UL);
            }

            long num = minRecipeTime / (long)res.time;

            res.time *= (float)num;
            foreach (Ingredient ingredient in res.ingredients)
            {
                ingredient.quantity *= num;
            }

            foreach (Ingredient product in res.products)
            {
                product.quantity *= num;
            }

            return (res, (ulong)num);
        }
        #endregion
    }
}
