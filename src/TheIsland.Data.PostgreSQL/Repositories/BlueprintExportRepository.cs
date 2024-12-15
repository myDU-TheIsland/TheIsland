// <copyright file="BlueprintExportRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Interfaces;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;

    public class BlueprintExportRepository : NpgsqlEntityRepository<BluePrintExport>, IBlueprintExportRepository
    {
        public BlueprintExportRepository(IDatabaseSettings settings) : base(settings, settings.Database)
        {
        }

        public async Task<BluePrintExport?> GetByUuidAsync(Guid key, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryFirstOrDefaultAsync<BluePrintExport>("SELECT * FROM public.bp_exports WHERE uuid = @Uuid;", new { Uuid = key }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<BluePrintExport>> GetByPlayerIdAsync(double key, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<BluePrintExport>("SELECT * FROM public.bp_exports WHERE player_id = @PlayerId;", new { PlayerId = key }).ConfigureAwait(false);
            }
        }
    }
}
