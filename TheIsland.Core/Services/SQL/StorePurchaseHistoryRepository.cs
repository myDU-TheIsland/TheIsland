// <copyright file="StorePurchaseHistoryRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Settings;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Duplicate rules overlapping")]
    public class StorePurchaseHistoryRepository(PostgresSettings settings) : EntityRepository<StorePurchaseHistory>(settings, settings.Database)
    {
        public async Task<IEnumerable<StorePurchaseHistory>> GetPlayerPurchaseHistory(double playerId)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<StorePurchaseHistory>("SELECT * FROM public.purchase_history where player_id = @playerId;", new { playerId = playerId }).ConfigureAwait(false);
            }
        }
    }
}
