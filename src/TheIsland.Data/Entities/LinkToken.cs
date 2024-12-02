// <copyright file="LinkToken.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.tokens")]
    public class LinkToken : DatabaseEntity
    {
        public string discord_id { get; set; } = string.Empty;

        public double player_id { get; set; } = 0;

        public string token { get; set; } = string.Empty;
    }
}
