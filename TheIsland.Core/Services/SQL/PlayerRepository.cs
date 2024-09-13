// <copyright file="PlayerRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System;
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;

    public class PlayerRepository : EntityRepository<Player>
    {
        public PlayerRepository(PostgresSettings settings) : base(settings, settings.DualDatabase)
        {
        }

        public async Task<Player?> FindByDisplayName(string displayName, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryFirstOrDefaultAsync<Player>("SELECT id, display_name, connected FROM public.player WHERE display_name = @DisplayName;", new { DisplayName = displayName }).ConfigureAwait(false);
            }
        }
    }
}
