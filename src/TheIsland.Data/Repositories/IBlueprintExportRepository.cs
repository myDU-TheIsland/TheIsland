// <copyright file="IBlueprintExportRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Repositories
{
    using TheIsland.Data.Entities;

    public interface IBlueprintExportRepository : IEntityRepository<BluePrintExport>
    {
        Task<BluePrintExport?> GetByUuidAsync(Guid key, CancellationToken cancellationToken = default);

        Task<IEnumerable<BluePrintExport>> GetByPlayerIdAsync(double key, CancellationToken cancellationToken = default);
    }
}
