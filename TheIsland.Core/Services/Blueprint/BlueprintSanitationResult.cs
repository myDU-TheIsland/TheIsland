// <copyright file="BlueprintSanitationResult.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.Blueprint;

public class BlueprintSanitationResult
{
    public BlueprintSanitationResult(bool success, byte[] bytes, string message)
    {
        this.Success = success;
        this.BlueprintBytes = bytes;
        this.Message = message;
    }

    public bool Success { get; }

    public byte[] BlueprintBytes { get; }

    public string Message { get; }

    public static BlueprintSanitationResult Failed(string msg) => new BlueprintSanitationResult(false, Array.Empty<byte>(), msg);

    public static BlueprintSanitationResult Succeeded(byte[] bp) => new BlueprintSanitationResult(true, bp, string.Empty);
}