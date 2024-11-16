// <copyright file="TestController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers
{
    using System.Text.Json;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;

    [Area("Admin")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Route("~/[area]/[controller]/[action]")]
    public class TestController : IslandController
    {
        private readonly BuildService _buildService;
        private readonly MarketService _marketService;
        private readonly DualMarketRepository _dualMarketRepository;
        private readonly IGeneralBot _generalBot;
        private readonly IMarketBot _marketBot;
        private readonly DualMarketRepository _marketRepo;

        public TestController(
            BuildService buildService,
            MarketService marketService,
            DualMarketRepository dualMarketRepository,
            IGeneralBot generalBot,
            IMarketBot marketBot,
            DualMarketRepository marketRepo,
            PlayerLinkingService playerLinkingService,
            IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._buildService = buildService;
            this._marketService = marketService;
            this._dualMarketRepository = dualMarketRepository;
            this._generalBot = generalBot;
            this._marketBot = marketBot;
            this._marketRepo = marketRepo;
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> PriceItems()
        {
            return this.Json(await this._buildService.CraftItems().ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> FactoryConvertOreIntoPure()
        {
            await this._buildService.FactoryConvertOreIntoPure().ConfigureAwait(false);
            return this.Json(true);
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> CraftItems()
        {
            return this.Json(await this._buildService.CraftItems().ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> BuyStuff()
        {
            return this.Json(await this._marketBot.BuyStuffAsync(0).ConfigureAwait(false));
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
            return this.Json(await this._marketBot.GetConstructNameAsync(170000).ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> SetMarketName()
        {
            return this.Json(await this._marketBot.SetConstructNameAsync(170000, "Aegis 123").ConfigureAwait(false));
        }
    }
}
