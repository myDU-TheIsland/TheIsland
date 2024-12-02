// <copyright file="MarketStock.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.market_stock")]
    public class MarketStock : DatabaseEntity
    {
        public double item_id { get; set; } = 0;

        public double quantity { get; set; } = 0;

        public bool is_magic { get; set; } = false;
    }
}
