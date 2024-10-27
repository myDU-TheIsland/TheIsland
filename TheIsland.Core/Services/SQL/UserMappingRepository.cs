// <copyright file="UserMappingRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Collections.Generic;
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    public class UserMappingRepository : EntityRepository<UserMapping>
    {
        public UserMappingRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }

        public async Task<IEnumerable<UserMapping>> FindByDiscordId(string discordId, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<UserMapping>("SELECT id, dual_id, discord_id FROM public.user_mappings WHERE discord_id = @DiscordId;", new { DiscordId = discordId }).ConfigureAwait(false);
            }
        }
    }
}
