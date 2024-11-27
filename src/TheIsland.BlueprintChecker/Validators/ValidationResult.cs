// <copyright file="ValidationResult.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.BlueprintChecker.Validators
{
    public class ValidationResult(bool success, string message)
    {
        public bool Success { get; } = success;

        public bool IsError => !this.Success;

        public string Message { get; set; } = message;

        public static ValidationResult Failed(string msg) => new ValidationResult(false, msg);

        public static ValidationResult Succeeded() => new ValidationResult(true, string.Empty);
    }
}
