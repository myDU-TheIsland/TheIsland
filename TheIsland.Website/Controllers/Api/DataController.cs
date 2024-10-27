// <copyright file="DataController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers.Api
{
    using System.Linq;
    using System.Text;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;

    [Area("Api")]
    public class DataController : IslandController
    {
        private readonly IMarketBot _marketBot;

        public DataController(IMarketBot marketBot, PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._marketBot = marketBot;
        }

        public IActionResult GetBotPricesById(string type = "JSON", double market = 0)
        {
            type = type.ToLower();
            Dictionary<ulong, double> prices = new Dictionary<ulong, double>();

            foreach (KeyValuePair<ulong, double> price in this._marketBot.BuyPrices)
            {
                prices.TryAdd(price.Key, this._marketBot.GetItemPrice(Convert.ToUInt64(market), price.Key) / 100);
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
