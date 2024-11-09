// <copyright file="MarketController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers.API
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;

    [Area("Admin")]
    [Route("~/[area]/API/[controller]/[action]")]
    public class MarketController : IslandController
    {
        private readonly IMarketBot _marketBot;
        private readonly DualMarketRepository _dualMarketRepository;

        public MarketController(
            IMarketBot marketBot,
            PlayerLinkingService playerLinkingService,
            IAuthorizationService authorizationService,
            DualMarketRepository dualMarketRepository) : base(playerLinkingService, authorizationService)
        {
            this._marketBot = marketBot;
            this._dualMarketRepository = dualMarketRepository;
        }

        [HttpPost]
        [ApiKey]
        public IActionResult AddItemMultiplier(ulong marketId, string itemType, double value)
        {
            this._marketBot.SetItemMultiplier(marketId, itemType, value);
            return this.Json(true);
        }

        [HttpPost]
        [ApiKey]
        public IActionResult AddItemSellMultiplier(ulong marketId, string itemType, double value)
        {
            this._marketBot.SetItemSellMultiplier(marketId, itemType, value);
            return this.Json(true);
        }

        [HttpPost]
        [ApiKey]
        public IActionResult AddItemMultiplierRecursive(ulong marketId, string itemType, double value)
        {
            this._marketBot.SetItemMultiplierRecursive(marketId, itemType, value);
            return this.Json(true);
        }

        [HttpPost]
        [ApiKey]
        public IActionResult AddItemSellMultiplierRecursive(ulong marketId, string itemType, double value)
        {
            this._marketBot.SetItemSellMultiplierRecursive(marketId, itemType, value);
            return this.Json(true);
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> SetPricesToAllMarkets([FromBody] IEnumerable<PriceEntry> entries)
        {
            var markets = (await this._dualMarketRepository.GetAsync().ConfigureAwait(false))
                .ToList();

            if (markets.Count == 0)
            {
                return this.BadRequest("No Markets Found");
            }

            foreach (var entry in entries)
            {
                foreach (var market in markets)
                {
                    this._marketBot.SetItemMultiplierRecursive(
                        (ulong)market.id,
                        entry.ItemType,
                        entry.BuyFactor);
                    this._marketBot.SetItemSellMultiplierRecursive(
                        (ulong)market.id,
                        entry.ItemType,
                        entry.SellFactor);
                }
            }

            return this.Ok();
        }

        [HttpPost]
        [ApiKey]
        public IActionResult SetPricesToSingleMarket(ulong marketId, [FromBody] IEnumerable<PriceEntry> entries)
        {
            foreach (var entry in entries)
            {
                this._marketBot.SetItemMultiplierRecursive(
                    marketId,
                    entry.ItemType,
                    entry.BuyFactor);
                this._marketBot.SetItemSellMultiplierRecursive(
                    marketId,
                    entry.ItemType,
                    entry.SellFactor);
            }

            return this.Ok();
        }

        public class PriceEntry
        {
            public string ItemType { get; set; } = string.Empty;

            public double BuyFactor { get; set; }

            public double SellFactor { get; set; }
        }
    }
}