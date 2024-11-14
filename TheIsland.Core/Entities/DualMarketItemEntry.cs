// <copyright file="DualMarketItemEntry.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.market_item_entry")]
    public class DualMarketItemEntry : DatabaseEntity
    {
        public double quantity { get; set; } = 0;

        public double item_type_id { get; set; } = 0;

        public double market_id { get; set; } = 0;

        public double order_id { get; set; } = 0;

        public double owner_id { get; set; } = 0;
    }
}
