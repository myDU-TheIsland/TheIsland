// <copyright file="BotSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Settings
{
    public class BotSettings
    {
        public string BotUser { get; set; } = string.Empty;

        public string BotPassword { get; set; } = string.Empty;

        public string PlayerName { get; set; } = string.Empty;

        public string QueueingUri { get; set; } = "http://queueing:9630";
    }
}
