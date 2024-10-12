// <copyright file="IBlueprintSanitizerService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>
namespace TheIsland.Core.Services.Blueprint
{
    using Backend;

    public interface IBlueprintSanitizerService
    {
        Task<BlueprintSanitationResult> SanitizeAsync(IGameplayBank bank, byte[] blueprintBytes, CancellationToken cancellationToken);
    }
}