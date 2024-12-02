// <copyright file="FactoryLedgerEntry.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.factory_ledger")]
    public class FactoryLedgerEntry : DatabaseEntity
    {
        public double item_id { get; set; } = 0;

        public double quantity { get; set; } = 0;

        public double price { get; set; } = 0;
    }
}
