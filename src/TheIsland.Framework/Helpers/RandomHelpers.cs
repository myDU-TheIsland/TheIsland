// <copyright file="RandomHelpers.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Framework.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class RandomHelpers
    {
        public static T Pick<T>(this Random random, Dictionary<T, double> elementToProbability) where T : notnull, new()
        {
            double totalProbability = elementToProbability.Values.Sum();
            double randomValue = random.NextDouble() * totalProbability;

            foreach (KeyValuePair<T, double> keyValuePair in elementToProbability)
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