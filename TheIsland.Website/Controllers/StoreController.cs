// <copyright file="StoreController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Models;

    public class StoreController : IslandController
    {
        public StoreController(PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
        }

        [HttpGet]
        public IActionResult Index(MarketModel model)
        {
            return this.View(model);
        }
    }
}
