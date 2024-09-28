// <copyright file="MarketController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;
    using TheIsland.Website.Framework.Helpers;

    [Area("Api")]
    public class MarketController : IslandController
    {
        private readonly MarketService _marketService;
        private readonly IImportMarketService _importMarketService;
        private readonly DualMarketRepository _dualMarketRepository;
        private readonly IGeneralBot _generalBot;
        private readonly IMarketBot _marketBot;
        private readonly DualMarketRepository _marketRepo;

        public MarketController(
            IImportMarketService importMarketService,
            MarketService marketService,
            DualMarketRepository dualMarketRepository,
            IGeneralBot generalBot,
            IMarketBot marketBot,
            DualMarketRepository marketRepo)
        {
            this._importMarketService = importMarketService;
            this._marketService = marketService;
            this._dualMarketRepository = dualMarketRepository;
            this._generalBot = generalBot;
            this._marketBot = marketBot;
            this._marketRepo = marketRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Markets()
        {
            List<DualMarket> output = (await this._marketRepo.GetAsync().ConfigureAwait(false)).ToList();
            output.Add(new DualMarket { id = 0, name = "All Markets" });
            output = output.OrderBy(x => x.id).ToList();
            return this.Json(output.ToDictionary(key => key.name, value => value.id));
        }

        [HttpGet]
        public IActionResult Hierarchy()
        {
            return this.Json(this._generalBot.GetMarketHierarchy(false));
        }

        [HttpGet]
        public IActionResult ItemList()
        {
            Dictionary<string, double> output = this._generalBot.GetListOfSellableItems().ToArray().DistinctBy(item => item.Value).ToDictionary(key => key.Value, value => value.Key);
            return this.Json(output);
        }

        [HttpGet]
        [Route("Api/Market/Stats/Hourly/{id}")]
        public async Task<IActionResult> GetHourlySales(double id)
        {
            IEnumerable<MarketStatistics> results = await this._marketService.GetHourlyStats(id).ConfigureAwait(false);
            return this.Json(results.ToGraph());
        }

        [HttpGet]
        [ApiKey]
        public async Task<IActionResult> Import()
        {
            return this.Json(await this._importMarketService.ImportAsync().ConfigureAwait(false));
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
