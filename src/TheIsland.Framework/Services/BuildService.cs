// <copyright file="BuildService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Framework.Services
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Backend;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Toolkit.HighPerformance;
    using NQ;
    using NQ.Interfaces;
    using NQutils.Def;
    using TheIsland.Core.Interfaces;
    using TheIsland.Data.Classes;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;
    using TheIsland.Framework.Bots;

    public class BuildService : IAppService
    {
        private readonly IMarketBot _marketBot;
        private readonly IRecipes _recipes;
        private readonly IGameplayBank _gameplayBank;

        private readonly IFactoryLedgerRepository _factoryLedgerRepository;
        private readonly ICraftedItemRepository _craftedItemRepository;

        public BuildService(
            IFactoryLedgerRepository factoryLedgerRepository,
            ICraftedItemRepository craftedItemRepository,
            IMarketBot dualClient)
        {
            this._factoryLedgerRepository = factoryLedgerRepository;
            this._craftedItemRepository = craftedItemRepository;

            this._marketBot = dualClient;
            this._gameplayBank = this._marketBot.ServiceProvider.GetRequiredService<IGameplayBank>();
            this._recipes = this._marketBot.ServiceProvider.GetRequiredService<IRecipes>();
        }

        #region Factory Stuff
        public async Task FactoryConvertOreIntoPure()
        {
            List<ItemEntry> ores = this._marketBot.GetAllItemsMarketEntries().Where(item => item.Type == "Ore").ToList();
            List<FactoryLedgerEntry> oresDB = (await this._factoryLedgerRepository.GetAllEntriesByItemIdsAsync(ores.Select(item => item.Id).ToArray()).ConfigureAwait(false)).ToList();

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

            List<FactoryLedgerEntry> pures = new List<FactoryLedgerEntry>();

            foreach (FactoryLedgerEntry ore in totals)
            {
                (Recipe? recipe, ulong batchSize) recipe = await this.CraftItem((await this.GetRecipeIdsByIngredientItemIdAsync(ore.item_id).ConfigureAwait(false)).First()).ConfigureAwait(false);
                if (recipe.recipe == null)
                {
                    //remove this ore from the list
                    oresDB = oresDB.Where(item => item.item_id != ore.item_id).ToList();
                    continue;
                }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                double multiplier = ore.quantity / recipe.recipe.ingredients.First().quantity.ToVolume();

                for (int i = 0; i < (recipe.recipe?.products.Count ?? 0); i++)
                {
                    Ingredient item = recipe.recipe.products[i];

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

        public async Task<CraftedItem[]> CraftItems()
        {
            ConcurrentDictionary<double, CraftedItem> recipesDictionary = new ConcurrentDictionary<double, CraftedItem>();
            Dictionary<double, ItemEntry> marketEntries = this._marketBot.GetAllItemsMarketEntries().ToDictionary(key => key.Id, value => value);
            List<ItemEntry> pures = marketEntries.Values.Where(item => item.Type == "Pure" || item.SubType == "Pure").ToList();

            double GetVolume(double itemId)
            {
                if (marketEntries.TryGetValue(itemId, out ItemEntry? output))
                {
                    return output.Volume;
                }
                else
                {
                    return 1;
                }
            }

            double GetQuantity(Ingredient ingredient)
            {
                double quan = Math.Round(ingredient.quantity.ToVolume() / GetVolume(ingredient.itemId), 2);
                return quan == 0 ? ingredient.quantity.value : quan;
            }

            bool isPure(double itemId)
            {
                return pures.Select(item => item.Id).Contains(itemId);
            }

            bool isOnlyPure(CraftedItem item)
            {
                return item.crafting_requirements.Keys.All(isPure);
            }

            Queue<ulong> queue = new Queue<ulong>(this._marketBot.Bot.Recipes.Select(item => item.id).ToArray());

            while (queue.Count != 0)
            {
                ulong queueId = queue.Dequeue();

                (Recipe? recipe, ulong batchSize) craftedItem = await this.CraftItem(queueId, 10000).ConfigureAwait(false);

                if (craftedItem.recipe == null)
                {
                    continue;
                }

                ulong itemId = craftedItem.recipe.products[0].itemId;

                if (!marketEntries.ContainsKey(itemId) || isPure(itemId))
                {
                    //probably a hidden item or pure.
                    continue;
                }

                double quantityProduced = GetQuantity(craftedItem.recipe.products[0]);

                CraftedItem craftedItemEntry = new CraftedItem()
                {
                    item_id = itemId,
                    crafting_requirements = craftedItem.recipe.ingredients
                            .ToDictionary(
                                key => Convert.ToDouble(key.itemId),
                                value => Math.Round(GetQuantity(value) / quantityProduced, 2)),
                };

                recipesDictionary.TryAdd(itemId, craftedItemEntry);
            }

            void Reduce(CraftedItem item)
            {
                bool isPureOnly = item.crafting_requirements.All(item => isPure(item.Key));
                List<double> items = item.crafting_requirements.Keys.ToList();
                while (!isPureOnly)
                {
                    foreach (double entry in items)
                    {
                        double quantity = item.crafting_requirements[entry];

                        if (double.IsNaN(quantity) || double.IsInfinity(quantity))
                        {
                            quantity = 1;
                        }

                        if (!isPure(entry))
                        {
                            item.crafting_requirements.Remove(entry);
                        }
                        else
                        {
                            item.crafting_requirements[entry] = Math.Round(item.crafting_requirements[entry], 2);
                        }

                        if (recipesDictionary.TryGetValue(entry, out CraftedItem? value))
                        {
                            if (!isOnlyPure(value))
                            {
                                Reduce(value);
                            }

                            foreach (KeyValuePair<double, double> newIngredient in value.crafting_requirements)
                            {
                                double ingredientValue = newIngredient.Value;
                                if (double.IsNaN(ingredientValue) || double.IsInfinity(ingredientValue))
                                {
                                    ingredientValue = 1;
                                }

                                double quantityRequired = Math.Round(ingredientValue * quantity, 2);

                                if (!item.crafting_requirements.TryAdd(newIngredient.Key, quantityRequired))
                                {
                                    item.crafting_requirements[newIngredient.Key] = Math.Round(item.crafting_requirements[newIngredient.Key] + quantityRequired, 2);
                                }
                            }
                        }
                    }

                    items = item.crafting_requirements.Keys.ToList();
                    isPureOnly = item.crafting_requirements.All(item => isPure(item.Key));
                }
            }

            foreach (KeyValuePair<double, CraftedItem> item in recipesDictionary)
            {
                Reduce(item.Value);
            }

            CraftedItem[] addToDB = recipesDictionary.Values.ToArray();

            foreach (CraftedItem? databaseEntry in addToDB)
            {
                try
                {
                    if (databaseEntry.crafting_requirements.Any(req => double.IsNaN(req.Value) || double.IsInfinity(req.Value)))
                    {
                        continue;
                    }

                    await this._craftedItemRepository.AddAsync(databaseEntry).ConfigureAwait(false);
                }
                catch
                {
                    //do nothing
                    continue;
                }
            }

            return (await this._craftedItemRepository.GetAsync().ConfigureAwait(false)).ToArray();
        }

        #endregion

        #region Craft Stuff
        public async Task<ulong[]> GetRecipeIdsByIngredientItemIdAsync(double itemId)
        {
            List<Recipe> allRecipes = await this._recipes.GetAllRecipes().ConfigureAwait(false);
            IEnumerable<Recipe> recipes = allRecipes
                .Where(item =>
                    item.ingredients
                        .Where(ingredient => ingredient.itemId == Convert.ToUInt64(itemId)).Count() > 0);

            return recipes?.Select(item => item.id).ToArray() ?? Array.Empty<ulong>();
        }

        public async Task<ulong> GetRecipeIdByProductItemIdAsync(double itemId)
        {
            List<Recipe> recipes = await this._recipes.GetAllRecipes().ConfigureAwait(false);
            Recipe? recipe = recipes.FirstOrDefault(item => item.products[0].itemId == Convert.ToUInt64(itemId));
            return recipe?.id ?? 0;
        }

        /// <summary>
        /// Gets the recipe results with talents applied for recipe provided.
        /// </summary>
        /// <param name="recipeId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        /// <remarks>Duplicated from Orleans IndustryUnitGrain.cs.</remarks>
        public async Task<(Recipe? recipe, ulong batchSize)> CraftItem(ulong recipeId, double playerId = 10000)
        {
            Recipe? recipe = this._marketBot.Bot.Recipes.FirstOrDefault(item => item.id == recipeId);

            if (recipe == null)
            {
                return (null, 0UL);
            }

            Recipe res = new Recipe()
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
            res.time = (float)EffectSystem.ApplyModifiers((double)res.time, EffectSystem.RegroupModifiers(await talent.Bonuses(itemId, true).ConfigureAwait(false)).GetValueOrDefault("industryEfficiency"));
            if ((double)res.time < 1.0)
            {
                res.time = 1f;
            }

            Dictionary<string, GameplayModifiers> dictionary = EffectSystem.RegroupModifiers(mainProductModifiers);
            res.time = (float)EffectSystem.ApplyModifiers((double)res.time, dictionary.GetValueOrDefault("craftingDurationMul"));
            if ((double)res.time < 1.0)
            {
                res.time = 1f;
            }

            double requiredAdd = EffectSystem.ApplyModifiers(0.0, dictionary.GetValueOrDefault("requiredMaterialAdd"));
            double requiredMul = EffectSystem.ApplyModifiers(1.0, dictionary.GetValueOrDefault("requiredMaterialMul"));

            foreach (Ingredient? ingredient in res.ingredients)
            {
                ingredient.quantity = (ItemQuantity)(long)((ingredient.quantity.value + requiredAdd) * requiredMul);
            }

            double productAdd = EffectSystem.ApplyModifiers(0.0, dictionary.GetValueOrDefault("productMaterialAdd"));
            double productMul = EffectSystem.ApplyModifiers(1.0, dictionary.GetValueOrDefault("productMaterialMul"));

            foreach (Ingredient? product in res.products)
            {
                product.quantity = (ItemQuantity)(long)((product.quantity.value + productAdd) * productMul);
            }

            talent = null;
            mainProductModifiers = null;

            res.time *= 1;
            if ((double)res.time < 1.0)
            {
                res.time = 1f;
            }

            long minRecipeTime = this._gameplayBank.GetBaseObject<IndustryConfig>().MinRecipeTime;
            if ((double)res.time >= minRecipeTime)
            {
                return (res, 1UL);
            }

            long num = minRecipeTime / (long)res.time;

            res.time *= num;
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
