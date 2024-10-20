// <copyright file="MarketController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
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
            DualMarketRepository marketRepo,
            PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
            this._importMarketService = importMarketService;
            this._marketService = marketService;
            this._dualMarketRepository = dualMarketRepository;
            this._generalBot = generalBot;
            this._marketBot = marketBot;
            this._marketRepo = marketRepo;
        }

        [HttpGet]
        [Route("Api/Market/Stats/Hourly/{id}")]
        [Route("Api/Market/Stats/Hourly/{id}/{marketId}")]
        public async Task<IActionResult> GetHourlySales(double id, double marketId = -1)
        {
            IEnumerable<MarketStatistics> results = await this._marketService.GetHourlyStats(id, marketId).ConfigureAwait(false);
            return this.Json(results.ToGraph());
        }
    }
}
