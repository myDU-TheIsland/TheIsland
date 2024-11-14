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
    using TheIsland.Core.Helpers;
    using TheIsland.Core.Helpers.Caching;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using static TheIsland.Core.Helpers.ItemEntryExtensions;

    [Area("Api")]
    [Route("~/[area]/[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class DataController : IslandController
    {
        private readonly IMarketBot _marketBot;
        private readonly IGeneralBot _generalBot;

        public DataController(IMarketBot marketBot, IGeneralBot generalBot, PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._marketBot = marketBot;
            this._generalBot = generalBot;
        }

        [HttpGet]
        public IActionResult GetItems(string type = "JSON")
        {
            List<Core.Classes.ItemEntry> items = this._marketBot.GetAllItemsMarketEntries();
            switch (type)
            {
                case "csv":
                    byte[] bytes = Encoding.UTF8.GetBytes(@$"{ItemEntryCSVHeader()}" + "\r\n" + string.Join("\r\n", items.Select(item => item.ToCSVLine())));
                    return this.File(bytes, "text/csv", "items.csv");
                case "json":
                default:
                    return this.Json(items);
            }
        }

        [HttpGet]
        public IActionResult GetPlaceableItems()
        {
            var nonPlaceableGrandParentNames = new List<string>
            {
                "StructuralPart",
                "IntermediaryPart",
                "FunctionalPart",
                "ExceptionalPart",
                "ComplexPart",
                "RefinedMaterial",
                "ProductMaterial",
                "PureMaterial",
                "Part",
                "MineableMaterial",
                "OreMaterial",
                "PureHoneycomb",
                "ProductHoneycomb",
                "Fuel",
                "PlanetElement",
                "Ammo",
                "AmmoRailgunSmall",
                "AmmoRailgunMedium",
                "AmmoRailgunLarge",
                "AmmoRailgunExtraSmall",
                "AmmoMissileSmall",
                "AmmoMissileMedium",
                "AmmoMissileLarge",
                "AmmoMissileExtraSmall",
                "AmmoLaserSmall",
                "AmmoLaserMedium",
                "AmmoLaserLarge",
                "AmmoLaserExtraSmall",
                "AmmoCannonSmall",
                "AmmoCannonMedium",
                "AmmoCannonLarge",
                "AmmoCannonExtraSmall",
                "BaseItem",
            };
            var placeableItems = this._marketBot.GetAllItemsMarketEntries()
                .Where(item => !nonPlaceableGrandParentNames.Contains(item.GrandParentName ?? string.Empty))
                .ToList();
            return this.Json(placeableItems);
        }

        [HttpGet]
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
