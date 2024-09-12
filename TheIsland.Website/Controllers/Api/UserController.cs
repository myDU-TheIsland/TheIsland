// <copyright file="UserController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Classes;
    using TheIsland.Website.Services.SQL;

    [Area("Api")]
    public class UserController : Controller
    {
        private readonly PlayerRepository _playerRepository;
        private readonly UserMappingRepository _userMappingRepository;

        public UserController(PlayerRepository playerRepository, UserMappingRepository userMappingRepository)
        {
            this._playerRepository = playerRepository;
            this._userMappingRepository = userMappingRepository;
        }

        public async Task<IActionResult> Index()
        {
            return this.Json(await this._playerRepository.GetAsync().ConfigureAwait(false));
        }
    }
}
