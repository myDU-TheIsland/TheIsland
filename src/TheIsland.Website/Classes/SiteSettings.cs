// <copyright file="SiteSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Classes
{
    using TheIsland.Core.Settings;
    using TheIsland.Data.PostgreSQL.Settings;
    using TheIsland.Website.Interfaces;

    public class SiteSettings : ISiteSettings
    {
        public DiscordSettings Discord { get; set; } = new DiscordSettings();

        public DualUniverseSettings DualUniverse { get; set; } = new DualUniverseSettings();

        public BotSettings WebsiteBot { get; set; } = new BotSettings();

        public BotSettings MarketBot { get; set; } = new BotSettings();

        public PostgresSettings Postgres { get; set; } = new PostgresSettings();

        public string[] Admins { get; set; } = { };

        public string[] ApiKey { get; set; } = { "5108b23c-9fec-497e-88a6-07f9d3032dcc" };

        public string RedisServer { get; set; } = "10.10.42.169";
    }
}
