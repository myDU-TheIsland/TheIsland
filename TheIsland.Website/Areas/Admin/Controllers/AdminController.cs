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
        public AdminController(PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
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