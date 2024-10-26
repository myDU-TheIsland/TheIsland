// <copyright file="ViewStoreItemModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models
{
    using TheIsland.Core.Entities;

    public class ViewStoreItemModel
    {
        public string ErrorMessage { get; set; } = string.Empty;

        public StoreItem StoreItem { get; set; } = new StoreItem();
    }
}
