// <copyright file="IStorePurchaseHistoryRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IStorePurchaseHistoryRepository : IEntityRepository<StorePurchaseHistory>
    {
        Task<IEnumerable<StorePurchaseHistory>> GetPlayerPurchaseHistory(double playerId);
    }
}
