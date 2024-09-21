// <copyright file="MarketController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;
    using TheIsland.Website.Framework.Helpers;
    using TheIsland.Website.Models;

    [Area("Api")]
    public class MarketController : IslandController
    {
        private readonly MarketService _marketService;
        private readonly IImportMarketService _importMarketService;
        private readonly DualMarketRepository _dualMarketRepository;
        private readonly IDUClient _client;
        private readonly DualMarketRepository _marketRepo;

        public MarketController(
            IImportMarketService importMarketService,
            MarketService marketService,
            DualMarketRepository dualMarketRepository,
            IDUClient client,
            DualMarketRepository marketRepo)
        {
            this._importMarketService = importMarketService;
            this._marketService = marketService;
            this._dualMarketRepository = dualMarketRepository;
            this._client = client;
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
            return this.Json(this._client.GetMarketHierarchy(false));
        }

        [HttpGet]
        public IActionResult ItemList()
        {
            var output = this._client.GetItemsForSale().ToArray().DistinctBy(item => item.Value).ToDictionary(key => key.Value, value => value.Key);
            return this.Json(output);
        }

        [HttpGet]
        [Route("Api/Market/Stats/Hourly/{id}")]
        public async Task<IActionResult> GetHourlySales(double id)
        {
            var results = await this._marketService.GetHourlyStats(id).ConfigureAwait(false);
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
            return this.Json(await this._client.BuyStuff(0).ConfigureAwait(false));
        }

        [HttpGet]
        [ApiKey]
        public IActionResult MarketBotConfig()
        {
            return this.Json(this._client.MarketBotConfig());
        }
    }
}
