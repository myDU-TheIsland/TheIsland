// <copyright file="AdminController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;

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
