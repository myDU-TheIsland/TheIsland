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
                return await databaseConnection.QueryFirstOrDefaultAsync<DualPlayer>("SELECT * FROM public.player WHERE display_name = @DisplayName;", new { DisplayName = displayName }).ConfigureAwait(false);
            }
        }

        public async Task UpdateWallet(double playerId, double amount, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                await databaseConnection.ExecuteAsync("UPDATE player SET wallet = wallet + @amount WHERE id = @playerId", new { playerId = playerId, amount = amount }).ConfigureAwait(false);
                return;
            }
        }
    }
}
