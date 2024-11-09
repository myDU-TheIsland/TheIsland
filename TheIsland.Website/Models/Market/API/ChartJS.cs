// <copyright file="ChartJS.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models.Market.API
{
    public class ChartJS
    {
        public List<string> labels { get; set; } = new List<string>();

        public ChartJSDataset[] datasets { get; set; } = { };
    }

    public class ChartJSDataset
    {
        public string label { get; set; } = string.Empty;

        public List<decimal> data { get; set; } = new List<decimal>();

        public string yAxisID { get; set; } = string.Empty;
    }
}
