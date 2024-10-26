// <copyright file="AdminController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;
    using TheIsland.Website.Framework.Helpers;

    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class AdminController : IslandController
    {
        public AdminController(PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
        }

        [HttpGet]
        [Route("~/[area]")]
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
