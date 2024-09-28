// <copyright file="GraphHelpers.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Framework.Helpers
{
    using TheIsland.Core.Services.SQL.Entities;
    using TheIsland.Website.Models;

    public static class GraphHelpers
    {
        public static Graph ToGraph(this IEnumerable<MarketStatistics> input)
        {
            Graph graph = new Graph()
            {
                cols = new ColInfo[]
                {
                    new ColInfo { id = "Time", label = "Time", type = "datetime" },
                    new ColInfo { id = "TotalSold", label = "Total Sold", type = "number" },
                    new ColInfo { id = "AveragePrice", label = "Average Price", type = "number" },
                },
                rows = input.Select(item =>
                    new DataPointSet
                    {
                        c = new DataPoint[]
                        {
                            new DataPoint { v = item.DateTime },
                            new DataPoint { v = item.total_quantity },
                            new DataPoint { v = item.average_price },
                        },
                    }).ToArray(),
            };

            return graph;
        }
    }
}
