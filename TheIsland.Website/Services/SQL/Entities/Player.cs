// <copyright file="Player.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Services.SQL.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.player")]
    public class Player : DatabaseEntity
    {
        public bool Connected { get; set; }

        public string Display_Name { get; set; } = string.Empty;
    }
}
