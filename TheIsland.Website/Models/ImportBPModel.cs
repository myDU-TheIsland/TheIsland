// <copyright file="ImportBPModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class ImportBPModel
    {
        public List<IFormFile> BluePrint { get; set; } = new List<IFormFile>();

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
