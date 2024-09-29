// <copyright file="DualPlayerRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class DualPlayerRepository : EntityRepository<DualPlayer>
    {
        public DualPlayerRepository(PostgresSettings settings) : base(settings, settings.DualDatabase)
        {
        }

        public async Task<DualPlayer?> FindByDisplayName(string displayName, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryFirstOrDefaultAsync<DualPlayer>("SELECT id, display_name, connected FROM public.player WHERE display_name = @DisplayName;", new { DisplayName = displayName }).ConfigureAwait(false);
            }
        }
    }
}
