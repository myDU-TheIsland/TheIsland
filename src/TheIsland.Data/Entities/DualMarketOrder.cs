// <copyright file="DualMarketOrder.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.market_order")]
    public class DualMarketOrder : DatabaseEntity
    {
        public double buy_quantity { get; set; }

        public int status { get; set; }

        public DateTime creation_date { get; set; }

        public DateTime completion_date { get; set; }

        public DateTime expiration_date { get; set; }

        public double price { get; set; }

        public double value_tax { get; set; }

        public double item_type_id { get; set; }

        public double market_id { get; set; }

        public double original_buy_quantity { get; set; }

        public DateTime update_date { get; set; }

        public double owner_id { get; set; }

        public double player_id { get; set; }

        public double flat_tax { get; set; }

        public double storage_fee { get; set; }
    }
}
