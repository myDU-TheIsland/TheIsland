// <copyright file="HandleBlueprint.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using TheIsland.BlueprintChecker.Classes;
    using TheIsland.BlueprintChecker.Sanitizers;
    using TheIsland.BlueprintChecker.Validators;

    public static class HandleBlueprint
    {
        public static List<ISanitize> Sanitizers { get; } = new List<ISanitize>();

        public static List<IValidate> Validators { get; } = new List<IValidate>();

        public static (bool IsGood, List<SanitizationResult> sanitizationResult, List<ValidationResult> validationResults) IsBlueprintGood(byte[] jsondata)
        {
            BlueprintDataExtended? blueprint = JsonConvert.DeserializeObject<BlueprintDataExtended>(Encoding.UTF8.GetString(jsondata));

            List<SanitizationResult> sanitizationResult = new List<SanitizationResult>();
            List<ValidationResult> validationResults = new List<ValidationResult>();

            if (blueprint == null)
            {
                return (true, new List<SanitizationResult>(), new List<ValidationResult>());
            }

            foreach (ISanitize sanitizer in Sanitizers)
            {
                sanitizationResult.Add(sanitizer.Santize(blueprint));
            }

            foreach (IValidate validator in Validators)
            {
                validationResults.Add(validator.Validate(blueprint));
            }

            if (validationResults.Any(item => item.IsError) || sanitizationResult.Any(item => item.IsError))
            {
                return (false, sanitizationResult, validationResults);
            }

            return (true, sanitizationResult, validationResults);
        }
    }
}
