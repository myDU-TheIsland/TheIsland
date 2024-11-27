// <copyright file="Validate.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Validators
{
    using System.Threading.Tasks;
    using Backend;
    using TheIsland.BlueprintChecker.Classes;

    public abstract class Validate : IValidate
    {
        protected IGameplayBank GameplayBank { get; }

        public string Name { get; }

        public Validate(IGameplayBank gameplayBank, string name)
        {
            this.GameplayBank = gameplayBank;
            this.Name = name;
        }

        public abstract Task<ValidationResult> ValidateAsync(BlueprintDataExtended blueprint);
    }
}
