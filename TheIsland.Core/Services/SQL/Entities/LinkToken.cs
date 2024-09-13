// <copyright file="LinkToken.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.tokens")]
    public class LinkToken : DatabaseEntity
    {
        public double discord_id { get; set; } = 0;

        public double player_id { get; set; } = 0;

        public string token { get; set; } = string.Empty;
    }
}
