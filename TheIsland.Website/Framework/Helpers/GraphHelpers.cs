// <copyright file="GraphHelpers.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Framework.Helpers
{
    using TheIsland.Core.Entities;
    using TheIsland.Website.Models.Market.API;

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

        public static ChartJS ToChartJS(this IEnumerable<MarketStatistics> input)
        {
            int counter = 1;

            ChartJS graph = new ChartJS()
            {
                labels = input.Select(item => item.DateTime.ToString("yyyy-MM-dd")).Distinct().OrderBy(item => item).ToList(),
                datasets = new[]
               {
                   new ChartJSDataset()
                   {
                       label = "Total Sold",
                       data = input.OrderBy(item => item.DateTime).Select(item => decimal.Parse(item.total_quantity?.ToString() ?? "0")).ToList(),
                       yAxisID = $@"y{counter++}",
                   },
                   new ChartJSDataset()
                   {
                       label = "Average Price",
                       data = input.OrderBy(item => item.DateTime).Select(item => decimal.Parse(item.average_price?.ToString() ?? "0")).ToList(),
                       yAxisID = $@"y{counter++}",
                   },
                   new ChartJSDataset()
                   {
                       label = "Maxium Price",
                       data = input.OrderBy(item => item.DateTime).Select(item => decimal.Parse(item.max_price?.ToString() ?? "0")).ToList(),
                       yAxisID = $@"y{counter++}",
                   },
                   new ChartJSDataset()
                   {
                       label = "Minium Price",
                       data = input.OrderBy(item => item.DateTime).Select(item => decimal.Parse(item.min_price?.ToString() ?? "0")).ToList(),
                       yAxisID = $@"y{counter++}",
                   },
               },
            };

            return graph;
        }
    }
}
