// <copyright file="StoreItemEntity.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using TheIsland.Core.Classes;

    public class StoreItem : StoreItemEntity
    {
    }

    [Table("public.store_items")]
    public class StoreItemEntity : DatabaseEntity
    {
        [Display(Name = "Item name")]
        public string name { get; set; } = string.Empty;

        [Column(TypeName = "jsonb")]
        public List<string> images { get; set; } = new List<string>();

        [Display(Name = "Description")]
        public string description { get; set; } = string.Empty;

        [Display(Name = "Purchase limit")]
        public double limit { get; set; } = 0;

        [Display(Name = "Price")]
        public double price { get; set; } = 0;

        public StoreItemContent content { get; set; } = new StoreItemContent();

        [Display(Name = "Is available for sale?")]
        public bool is_active { get; set; } = false;
    }
}
