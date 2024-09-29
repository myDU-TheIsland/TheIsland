// <copyright file="DualWalletTransaction.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.wallet_operation")]
    public class DualWalletTransaction : DatabaseEntity
    {
        public double entity_id { get; set; } = 0;

        public double peer_id { get; set; } = 0;

        public double amount { get; set; } = 0;

        public int operation_type { get; set; } = 0;

        public DateTime time { get; set; }

        public string payload { get; set; } = string.Empty;

        public double item_id { get; set; } = 0;

        public double market_id { get; set; } = 0;

        public double quantity { get; set; } = 0;
    }
}
