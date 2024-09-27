// <copyright file="RandomHelpers.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class RandomHelpers
    {
        public static T Pick<T>(this Random random, Dictionary<T, double> elementToProbability) where T : notnull, new()
        {
            var totalProbability = elementToProbability.Values.Sum();
            var randomValue = random.NextDouble() * totalProbability;

            foreach (var keyValuePair in elementToProbability)
            {
                if (randomValue < keyValuePair.Value)
                {
                    return keyValuePair.Key;
                }

                randomValue -= keyValuePair.Value;
            }

            return new T();
        }
    }
}