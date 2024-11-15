// <copyright file="CraftedItem.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.crafted_items")]
    public class CraftedItem : DatabaseEntity
    {
        public Dictionary<double, double> crafting_requirements { get; set; } = new Dictionary<double, double>();
    }
}
