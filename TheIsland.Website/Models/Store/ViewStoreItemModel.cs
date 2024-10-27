// <copyright file="ViewStoreItemModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models.Store
{
    using TheIsland.Core.Entities;

    public class ViewStoreItemModel
    {
        public string ErrorMessage { get; set; } = string.Empty;

        public StoreItemEntity StoreItem { get; set; } = new StoreItemEntity();
    }
}
