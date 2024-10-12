// <copyright file="ResourceLoader.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

using System.Reflection;

namespace TheIsland.Core.Tests
{
    public static class ResourceLoader
    {
        public static string GetStringContents(string resourceName)
        {
            var assembly = Assembly.GetAssembly(typeof(BlueprintSanitizerServiceTests)) !;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new NullReferenceException($"{resourceName} not found or is not an Embedded Resource");
            }

            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }
}