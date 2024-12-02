// <copyright file="ExportBPModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models
{
    using TheIsland.Data.Entities;

    public class ExportBPModel
    {
        public double SelectedBP { get; set; } = 0;

        public string SelectedBPName { get; set; } = string.Empty;

        public Dictionary<ulong, string> ExportableBPs { get; set; } = new Dictionary<ulong, string>();

        public BluePrintExport[] ExportedBPs { get; set; } = { };

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
