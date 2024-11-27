// <copyright file="IValidate.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Validators
{
    using System.Threading.Tasks;
    using TheIsland.BlueprintChecker.Classes;

    public interface IValidate
    {
        string Name { get; }

        Task<ValidationResult> ValidateAsync(BlueprintDataExtended blueprint);

        ValidationResult Validate(BlueprintDataExtended blueprint) => this.ValidateAsync(blueprint).GetAwaiter().GetResult();
    }
}
