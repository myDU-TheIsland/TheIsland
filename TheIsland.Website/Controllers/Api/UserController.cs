// <copyright file="UserController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;

    [Area("Api")]
    public class UserController : IslandController
    {
        private readonly DualPlayerRepository _playerRepository;
        private readonly UserMappingRepository _userMappingRepository;
        private readonly IGeneralBot _client;

        public UserController(DualPlayerRepository playerRepository, UserMappingRepository userMappingRepository, IGeneralBot client)
        {
            this._playerRepository = playerRepository;
            this._userMappingRepository = userMappingRepository;
            this._client = client;
        }

        public async Task<IActionResult> Index()
        {
            return this.Json(await this._playerRepository.GetAsync().ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> GiveQuantaToAll(double amount, string note)
        {
            return this.Json(await this._client.GiveAllQuanta(amount, note).ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> GiveTalentPointsToAll(double amount)
        {
            return this.Json(await this._client.GiveTalentPoints(amount).ConfigureAwait(false));
        }
    }
}
