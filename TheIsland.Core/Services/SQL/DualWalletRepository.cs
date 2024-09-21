// <copyright file="DualWalletRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;

    public class DualWalletRepository : EntityRepository<DualWalletTransaction>
    {
        public DualWalletRepository(PostgresSettings settings) : base(settings, settings.DualDatabase)
        {
        }

        public async Task<IEnumerable<DualWalletTransaction>> GetAllAfterIdAsync(double id)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualWalletTransaction>("SELECT * FROM public.wallet_operation where amount < 0 and entity_id not IN (1,7,3,2) and operation_type in (2,3,5) and id > @Id;", new { Id = id }).ConfigureAwait(false);
            }
        }
    }
}
