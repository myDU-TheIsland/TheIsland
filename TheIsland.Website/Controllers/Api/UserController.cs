// <copyright file="UserController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;

    [Area("Api")]
    public class UserController : IslandController
    {
        private readonly DualPlayerRepository _playerRepository;
        private readonly PlayerLinkingService _playerLinkingService;

        public UserController(DualPlayerRepository playerRepository, PlayerLinkingService playerLinkingService)
        {
            this._playerRepository = playerRepository;
            this._playerLinkingService = playerLinkingService;
        }

        public async Task<IActionResult> Index()
        {
            return this.Json(await this._playerRepository.GetAsync().ConfigureAwait(false));
        }
    }
}
