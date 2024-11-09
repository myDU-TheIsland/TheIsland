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
    [Route("~/[area]/api/[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = false)]
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
        public async Task<IActionResult> AddItemMultiplier(ulong marketId, string itemType, double value)
        {
            await this._marketBot.SetItemMultiplier(marketId, itemType, value).ConfigureAwait(false);
            return this.Json(true);
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> AddItemSellMultiplier(ulong marketId, string itemType, double value)
        {
            await this._marketBot.SetItemSellMultiplier(marketId, itemType, value).ConfigureAwait(false);
            return this.Json(true);
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> AddItemMultiplierRecursive(ulong marketId, string itemType, double value)
        {
            await this._marketBot.SetItemMultiplierRecursive(marketId, itemType, value).ConfigureAwait(false);
            return this.Json(true);
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> AddItemSellMultiplierRecursive(ulong marketId, string itemType, double value)
        {
            await this._marketBot.SetItemSellMultiplierRecursive(marketId, itemType, value).ConfigureAwait(false);
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
                    await this._marketBot.SetItemMultiplierRecursive(
                        (ulong)market.id,
                        entry.ItemType,
                        entry.BuyFactor).ConfigureAwait(false);
                    await this._marketBot.SetItemSellMultiplierRecursive(
                        (ulong)market.id,
                        entry.ItemType,
                        entry.SellFactor).ConfigureAwait(false);
                }
            }

            return this.Ok();
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> SetPricesToSingleMarket(ulong marketId, [FromBody] IEnumerable<PriceEntry> entries)
        {
            foreach (var entry in entries)
            {
                await this._marketBot.SetItemMultiplierRecursive(
                    marketId,
                    entry.ItemType,
                    entry.BuyFactor).ConfigureAwait(false);
                await this._marketBot.SetItemSellMultiplierRecursive(
                    marketId,
                    entry.ItemType,
                    entry.SellFactor).ConfigureAwait(false);
            }

            return this.Ok();
        }

        [HttpGet]
        [ApiKey]
        public IActionResult GetConfigs()
        {
            return this.Json(this._marketBot.GetConfigs());
        }

        public class PriceEntry
        {
            public string ItemType { get; set; } = string.Empty;

            public double BuyFactor { get; set; }

            public double SellFactor { get; set; }
        }
    }
}