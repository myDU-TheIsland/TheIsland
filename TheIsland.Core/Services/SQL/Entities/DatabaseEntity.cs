// <copyright file="DatabaseEntity.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL.Entities
{
    using Dapper.Contrib.Extensions;

    public class DatabaseEntity
    {
        [Key]
        public double Id { get; set; } = 0;
    }
}
