// <copyright file="UserMapping.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.user_mappings")]
    public class UserMapping : DatabaseEntity
    {
        public string discord_id { get; set; } = string.Empty;

        public double dual_id { get; set; } = 0;

        public string player_name { get; set; } = string.Empty;
    }
}
