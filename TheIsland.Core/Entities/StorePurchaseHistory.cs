// <copyright file="StorePurchaseHistory.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("public.purchase_history")]
    public class StorePurchaseHistory : DatabaseEntity
    {
        public double price { get; set; } = 0;

        public double quantity { get; set; } = 0;

        public DateTime timestamp { get; set; } = DateTime.UtcNow;

        public double player_id { get; set; } = 0;

        public string discord_user { get; set; } = string.Empty;

        public bool success { get; set; } = false;

        public StoreItem store_item { get; set; } = new StoreItem();

        public List<string> log { get; set; } = new List<string>();
    }
}
