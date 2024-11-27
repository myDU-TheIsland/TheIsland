// <copyright file="VoxelData.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Classes
{
    using System;

    public class VoxelData
    {
        public byte[] data { get; set; }

        public object actualCellData { get; set; }

        public long hash { get; set; }

        public DateTime updated_at { get; set; }
    }
}
