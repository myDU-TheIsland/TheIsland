// <copyright file="LastRead.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.last_read")]
    public class LastRead
    {
        [ExplicitKey]
        public string table_name { get; set; } = string.Empty;

        public double table_id { get; set; } = 0;
    }
}
