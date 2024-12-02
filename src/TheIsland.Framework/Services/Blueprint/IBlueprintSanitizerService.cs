// <copyright file="IBlueprintSanitizerService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>
namespace TheIsland.Framework.Services.Blueprint
{
    using Backend;
    using TheIsland.Core.Interfaces;

    public interface IBlueprintSanitizerService : IAppService
    {
        Task<BlueprintSanitationResult> SanitizeAsync(IGameplayBank bank, byte[] blueprintBytes, CancellationToken cancellationToken);
    }
}