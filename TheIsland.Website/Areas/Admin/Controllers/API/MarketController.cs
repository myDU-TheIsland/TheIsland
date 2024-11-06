// <copyright file="MarketController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers.API
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;

    [Area("Admin")]
    [Route("~/[area]/api/[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class MarketController : IslandController
    {
        private readonly IMarketBot _marketBot;

        public MarketController(IMarketBot marketBot, PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._marketBot = marketBot;
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

        [HttpGet]
        [ApiKey]
        public IActionResult GetConfigs()
        {
            return this.Json(this._marketBot.GetConfigs());
        }
    }
}
