// <copyright file="BuildService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection.Metadata.Ecma335;
    using System.Text.Json;
    using Amazon.Runtime.Internal.Transform;
    using Backend;
    using Microsoft.Extensions.DependencyInjection;
    using NQ;
    using NQ.Interfaces;
    using NQutils.Def;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Helpers;
    using TheIsland.Core.Helpers.Caching;
    using TheIsland.Core.Services.SQL;

    public class BuildService : IAppService
    {
        private readonly IMarketBot _marketBot;
        private readonly IRecipes _recipes;
        private readonly IGameplayBank _gameplayBank;

        private readonly ThreadSafeCacheLookup<Task> _threadSafeCacheLookup;
        private readonly FactoryLedgerRepository _factoryLedgerRepository;
        private readonly CraftedItemRepository _craftedItemRepository;

        public BuildService(
            FactoryLedgerRepository factoryLedgerRepository,
            CraftedItemRepository craftedItemRepository,
            IMarketBot dualClient)
        {
            this._factoryLedgerRepository = factoryLedgerRepository;
            this._craftedItemRepository = craftedItemRepository;

            this._marketBot = dualClient;
            this._gameplayBank = this._marketBot.ServiceProvider.GetRequiredService<IGameplayBank>();
            this._recipes = this._marketBot.ServiceProvider.GetRequiredService<IRecipes>();

            this._threadSafeCacheLookup = new ThreadSafeCacheLookup<Task>();
        }

        #region Factory Stuff
        public async Task FactoryConvertOreIntoPure()
        {
            var ores = this._marketBot.GetAllItemsMarketEntries().Where(item => item.Type == "Ore").ToList();
            var oresDB = (await this._factoryLedgerRepository.GetAllEntriesByItemIdsAsync(ores.Select(item => item.Id).ToArray()).ConfigureAwait(false)).ToList();

            //sum up all ores into singluar line items
            List<FactoryLedgerEntry> totals =
                oresDB
                .GroupBy(item => item.item_id)
                .Select(cl => new FactoryLedgerEntry()
                {
                    item_id = cl.First().item_id,
                    quantity = cl.Sum(c => c.quantity),
                    price = cl.Sum(c => c.price),
                }).ToList();

            var pures = new List<FactoryLedgerEntry>();

            foreach (var ore in totals)
            {
                var recipe = await this.CraftItem((await this.GetRecipeIdsByIngredientItemIdAsync(ore.item_id).ConfigureAwait(false)).First()).ConfigureAwait(false);
                if (recipe == null)
                {
                    //remove this ore from the list
                    oresDB = oresDB.Where(item => item.item_id != ore.item_id).ToList();
                    continue;
                }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                double multiplier = ore.quantity / recipe.ingredients.First().quantity.ToVolume();

                for (var i = 0; i < (recipe?.products.Count ?? 0); i++)
                {
                    var item = recipe.products[i];

                    pures.Add(new FactoryLedgerEntry()
                    {
                        item_id = item.itemId,
                        price = i == 0 ? ore.price : 0,
                        quantity = Math.Round(item.quantity.ToVolume() * multiplier, 2),
                    });
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            //remove ore
            await this._factoryLedgerRepository.RemoveAsync(oresDB).ConfigureAwait(false);

            //add pures
            await this._factoryLedgerRepository.AddAsync(pures).ConfigureAwait(false);

            return;
        }

        public async Task CraftItems()
        {
            ConcurrentDictionary<double, CraftedItem> recipesDictionary = new ConcurrentDictionary<double, CraftedItem>();
            List<Classes.ItemEntry> pures = this._marketBot.GetAllItemsMarketEntries().Where(item => item.Type == "Pure").ToList();

            List<ulong> removeRecipes = new List<ulong>();
            foreach (var item in pures)
            {
                removeRecipes.Add(await this.GetRecipeIdByProductItemIdAsync(item.Id).ConfigureAwait(false));
            }

            Queue<ulong> queue = new Queue<ulong>(this._marketBot.Bot.Recipes.Where(item => !removeRecipes.Contains(item.id)).Select(item => item.id));

            while (queue.Count != 0)
            {
                var queueId = queue.Dequeue();

                var craftedItem = await this.CraftItem(queueId, 10000).ConfigureAwait(false);

                if (craftedItem == null)
                {
                    continue;
                }

                var itemId = craftedItem.products[0].itemId;
                var quantityProduced = craftedItem.products[0].quantity.ToVolume();

                recipesDictionary.TryAdd(
                    itemId,
                    new CraftedItem()
                    {
                        id = itemId,
                        crafting_requirements = craftedItem.ingredients.ToDictionary(key => (double)key.itemId, value => Math.Round(value.quantity.ToVolume() / quantityProduced, 2)),
                    });
            }

            void Reduce(CraftedItem item)
            {
                List<double> items = item.crafting_requirements.Keys.Where(item => !isPure(item)).ToList();
                while (items.Count() > 0)
                {
                    foreach (var entry in items)
                    {
                        item.crafting_requirements.Remove(entry);

                        if (recipesDictionary.ContainsKey(entry))
                        {
                            if (!isOnlyPure(recipesDictionary[entry]))
                            {
                                Reduce(recipesDictionary[entry]);
                            }

                            foreach (var newIngredient in recipesDictionary[entry].crafting_requirements)
                            {
                                if (item.crafting_requirements.ContainsKey(newIngredient.Key))
                                {
                                    item.crafting_requirements[newIngredient.Key] += newIngredient.Value;
                                }
                                else
                                {
                                    item.crafting_requirements.Add(newIngredient);
                                }
                            }
                        }
                    }

                    items = item.crafting_requirements.Keys.Where(item => !isPure(item)).ToList();
                }
            }

            bool isPure(double itemId)
            {
                return pures.Select(item => item.Id).ToArray().Contains(itemId);
            }

            bool isOnlyPure(CraftedItem item)
            {
                return item.crafting_requirements.Keys.All(isPure);
            }

            foreach (var item in recipesDictionary)
            {
                Reduce(item.Value);
            }

            var addToDB = recipesDictionary.Values.ToArray();
            await System.IO.File.WriteAllBytesAsync("output.json", JsonSerializer.SerializeToUtf8Bytes(addToDB)).ConfigureAwait(false);
            return;
        }

        #endregion

        #region Craft Stuff
        public async Task<ulong[]> GetRecipeIdsByIngredientItemIdAsync(double itemId)
        {
            var allRecipes = await this._recipes.GetAllRecipes().ConfigureAwait(false);
            var recipes = allRecipes
                .Where(item =>
                    item.ingredients
                        .Where(ingredient => ingredient.itemId == Convert.ToUInt64(itemId)).Count() > 0);

            return recipes?.Select(item => item.id).ToArray() ?? Array.Empty<ulong>();
        }

        public async Task<ulong> GetRecipeIdByProductItemIdAsync(double itemId)
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
        public async Task<Recipe?> CraftItem(ulong recipeId, double playerId = 10000)
        {
            NQ.Recipe? recipe = this._marketBot.Bot.Recipes.FirstOrDefault(item => item.id == recipeId);

            if (recipe == null)
            {
                return null;
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
                return res;
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

            return res;
        }
        #endregion
    }
}
