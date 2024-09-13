// <copyright file="UserMapping.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.user_mappings")]
    public class UserMapping : DatabaseEntity
    {
        public double discord_id { get; set; } = 0;

        public double dual_id { get; set; } = 0;
    }
}
