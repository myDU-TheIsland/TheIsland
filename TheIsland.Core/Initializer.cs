// <copyright file="Initializer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core
{
    public static class Initializer
    {
        public static void InitializeCore()
        {
            NQutils.Config.Config.ReadYamlFile("mod", "./dual.yaml");
        }
    }
}
