// <copyright file="DualMarketOrderRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using TheIsland.Core.Interfaces;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;

    public class DualMarketOrderRepository : NpgsqlEntityRepository<DualMarketOrder>, IDualMarketOrderRepository
    {
        private const string GetByMarketIdQuery = $@"
SELECT market_order.*,ownership.player_id
FROM public.market_order
INNER JOIN public.ownership ON 
	ownership.organization_id is null
	AND ownership.id = market_order.owner_id
INNER JOIN public.player ON
	ownership.player_id = player.id
	AND player.is_bot = false
WHERE market_order.status = 0
AND market_order.buy_quantity < 0
AND market_order.market_id = @marketId
";

        public DualMarketOrderRepository(IDatabaseSettings settings) : base(settings, settings.DualDatabase)
        {
        }

        public async Task<IEnumerable<DualMarketOrder>> GetByMarketIdAsync(double marketId, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualMarketOrder>(GetByMarketIdQuery, new { marketId }).ConfigureAwait(false);
            }
        }
    }
}
