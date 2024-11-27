// <copyright file="BlueprintDataExtended.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Classes
{
    using System.Collections.Generic;

    public class BlueprintDataExtended : NQ.BlueprintData
    {
        public List<Voxel> Voxels { get; set; } = new List<Voxel>();
    }
}
