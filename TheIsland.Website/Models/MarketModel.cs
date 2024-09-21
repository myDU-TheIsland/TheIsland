// <copyright file="MarketModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models
{
    using Microsoft.AspNetCore.Mvc;

    public enum MarketSearchType
    {
        Daily,
        Hourly,
    }

    public class MarketModel
    {
        [FromRoute(Name = "marketId")]
        public double MarketId { get; set; } = 0;

        [FromRoute(Name = "itemId")]
        public double ItemId { get; set; } = 0;

        [FromRoute(Name = "search")]
        public MarketSearchType SearchType { get; set; } = MarketSearchType.Daily;
    }
}
