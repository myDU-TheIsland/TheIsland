// <copyright file="DualMarketRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.SQL
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Core.Settings;

    public class DualMarketRepository : EntityRepository<DualMarket>
    {
        public DualMarketRepository(PostgresSettings settings) : base(settings, settings.DualDatabase)
        {
        }

        public override async Task<IEnumerable<DualMarket>> GetAsync(CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualMarket>("SELECT public.market.id, public.market.name, public.element.construct_id as construct_id FROM public.market INNER JOIN public.element ON public.element.id = public.market.element_id order by id;").ConfigureAwait(false);
            }
        }

        public override async Task<IEnumerable<DualMarket>> GetAsync(IEnumerable<double> keys, CancellationToken cancellationToken = default)
        {
            if (keys == null)
            {
                throw new Exception("Keys can not be null");
            }

            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryAsync<DualMarket>("SELECT public.market.id, public.market.name, public.element.construct_id as construct_id FROM public.market INNER JOIN public.element ON public.element.id = public.market.element_id WHERE public.market.id IN @Ids order by id;", new { Ids = keys }).ConfigureAwait(false);
            }
        }

        public override async Task<DualMarket> GetAsync(double key, CancellationToken cancellationToken = default)
        {
            using (DbConnection databaseConnection = this.GetConnection())
            {
                return await databaseConnection.QueryFirstAsync<DualMarket>("SELECT public.market.id, public.market.name, public.element.construct_id as construct_id FROM public.market INNER JOIN public.element ON public.element.id = public.market.element_id WHERE public.market.id = @Id order by id;", new { Id = key }).ConfigureAwait(false);
            }
        }
    }
}
