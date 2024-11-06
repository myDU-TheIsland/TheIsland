// <copyright file="TestController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;

    [Area("Admin")]
    public class TestController : IslandController
    {
        private readonly MarketService _marketService;
        private readonly DualMarketRepository _dualMarketRepository;
        private readonly IGeneralBot _generalBot;
        private readonly IMarketBot _marketBot;
        private readonly DualMarketRepository _marketRepo;

        public TestController(
            MarketService marketService,
            DualMarketRepository dualMarketRepository,
            IGeneralBot generalBot,
            IMarketBot marketBot,
            DualMarketRepository marketRepo,
            PlayerLinkingService playerLinkingService,
            IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._marketService = marketService;
            this._dualMarketRepository = dualMarketRepository;
            this._generalBot = generalBot;
            this._marketBot = marketBot;
            this._marketRepo = marketRepo;
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> BuyStuff()
        {
            return this.Json(await this._marketBot.BuyStuff(0).ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> CancelBotOrders()
        {
            return this.Json(await this._marketService.CancelAllBotOrders().ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> SellStuff()
        {
            return this.Json(await this._marketService.SellAllMarketsContainerContents().ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> GetMarkets()
        {
            return this.Json(await this._dualMarketRepository.GetAsync().ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> HotTime()
        {
            return this.Json(await this._marketService.HotTimeEvent().ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> GetMarketName()
        {
            return this.Json(await this._marketBot.GetConstructName(170000).ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> SetMarketName()
        {
            return this.Json(await this._marketBot.SetConstructName(170000, "Aegis 123").ConfigureAwait(false));
        }
    }
}
