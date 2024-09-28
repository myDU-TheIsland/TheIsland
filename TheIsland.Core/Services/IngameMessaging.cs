// <copyright file="IngameMessaging.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System.Threading.Tasks;
    using TheIsland.Core.Bots;

    public interface IIngameMessaging
    {
        Task SendMessage(ulong who, string message);
    }

    public class IngameMessaging : IIngameMessaging
    {
        private readonly IGeneralBot _client;

        public IngameMessaging(IGeneralBot client)
        {
            this._client = client;
        }

        public Task SendMessage(ulong who, string message)
        {
            return this._client.SendMessage(who, message);
        }
    }
}
