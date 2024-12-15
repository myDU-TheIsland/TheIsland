// <copyright file="ISiteSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Interfaces
{
    using TheIsland.Core.Settings;
    using TheIsland.Website.Classes;

    public interface ISiteSettings
    {
        DiscordSettings Discord { get; set; }

        DualUniverseSettings DualUniverse { get; set; }

        BotSettings WebsiteBot { get; set; }

        BotSettings MarketBot { get; set; }

        string[] Admins { get; set; }

        string[] ApiKey { get; set; }

        string RedisServer { get; set; }

        bool BackgroundBuy { get; set; }

        bool BackgroundHotTime { get; set; }
    }
}
