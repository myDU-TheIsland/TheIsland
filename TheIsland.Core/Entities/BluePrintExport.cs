// <copyright file="BluePrintExport.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using Dapper.Contrib.Extensions;

    [Table("public.bp_exports")]
    public class BluePrintExport : DatabaseEntity
    {
        public Guid uuid { get; set; }

        public double player_id { get; set; } = 0;

        public double blueprint_id { get; set; } = 0;

        public string blueprint_name { get; set; } = string.Empty;

        public DateTime timestamp { get; set; } = DateTime.UtcNow;
    }
}
