// <copyright file="StoreItemExtensions.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Helpers
{
    using TheIsland.Core.Entities;

    public static class StoreItemExtensions
    {
        public static StoreItem ToStoreItem(this StoreItemEntity input)
        {
            return new StoreItem
            {
                id = input.id,
                name = input.name,
                images = input.images,
                description = input.description,
                limit = input.limit,
                price = input.price,
                content = input.content,
                is_active = input.is_active,
            };
        }
    }
}
