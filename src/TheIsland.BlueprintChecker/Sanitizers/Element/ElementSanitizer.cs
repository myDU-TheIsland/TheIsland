// <copyright file="ElementSanitizer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Sanitizers.Element
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Backend;
    using NQ;
    using TheIsland.BlueprintChecker.Sanitizers;
    using static TheIsland.BlueprintChecker.Sanitizers.SanitizationResult;

    internal class ElementSanitizer : Sanitize
    {
        public ElementSanitizer(IGameplayBank gameplayBank) : base(gameplayBank, nameof(ElementSanitizer))
        {
        }

        public override Task<SanitizationResult> SantizeAsync(BlueprintData blueprint)
        {
            if (blueprint == null)
            {
                return Task.FromResult(Failed("Blueprint is null"));
            }

            if (blueprint.Elements == null)
            {
                return Task.FromResult(Failed("Blueprint:Elements is null"));
            }

            if (blueprint.Elements.Count == 0)
            {
                return Task.FromResult(Failed("Blueprint:Elements is empty"));
            }

            List<ulong> elementsToRemove = new List<ulong>();
            List<ulong> elementsChecked = new List<ulong>();

            foreach (ElementInfo element in blueprint.Elements)
            {
                if (elementsChecked.Contains(element.elementType))
                {
                    continue;
                }

                elementsChecked.Add(element.elementType);

                IGameplayDefinition elementData = this.GameplayBank.GetDefinition(element.elementType);

                if (elementData == null)
                {
                    elementsToRemove.Add(element.elementType);
                }
            }

            blueprint.Elements = blueprint.Elements.Where(item => !elementsToRemove.Contains(item.elementType)).ToList();

            return Task.FromResult(Succeeded());
        }
    }
}
