// <copyright file="UserController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;

    [Area("Api")]
    public class UserController : IslandController
    {
        private readonly DualPlayerRepository _playerRepository;

        public UserController(DualPlayerRepository playerRepository, PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
            this._playerRepository = playerRepository;
        }

        public async Task<IActionResult> Index()
        {
            return this.Json(await this._playerRepository.GetAsync().ConfigureAwait(false));
        }
    }
}
