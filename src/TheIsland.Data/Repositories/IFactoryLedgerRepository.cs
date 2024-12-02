// <copyright file="IFactoryLedgerRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IFactoryLedgerRepository : IEntityRepository<FactoryLedgerEntry>
    {
        Task<IEnumerable<FactoryLedgerEntry>> GetAllEntriesByItemIdsAsync(double[] itemIds, CancellationToken cancellationToken = default);
    }
}
