// <copyright file="DataTableModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models
{
    public class Graph
    {
        public ColInfo[] cols { get; set; } = { };

        public DataPointSet[] rows { get; set; } = { };

        public Dictionary<string, string> p { get; set; } = new Dictionary<string, string>();
    }

    public class ColInfo
    {
        public string id { get; set; } = string.Empty;

        public string label { get; set; } = string.Empty;

        public string type { get; set; } = string.Empty;
    }

    public class DataPointSet
    {
        public DataPoint[] c { get; set; } = { };
    }

    public class DataPoint
    {
        public object? v { get; set; } = string.Empty; // value

        public string f { get; set; } = string.Empty; // format
    }
}
