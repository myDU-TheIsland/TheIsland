// <copyright file="StoreItemContent.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.Classes
{
    using System.ComponentModel.DataAnnotations;

    public class StoreItemContent
    {
        [Display(Name = "Give talent points")]
        public double TalentPoints { get; set; }

        [Display(Name = "Reset talent points?")]
        public bool RespecTalentPoints { get; set; }

        public List<KeyValuePair<double, double>> Items { get; set; } = new List<KeyValuePair<double, double>>();

        public List<KeyValuePair<double, string>> Skins { get; set; } = new List<KeyValuePair<double, string>>();
    }
}
