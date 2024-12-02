// <copyright file="MarketTransaction.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Entities
{
    using System;
    using Dapper.Contrib.Extensions;

    public enum TransactionType
    {
        BuyOrder = 1,
        SellOrder = 2,
        Buy = 3,
        Sell = 4,
    }

    [Table("public.market_transactions")]
    public class MarketTransaction : DatabaseEntity
    {
        public TransactionType type { get; set; }

        public DateTime creation_date { get; set; }

        public double item_id { get; set; }

        public double price { get; set; }

        public double quantity { get; set; }

        public double market_id { get; set; }
    }
}
