// <copyright file="DualMarket.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL.Entities
{
    public class DualMarket : DatabaseEntity
    {
        public string name { get; set; } = string.Empty;

        public double construct_id { get; set; } = 0;

        public decimal value_tax { get; set; } = 0;
    }
}
