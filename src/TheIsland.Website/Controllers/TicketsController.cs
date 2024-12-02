// <copyright file="TicketsController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Framework.Services;
    using TheIsland.Website.Classes;

    public class TicketsController : IslandController
    {
        public TicketsController(PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
        }

        public IActionResult Index()
        {
            return this.View();
        }
    }
}
