// <copyright file="MarketBuyLimit.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.market_buy_limit")]
    public class MarketBuyLimit : DatabaseEntity
    {
        public string filter { get; set; } = string.Empty;

        public double quantity { get; set; } = 0;
    }
}
