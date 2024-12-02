// <copyright file="Initializer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker
{
    using Backend;
    using TheIsland.BlueprintChecker.Sanitizers.Blueprint;
    using TheIsland.BlueprintChecker.Sanitizers.Element;
    using TheIsland.BlueprintChecker.Validators.Blueprint;

    public static class Initializer
    {
        public static void InitializeBlueprintChecker(IGameplayBank gameplayBank)
        {
            HandleBlueprint.Sanitizers.Add(new BlueprintSanitizer(gameplayBank));
            HandleBlueprint.Sanitizers.Add(new ElementSanitizer(gameplayBank));
            HandleBlueprint.Sanitizers.Add(new ElementAttributeSanitizer(gameplayBank));

            HandleBlueprint.Validators.Add(new BlueprintValidator(gameplayBank));
        }
    }
}
