// <copyright file="IIngameMessaging.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Interfaces
{
    public interface IIngameMessaging
    {
        Task SendMessage(ulong who, string message);
    }
}
