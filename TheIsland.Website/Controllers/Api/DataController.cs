// <copyright file="DataController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using System.Linq;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Attributes;
    using TheIsland.Website.Framework.Helpers;

    [Area("Api")]
    public class DataController : IslandController
    {
        private readonly IMarketBot _marketBot;

        public DataController(IMarketBot marketBot, PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
            this._marketBot = marketBot;
        }

        public IActionResult GetBotPricesById(string type = "JSON", double market = 0)
        {
            type = type.ToLower();
            Dictionary<ulong, double> prices = new Dictionary<ulong, double>();

            if (!this._marketBot.MarketBudgetMultiplier.TryGetValue(Convert.ToUInt64(market), out double multi))
            {
                multi = 1;
            }

            foreach (var price in this._marketBot.BuyPrices)
            {
                prices.TryAdd(price.Key, Math.Ceiling((price.Value / 100) * multi));
            }

            switch (type)
            {
                case "csv":
                    byte[] bytes = Encoding.UTF8.GetBytes("item_id,price\r\n" + string.Join("\r\n", prices.Select(item => $@"{item.Key},{item.Value}")));
                    return this.File(bytes, "text/csv", "pricing.csv");
                case "json":
                default:
                    return this.Json(prices);
            }
        }
    }
}
