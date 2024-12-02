// <copyright file="ElementAttributeSanitizer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Sanitizers.Element
{
    using System;
    using System.Threading.Tasks;
    using Backend;
    using NQ;
    using TheIsland.BlueprintChecker.Sanitizers;
    using static TheIsland.BlueprintChecker.Sanitizers.SanitizationResult;

    internal class ElementAttributeSanitizer : Sanitize
    {
        public ElementAttributeSanitizer(IGameplayBank gameplayBank) : base(gameplayBank, nameof(ElementAttributeSanitizer))
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

            foreach (ElementInfo element in blueprint.Elements)
            {
                if (element.properties == null || element.properties.Count == 0)
                {
                    continue;
                }

                foreach (string property in element.properties.Keys)
                {
                    try
                    {
                        element.properties[property] = this.GetDefaultValue(element.elementType, property, element.properties[property]);
                    }
                    catch (Exception exception)
                    {
                        return Task.FromResult(Failed(exception.Message));
                    }
                }
            }

            return Task.FromResult(Succeeded());
        }

        private PropertyValue GetDefaultValue(ulong elementType, string propName, PropertyValue value)
        {
            NQutils.Def.Element? obj = this.GameplayBank.GetBaseObject<NQutils.Def.Element>(elementType);
            IGameplayDefinition? def = this.GameplayBank.GetDefinition(elementType);

            if (def == null || obj == null)
            {
                return value;
            }

            if (obj.hidden)
            {
                throw new InvalidOperationException("BP has hidden element");
            }

            PropertyValue? propVal = def.GetStaticPropertyOpt(propName);

            if (propVal != null)
            {
                return propVal.DeepCopy();
            }

            return value;
        }
    }
}
