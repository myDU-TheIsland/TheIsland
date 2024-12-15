// <copyright file="DualPlayerRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Interfaces;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;

    public class DualPlayerRepository : NpgsqlEntityRepository<DualPlayer>, IDualPlayerRepository
    {
        public DualPlayerRepository(IDatabaseSettings settings) : base(settings, settings.DualDatabase)
        {
        }

        public async Task<bool> IsBotByPlayerId(double playerId, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.ExecuteScalarAsync<bool>("SELECT COALESCE(player.is_bot, false) as is_bot FROM public.ownership LEFT JOIN public.player ON ownership.player_id = player.id WHERE  ownership.player_id = @playerId;", new { playerId }).ConfigureAwait(false);
            }
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
                await databaseConnection.ExecuteAsync("UPDATE player SET wallet = wallet + @amount WHERE id = @playerId", new { playerId, amount }).ConfigureAwait(false);
                return;
            }
        }
    }
}
