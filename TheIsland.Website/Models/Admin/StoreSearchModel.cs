// <copyright file="StoreSearchModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models.Admin
{
    using TheIsland.Core.Entities;

    public class StoreSearchModel
    {
        public string ErrorMessage { get; set; } = string.Empty;

        public StoreItem[] StoreItems { get; set; } = Array.Empty<StoreItem>();

        public string Name { get; set; } = string.Empty;
    }
}
