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
    [Route("~/[area]/API/[controller]/[action]")]
    public class MarketController : IslandController
    {
        private readonly IMarketBot _marketBot;

        public MarketController(IMarketBot marketBot, PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._marketBot = marketBot;
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
    }
}
