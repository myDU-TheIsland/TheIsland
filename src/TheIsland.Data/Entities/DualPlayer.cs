// <copyright file="DualPlayer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.player")]
    public class DualPlayer : DatabaseEntity
    {
        public bool connected { get; set; }

        public string display_name { get; set; } = string.Empty;

        public bool is_bot { get; set; } = false;

        public bool admin { get; set; } = false;

        public double wallet { get; set; } = 0;
    }
}
