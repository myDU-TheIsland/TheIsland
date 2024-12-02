// <copyright file="BlueprintValidator.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Validators.Blueprint
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Backend;
    using NQ;
    using TheIsland.BlueprintChecker.Classes;
    using TheIsland.BlueprintChecker.Validators;
    using static TheIsland.BlueprintChecker.Validators.ValidationResult;

    internal class BlueprintValidator : Validate
    {
        public BlueprintValidator(IGameplayBank gameplayBank) : base(gameplayBank, nameof(BlueprintValidator))
        {
        }

        public override Task<ValidationResult> ValidateAsync(BlueprintDataExtended blueprint)
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

            if (blueprint.Elements == null)
            {
                return Task.FromResult(Failed("Blueprint:Elements is null"));
            }

            if (blueprint.Elements.Count == 0)
            {
                return Task.FromResult(Failed("Blueprint:Elements is empty"));
            }

            bool isDyanmic = blueprint.Model.JsonProperties.kind == ConstructKind.DYNAMIC;
            bool isStatic = blueprint.Model.JsonProperties.kind == ConstructKind.STATIC;
            bool isSpace = blueprint.Model.JsonProperties.kind == ConstructKind.SPACE;

            ulong[] itemIds;

            switch (blueprint.Model.JsonProperties.kind)
            {
                case ConstructKind.DYNAMIC:
                    itemIds = this.GameplayBank.GetDefinition("CoreUnitDynamic")?.GetChildren().Select(item => item.Id).ToArray() ?? Array.Empty<ulong>();
                    break;
                case ConstructKind.STATIC:
                    itemIds = this.GameplayBank.GetDefinition("CoreUnitStatic")?.GetChildren().Select(item => item.Id).ToArray() ?? Array.Empty<ulong>();
                    break;
                case ConstructKind.SPACE:
                    itemIds = this.GameplayBank.GetDefinition("CoreUnitSpace")?.GetChildren().Select(item => item.Id).ToArray() ?? Array.Empty<ulong>();
                    break;
                default:
                    itemIds = Array.Empty<ulong>();
                    break;
            }

            ElementInfo? coreUnit = blueprint.Elements.FirstOrDefault(item => itemIds.Contains(item.elementType));

            if (coreUnit == null)
            {
                return Task.FromResult(Failed("core Unit doesn't match kind of bp"));
            }

            long coreSize = this.GameplayBank.GetDefinition(coreUnit.elementType)?.GetStaticProperty("constructSize").intValue ?? 0;
            long blueprintSize = Convert.ToInt64(blueprint.Model.JsonProperties.size);

            if (coreSize != blueprintSize)
            {
                return Task.FromResult(Failed("core unit size does not match size of blueprint"));
            }

            if (blueprint.Model.JsonProperties.voxelGeometry.size != blueprintSize)
            {
                return Task.FromResult(Failed("size does not match voxel geometry"));
            }

            return Task.FromResult(Succeeded());
        }
    }
}
