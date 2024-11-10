// <copyright file="MarketBotConfig.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Settings
{
    using System.Collections.Generic;

    /// <summary>
    /// A class containing the config settings for this mod.
    /// </summary>
    public class MarketBotConfig
    {
        /// <summary>
        /// Gets or sets list of items to buy and the top price to pay.
        /// </summary>
        public Dictionary<string, double> BuyPrices { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets list of items to recursively buy and the top price to pay.
        /// </summary>
        public Dictionary<string, double> BuyRecursivePrices { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets planets / parents to iterate over for markets.
        /// </summary>
        public ulong[] Planets { get; set; } = { 0 };

        /// <summary>
        /// Gets or sets the numbers days to buy before expiration.
        /// </summary>
        /// <example> 1 means once its goes under 24hrs remaining itll buy if within the buy range.</example>
        public int DaysToWaitBeforeExpiration { get; set; } = 1;

        /// <summary>
        /// Gets or sets the mark up for item resells.
        /// </summary>
        public double MarketMarkUp { get; set; } = 1.1;

        /// <summary>
        /// Gets or sets list of items to recursively buy and the top price to pay.
        /// </summary>
        public List<string> OnlyResellItemsRecursive { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets list of markets hot time events can happen at.
        /// </summary>
        public List<double> HotTimeMarkets { get; set; } = new List<double>() { 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137 };

        /// <summary>
        /// Gets or sets list of markets hot time events can happen at.
        /// </summary>
        public List<double> LimitBracket { get; set; } = new List<double>() { 0, 3, 6, 9, 12, 15, 18, 21 };

        public Dictionary<double, double> HotTimeMargins { get; set; } = new Dictionary<double, double>
        {
            { 2.0, 1 },
            { 1.9, 1 },
            { 1.8, 2 },
            { 1.8, 4 },
            { 1.6, 8 },
            { 1.5, 16 },
            { 1.45, 32 },
            { 1.40, 64 },
            { 1.35, 128 },
            { 1.30, 256 },
            { 1.25, 512 },
        };
    }
}