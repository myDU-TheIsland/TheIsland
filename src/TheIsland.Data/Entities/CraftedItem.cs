// <copyright file="CraftedItem.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.crafted_items")]
    public class CraftedItem : DatabaseEntity
    {
        public double item_id { get; set; } = 0;

        public Dictionary<double, double> crafting_requirements { get; set; } = new Dictionary<double, double>();
    }
}
