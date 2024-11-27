// <copyright file="SanitizationResult.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Sanitizers
{
    public class SanitizationResult(bool success, string message)
    {
        public bool Success { get; } = success;

        public bool IsError => !this.Success;

        public string Message { get; set; } = message;

        public static SanitizationResult Failed(string msg) => new SanitizationResult(false, msg);

        public static SanitizationResult Succeeded() => new SanitizationResult(true, string.Empty);
    }
}
