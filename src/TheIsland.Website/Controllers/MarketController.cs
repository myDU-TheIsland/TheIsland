// <copyright file="MarketController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Models;

    public class MarketController : IslandController
    {
        public MarketController(PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
        }

        [HttpGet]
        [Route("~/Market/{itemId}/{marketId}/{search}/")]
        [Route("~/Market/{itemId}/{marketId}")]
        [Route("~/Market/{itemId}")]
        [Route("~/Market")]
        public IActionResult Index(MarketModel model)
        {
            return this.View(model);
        }
    }
}
