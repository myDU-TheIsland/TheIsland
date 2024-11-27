// <copyright file="LinkTokenRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class LinkTokenRepository : EntityRepository<LinkToken>
    {
        public LinkTokenRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }

        public async Task<LinkToken?> FindByPlayerId(double playerId, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryFirstOrDefaultAsync<LinkToken>("SELECT id, discord_id, token, player_id FROM public.tokens WHERE player_id = @PlayerId;", new { PlayerId = playerId }).ConfigureAwait(false);
            }
        }

        public async Task<LinkToken?> FindByToken(string token, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryFirstOrDefaultAsync<LinkToken>("SELECT id, discord_id, token, player_id FROM public.tokens WHERE token = @Token;", new { Token = token }).ConfigureAwait(false);
            }
        }
    }
}
