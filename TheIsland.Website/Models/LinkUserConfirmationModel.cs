// <copyright file="LinkUserConfirmationModel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class LinkUserConfirmationModel
    {
        [DisplayName("Token")]
        [StringLength(8)]
        public string Token { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
