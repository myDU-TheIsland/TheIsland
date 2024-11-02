// <copyright file="AdminController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers.API
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;

    [Area("Admin")]
    [Route("~/[area]/API/[action]")]
    public class AdminController : IslandController
    {
        private readonly IGeneralBot _generalBot;

        public AdminController(IGeneralBot generalBot, PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._generalBot = generalBot;
        }

        [HttpGet]
        [ApiKey]
        public IActionResult GetAllItems()
        {
            return this.Json(this._generalBot.GetAllItems());
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> RespecEntireCategoryForAll(string category)
        {
            await this._generalBot.RespecEntireCategoryForAllPlayers(category).ConfigureAwait(false);
            return this.Json(true);
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> GiveQuantaToAll(double amount, string note)
        {
            return this.Json(await this._generalBot.GiveAllQuanta(amount, note).ConfigureAwait(false));
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> GiveTalentPointsToAll(double amount)
        {
            return this.Json(await this._generalBot.GiveTalentPoints(amount).ConfigureAwait(false));
        }
    }
}
