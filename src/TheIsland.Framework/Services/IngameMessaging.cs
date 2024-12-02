// <copyright file="IngameMessaging.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Framework.Services
{
    using System.Threading.Tasks;
    using TheIsland.Core.Interfaces;
    using TheIsland.Framework.Bots;

    public interface IIngameMessaging : IAppService
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
            return this._client.SendMessageAsync(who, message);
        }
    }
}
