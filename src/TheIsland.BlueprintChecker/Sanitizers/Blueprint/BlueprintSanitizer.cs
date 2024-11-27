// <copyright file="BlueprintSanitizer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Sanitizers.Blueprint
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Backend;
    using NQ;
    using TheIsland.BlueprintChecker.Sanitizers;
    using static TheIsland.BlueprintChecker.Sanitizers.SanitizationResult;

    internal class BlueprintSanitizer : Sanitize
    {
        public BlueprintSanitizer(IGameplayBank gameplayBank) : base(gameplayBank, nameof(BlueprintSanitizer))
        {
        }

        public override Task<SanitizationResult> SantizeAsync(BlueprintData blueprint)
        {
            if (blueprint == null)
            {
                return Task.FromResult(Failed("Blueprint is null"));
            }

            if (blueprint.Model == null)
            {
                return Task.FromResult(Failed("Blueprint:Model is null"));
            }

            if (blueprint.Model.JsonProperties == null)
            {
                return Task.FromResult(Failed("Blueprint:Model:JsonProperties is null"));
            }

            blueprint.Model.FreeDeploy = false;
            blueprint.Model.JsonProperties.isNPC = false;
            blueprint.Model.JsonProperties.isUntargetable = false;
            blueprint.Model.JsonProperties.planetProperties = null;

            if (blueprint.Model.JsonProperties.serverProperties != null)
            {
                blueprint.Model.JsonProperties.serverProperties.isFixture = null;
                blueprint.Model.JsonProperties.serverProperties.isBase = null;
                blueprint.Model.JsonProperties.serverProperties.isFlaggedForModeration = null;
                blueprint.Model.JsonProperties.serverProperties.isDynamicWreck = false;
                blueprint.Model.JsonProperties.serverProperties.fuelType = null;
                blueprint.Model.JsonProperties.serverProperties.fuelAmount = null;
                blueprint.Model.JsonProperties.serverProperties.compacted = false;
                blueprint.Model.JsonProperties.serverProperties.dynamicFixture = null;
                blueprint.Model.JsonProperties.serverProperties.constructCloneSource = null;
                blueprint.Model.JsonProperties.serverProperties.rdmsTags = new ConstructTags
                {
                    constructTags = new List<string>(),
                    elementsTags = new List<ElementTags>(),
                };
            }

            return Task.FromResult(Succeeded());
        }
    }
}
