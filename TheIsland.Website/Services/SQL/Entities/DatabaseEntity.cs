// <copyright file="DatabaseEntity.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Services.SQL.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class DatabaseEntity
    {
        [Column(name: "id")]
        [Key]
        public double Id { get; set; } = 0;
    }
}
