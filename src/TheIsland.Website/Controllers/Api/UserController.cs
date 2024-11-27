// <copyright file="UserController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;

    [Area("Api")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserController : IslandController
    {
        private readonly DualPlayerRepository _playerRepository;

        public UserController(DualPlayerRepository playerRepository, PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._playerRepository = playerRepository;
        }

        public async Task<IActionResult> Index()
        {
            return this.Json(await this._playerRepository.GetAsync().ConfigureAwait(false));
        }

        public IActionResult Debug()
        {
            var output = new
            {
                claims = this.HttpContext?.User?.Claims?.Select(item => new KeyValuePair<string, string>(item.Type, item.Value)) ?? Array.Empty<KeyValuePair<string, string>>(),
                isAdmin = this.IsAdmin,
                isUser = this.IsUser,
            };

            return this.Json(output);
        }
    }
}
