// <copyright file="DualMarketTransaction.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.market_order")]
    public class DualMarketTransaction : DatabaseEntity
    {
        public double original_buy_quantity { get; set; }

        public bool status { get; set; }

        public DateTime creation_date { get; set; }

        public DateTime? completion_date { get; set; }

        public DateTime expiration_date { get; set; }

        public double price { get; set; }

        public double item_type_id { get; set; }

        public double market_id { get; set; }

        public double owner_id { get; set; }
    }
}
