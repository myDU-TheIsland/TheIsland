// <copyright file="DualWalletRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL.Repositories
{
    using System.Data.Common;
    using Dapper;
    using StackExchange.Redis;
    using TheIsland.Core.Caching;
    using TheIsland.Core.Interfaces;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;

    public class DualWalletRepository : NpgsqlEntityRepository<DualWalletTransaction>, IDualWalletRepository
    {
        private readonly RedisCache<List<MarketStatistics>> _marketStatistics;

        public DualWalletRepository(IDatabase redisDatabase, IDatabaseSettings settings) : base(settings, settings.DualDatabase)
        {
            this._marketStatistics = new RedisCache<List<MarketStatistics>>(redisDatabase);
        }

        public async Task<IEnumerable<DualWalletTransaction>> GetAllBotTransactionOnMarket(double marketId, double entity_id = 43453)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualWalletTransaction>("SELECT *, payload->'market'->>'marketId' as market_id, payload->'market'->>'itemType' as item_id, payload->'market'->'quantity'->>'quantity' as quantity FROM public.wallet_operation where amount < 0 and entity_id = @EntityId AND payload->'market'->>'marketId' = @MarketId AND time >= (NOW() - INTERVAL '72 hours' ) ", new { MarketId = marketId.ToString(), EntityId = entity_id }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<MarketStatistics>> GetDailyStats(double item, double marketId = -1)
        {
            string query = $@"
				WITH 
TIME_SERIES AS (
  SELECT GENERATE_SERIES AS day
  FROM GENERATE_SERIES
        ( (SELECT MIN(date_trunc('day', time)) FROM public.wallet_operation)
        , (SELECT MAX(date_trunc('day', time)) FROM public.wallet_operation)
        , '1 day'::interval)),
MARKET_TRANS AS (
SELECT 
	CASE WHEN amount > 0 then 'SELL' ELSE 'BUY' END AS type, 
	ABS(amount/100) as amount, 
	time, 
	(payload->'market'->>'marketId')::int as market_id, 
	(payload->'market'->>'itemType')::bigint as item_id, 
	(payload->'market'->'quantity'->>'quantity')::bigint as quantity 
	FROM public.wallet_operation where amount < 0 and entity_id not IN (1,7,3,2) and operation_type in (2,3,5) 

)

SELECT 
	TIME_SERIES.day as ""DateTime"",
	COALESCE(item_id,@ItemId) as item_id, 
	COALESCE(SUM(quantity),0) as total_quantity,
	COUNT(quantity) as transactions,
	ABS(AVG(amount / quantity)) as average_price, 
	max(ABS(amount / quantity)) as max_price,
	min(ABS(amount / quantity)) as min_price
FROM TIME_SERIES
LEFT JOIN MARKET_TRANS ON
	TIME_SERIES.day = date_trunc('day', MARKET_TRANS.time) 
	AND MARKET_TRANS.item_id = @ItemId
	{{market}}
GROUP BY 
	item_id,
	TIME_SERIES.day
ORDER BY TIME_SERIES.day DESC
            ";

            if (marketId > 0)
            {
                query = query.Replace("{market}", "AND market_id = @MarketId");
            }
            else
            {
                query = query.Replace("{market}", string.Empty);
            }

            string key = $@"MarketStatistics:{marketId}:{item}";
            List<MarketStatistics>? output = this._marketStatistics.Get(key);

            if (output == null)
            {
                using (DbConnection databaseConnection = this.GetConnection())
                {
                    output = (await databaseConnection.QueryAsync<MarketStatistics>(query, new { ItemId = item, MarketId = marketId }).ConfigureAwait(false)).ToList();
                    this._marketStatistics.Add(key, output, TimeSpan.FromDays(1));
                }
            }

            return output;
        }
    }
}
