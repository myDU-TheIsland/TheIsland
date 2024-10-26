// <copyright file="StoreItemModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models.Admin
{
    using TheIsland.Core.Entities;

    public class StoreItemModel
    {
        public string ErrorMessage { get; set; } = string.Empty;

        public StoreItem StoreItem { get; set; } = new StoreItem();
    }
}
