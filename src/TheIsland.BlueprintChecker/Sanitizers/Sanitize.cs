// <copyright file="Sanitize.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Sanitizers
{
    using System.Threading.Tasks;
    using Backend;
    using NQ;

    public abstract class Sanitize : ISanitize
    {
        protected IGameplayBank GameplayBank { get; }

        public string Name { get; }

        public Sanitize(IGameplayBank gameplayBank, string name)
        {
            this.GameplayBank = gameplayBank;
            this.Name = name;
        }

        public abstract Task<SanitizationResult> SantizeAsync(BlueprintData blueprint);
    }
}
