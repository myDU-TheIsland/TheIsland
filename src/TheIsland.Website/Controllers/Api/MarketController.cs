// <copyright file="MarketController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Helpers;

    [Area("Api")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Route("~/[area]/[controller]/[action]")]
    public class MarketController : IslandController
    {
        private readonly MarketService _marketService;

        public MarketController(
            MarketService marketService,
            PlayerLinkingService playerLinkingService,
            IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._marketService = marketService;
        }

        [HttpGet]
        [Route("{id}")]
        [Route("{id}/{marketId}")]
        public async Task<IActionResult> Hourly(double id, double marketId = -1, string type = "graph")
        {
            type ??= "graph";

            IEnumerable<MarketStatistics> results = await this._marketService.GetHourlyStats(id, marketId).ConfigureAwait(false);

            switch (type)
            {
                case "chartjs":
                    return this.Json(results.ToChartJS());
                case "graph":
                default:
                    return this.Json(results.ToGraph());
            }
        }

        [HttpGet]
        [Route("{id}")]
        [Route("{id}/{marketId}")]
        public async Task<IActionResult> Daily(double id, double marketId = -1, string type = "graph")
        {
            type ??= "graph";

            IEnumerable<MarketStatistics> results = await this._marketService.GetDailyStats(id, marketId).ConfigureAwait(false);

            switch (type)
            {
                case "chartjs":
                    return this.Json(results.ToChartJS());
                case "graph":
                default:
                    return this.Json(results.ToGraph());
            }
        }
    }
}
