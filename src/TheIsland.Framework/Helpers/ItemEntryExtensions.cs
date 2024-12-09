// <copyright file="ItemEntryExtensions.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Framework.Helpers
{
    using TheIsland.Data.Entities;

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
