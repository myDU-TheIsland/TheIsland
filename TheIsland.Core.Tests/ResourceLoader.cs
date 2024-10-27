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
            Assembly assembly = Assembly.GetAssembly(typeof(BlueprintSanitizerServiceTests)) !;
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new NullReferenceException($"{resourceName} not found or is not an Embedded Resource");
            }

            StreamReader sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }
}