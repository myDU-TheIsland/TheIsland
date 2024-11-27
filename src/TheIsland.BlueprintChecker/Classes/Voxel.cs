// <copyright file="Voxel.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Classes
{
    using System;
    using System.Collections.Generic;

    public class Voxel
    {
        public DateTime created_at { get; set; }

        public VoxelMetadata metadata { get; set; }

        public long h { get; set; }

        public long k { get; set; }

        public long oid { get; set; }

        public Dictionary<string, VoxelData> records { get; set; }

        public DateTime updated_at { get; set; }

        public long version { get; set; }

        public long x { get; set; }

        public long y { get; set; }

        public long z { get; set; }
    }
}
