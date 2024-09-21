// <copyright file="MarketController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Models;

    public class MarketController : IslandController
    {
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
