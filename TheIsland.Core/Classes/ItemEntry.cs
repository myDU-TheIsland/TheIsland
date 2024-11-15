// <copyright file="ItemEntry.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>
namespace TheIsland.Core.Classes
{
    using System.Text.Json.Serialization;

    public class ItemEntry
    {
        [JsonIgnore]
        public double? GrandParentId { get; set; } = null;

        public string? GrandParentName { get; set; } = null;

        [JsonIgnore]
        public double ParentId { get; set; } = 0;

        public string ParentName { get; set; } = string.Empty;

        public double Id { get; set; } = 0;

        public string Name { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public long Tier { get; set; } = 0;

        public string Type { get; set; } = string.Empty;

        public string SubType { get; set; } = string.Empty;

        public double Volume { get; set; } = 0;

        public double Mass { get; set; } = 0;

        [JsonIgnore]
        public List<ItemEntry> Children { get; set; } = new List<ItemEntry>();
    }
}
