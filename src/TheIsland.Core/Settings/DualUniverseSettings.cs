// <copyright file="DualUniverseSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Settings
{
    public class DualUniverseSettings
    {
        public string ExportPath { get; set; } = string.Empty;

        public string ConfigPath { get; set; } = string.Empty;

        public string StoreImagePath { get; set; } = string.Empty;

        public string StoreImageUrl { get; set; } = string.Empty;

        public ulong[] MarketHeaderIds { get; set; } = { };

        public ulong[] Markets { get; set; } = { };

        public BotSettings WebsiteBot { get; set; } = new BotSettings();

        public BotSettings MarketBot { get; set; } = new BotSettings();
    }
}
