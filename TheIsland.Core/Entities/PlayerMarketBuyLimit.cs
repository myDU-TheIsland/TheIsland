// <copyright file="PlayerMarketBuyLimit.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.player_market_buy_limit")]
    public class PlayerMarketBuyLimit : DatabaseEntity
    {
        public double player_id { get; set; } = 0;

        public double market_id { get; set; } = 0;

        public double item_id { get; set; } = 0;

        public DateTime start_time { get; set; } = DateTime.Now;

        public DateTime end_time { get; set; } = DateTime.Now.AddHours(3);

        public double quantity { get; set; } = 0;
    }
}
