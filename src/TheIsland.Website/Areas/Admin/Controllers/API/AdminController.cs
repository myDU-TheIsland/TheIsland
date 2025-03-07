// <copyright file="AdminController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers.API
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Framework.Bots;
    using TheIsland.Framework.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;

    [Area("Admin")]
    [Route("~/[area]/actions/[action]")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class AdminController : IslandController
    {
        private readonly IGeneralBot _generalBot;
        private readonly IMarketBot _marketBot;

        public AdminController(IMarketBot marketBot, IGeneralBot generalBot, PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._generalBot = generalBot;
            this._marketBot = marketBot;
        }

        [HttpGet]
        [ApiKey]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetAllItems()
        {
            return this.Json(this._marketBot.GetAllItems());
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
        public async Task<IActionResult> GiveQuantaToAll(double amount, string note, double hours = 1)
        {
            return this.Json(await this._generalBot.GiveAllQuanta(amount, note, hours).ConfigureAwait(false));
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> GiveTalentPointsToAll(double amount)
        {
            return this.Json(await this._generalBot.GiveTalentPoints(amount).ConfigureAwait(false));
        }
    }
}
