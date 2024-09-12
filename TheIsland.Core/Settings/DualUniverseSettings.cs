// <copyright file="DualUniverseSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Settings
{
    public class DualUniverseSettings
    {
        public string BotUser { get; set; } = string.Empty;

        public string BotPassword { get; set; } = string.Empty;

        public string IngameName { get; set; } = string.Empty;

        public string QueueingURL { get; set; } = "http://localhost:9630";
    }
}
