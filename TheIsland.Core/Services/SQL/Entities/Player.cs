// <copyright file="Player.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.player")]
    public class Player : DatabaseEntity
    {
        public bool connected { get; set; }

        public string display_name { get; set; } = string.Empty;
    }
}
