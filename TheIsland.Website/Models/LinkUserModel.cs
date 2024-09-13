// <copyright file="LinkUserModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class LinkUserModel
    {
        [DisplayName("Player Name")]
        [StringLength(32)]
        public string PlayerName { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
