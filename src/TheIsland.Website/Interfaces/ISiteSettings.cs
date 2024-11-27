// <copyright file="ISiteSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Interfaces
{
    using TheIsland.Core.Settings;
    using TheIsland.Website.Classes;

    public interface ISiteSettings
    {
        string[] Admins { get; set; }

        string[] ApiKey { get; set; }

        string DPKPath { get; set; }

        DiscordSettings Discord { get; set; }

        DualUniverseSettings DualUniverse { get; set; }

        PostgresSettings Postgres { get; set; }
    }
}
