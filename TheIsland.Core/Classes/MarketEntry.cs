// <copyright file="MarketEntry.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>
namespace TheIsland.Core.Classes
{
    using System.Text.Json.Serialization;

    public class MarketEntry
    {
        [JsonIgnore]
        public double? GrandParentId { get; set; } = null;

        public string? GrandParentName { get; set; } = null;

        [JsonIgnore]
        public double ParentId { get; set; } = 0;

        public string ParentName { get; set; } = string.Empty;

        public double Id { get; set; } = 0;

        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public string Size { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        [JsonIgnore]
        public List<MarketEntry> Children { get; set; } = new List<MarketEntry>();
    }
}
