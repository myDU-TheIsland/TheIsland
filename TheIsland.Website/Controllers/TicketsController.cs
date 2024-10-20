// <copyright file="TicketsController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;

    public class TicketsController : IslandController
    {
        public TicketsController(PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
        }

        public IActionResult Index()
        {
            return this.View();
        }
    }
}
