// <copyright file="DualMarket.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.market")]
    public class DualMarket : DatabaseEntity
    {
        public string name { get; set; } = string.Empty;

        public decimal value_tax { get; set; } = 0;
    }
}
