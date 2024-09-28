// <copyright file="MarketTransactionRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Data.Common;
    using Dapper;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;

    public class MarketTransactionRepository : EntityRepository<MarketTransaction>
    {
        public MarketTransactionRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }

        public async Task<IEnumerable<MarketStatistics>> GetHourlyStats(double item)
        {
            string query = $@"
WITH 
--Generate time series based on min and max creation_date
TIME_SERIES AS (
  SELECT GENERATE_SERIES AS hour
  FROM GENERATE_SERIES
        ( (SELECT MIN(date_trunc('hour', creation_date)) FROM public.market_transactions)
        , (SELECT MAX(date_trunc('hour', creation_date)) FROM public.market_transactions)
        , '1 hour'::interval))

SELECT 
	TIME_SERIES.hour as ""DateTime"",
	COALESCE(item_id, @ItemId) as item_id, 
	SUM(quantity) as total_quantity,
	COUNT(quantity) as transactions,
	ABS(AVG(price / quantity)) as average_price
FROM TIME_SERIES
LEFT JOIN public.market_transactions
	ON TIME_SERIES.hour = date_trunc('hour', creation_date) 
	AND item_id = @ItemId
GROUP BY 
	item_id,
	TIME_SERIES.hour
ORDER BY TIME_SERIES.hour DESC
LIMIT 72
			";

            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<MarketStatistics>(query, new { ItemId = item }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<MarketStatistics>> GetDailyStats(double item)
        {
            string query = $@"
				SELECT 
					date_part('year', creation_date) as ""year"",
					date_part('month', creation_date) as ""month"",
					date_part('day', creation_date) as ""day"",
					-1 as ""hour"",
					item_id, 
					market_id,
					SUM(quantity) as total_quantity,
					COUNT(quantity) as transactions,
					ABS(AVG(price / quantity)) as average_price
				FROM public.market_transactions
				WHERE item_id = @ItemId
				GROUP BY 
						item_id,
						market_id,
						date_part('year', creation_date),
						date_part('month', creation_date),
						date_part('day', creation_date)
				ORDER BY date_part('year', creation_date),
						date_part('month', creation_date),
						date_part('day', creation_date)
			";

            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<MarketStatistics>(query, new { ItemId = item }).ConfigureAwait(false);
            }
        }
    }
}
