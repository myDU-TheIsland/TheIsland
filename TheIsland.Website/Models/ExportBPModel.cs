// <copyright file="ExportBPModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models
{
    using TheIsland.Core.Entities;

    public class ExportBPModel
    {
        public double SelectedBP { get; set; } = 0;

        public string SelectedBPName { get; set; } = string.Empty;

        public Dictionary<string, ulong> ExportableBPs { get; set; } = new Dictionary<string, ulong>();

        public BluePrintExport[] ExportedBPs { get; set; } = { };

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
