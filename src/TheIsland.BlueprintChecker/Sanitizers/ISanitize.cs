// <copyright file="ISanitize.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Sanitizers
{
    using System.Threading.Tasks;
    using NQ;

    public interface ISanitize
    {
        string Name { get; }

        Task<SanitizationResult> SantizeAsync(BlueprintData blueprint);

        SanitizationResult Santize(BlueprintData blueprint) => this.SantizeAsync(blueprint).GetAwaiter().GetResult();
    }
}
