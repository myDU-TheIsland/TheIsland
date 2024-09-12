// <copyright file="SiteSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Classes
{
    using TheIsland.Core.Settings;
    using TheIsland.Website.Interfaces;

    public class SiteSettings : ISiteSettings
    {
        public DiscordSettings Discord { get; set; } = new DiscordSettings();

        public DualUniverseSettings DualUniverse { get; set; } = new DualUniverseSettings();

        public PostgresSettings Postgres { get; set; } = new PostgresSettings();
    }
}
