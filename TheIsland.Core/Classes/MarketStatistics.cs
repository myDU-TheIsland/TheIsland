// <copyright file="MarketStatistics.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Classes
{
    using System.Text.Json.Serialization;

    public class MarketStatistics
    {
        [JsonIgnore]
        public string market_name { get; set; } = string.Empty;

        public DateTime DateTime { get; set; }

        public double market_id { get; set; } = 0;

        public double? total_quantity { get; set; } = 0;

        public double? transactions { get; set; } = 0;

        public double? average_price { get; set; } = 0;
    }
}
