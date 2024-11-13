// <copyright file="ItemEntryExtensions.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TheIsland.Core.Classes;

    public static class ItemEntryExtensions
    {
        public static string ItemEntryCSVHeader()
        {
            return "id,name,size,tier,type,subtype,name,display_name";
        }

        public static string ToCSVLine(this ItemEntry input)
        {
            return @$"{input.Id},{input.Name},{input.Size},{input.Tier},{input.Type},{input.SubType},{input.Name},{input.DisplayName}";
        }
    }
}
