// <copyright file="StoreItem.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Dapper.Contrib.Extensions;
    using TheIsland.Core.Classes;

    [Table("public.store_items")]
    public class StoreItem : DatabaseEntity
    {
        [Display(Name = "Item Name")]
        public string name { get; set; } = string.Empty;

        public string[] images { get; set; } = Array.Empty<string>();

        [Display(Name = "Description")]
        public string description { get; set; } = string.Empty;

        [Display(Name = "Purchase Limit")]
        public double limit { get; set; } = 0;

        [Display(Name = "Price")]
        public double price { get; set; } = 0;

        public StoreItemContent content { get; set; } = new StoreItemContent();

        [Display(Name = "IsActive?")]
        public bool is_active { get; set; } = false;
    }
}
