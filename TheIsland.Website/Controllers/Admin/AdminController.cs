// <copyright file="AdminController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Admin
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;
    using TheIsland.Website.Framework.Helpers;

    [Area("Admin")]
    public class AdminController : IslandController
    {
        private readonly IGeneralBot _generalBot;

        public AdminController(IGeneralBot generalBot, PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
            this._generalBot = generalBot;
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> GiveQuantaToAll(double amount, string note)
        {
            return this.Json(await this._generalBot.GiveAllQuanta(amount, note).ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> GiveTalentPointsToAll(double amount)
        {
            return this.Json(await this._generalBot.GiveTalentPoints(amount).ConfigureAwait(false));
        }
    }
}
