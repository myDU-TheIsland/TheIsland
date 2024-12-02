// <copyright file="StoreSearchModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models.Store
{
    using TheIsland.Data.Entities;

    public class StoreSearchModel
    {
        public string ErrorMessage { get; set; } = string.Empty;

        public StoreItemEntity[] StoreItems { get; set; } = Array.Empty<StoreItemEntity>();

        public string Name { get; set; } = string.Empty;
    }
}
