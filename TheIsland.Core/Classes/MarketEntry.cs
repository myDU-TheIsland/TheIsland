// <copyright file="MarketEntry.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>
namespace TheIsland.Core.Classes
{
    using System.Text.Json.Serialization;

    public class MarketEntry
    {
        public double Id { get; set; } = 0;

        [JsonIgnore]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public string Size { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public List<MarketEntry> Children { get; set; } = new List<MarketEntry>();
    }
}
