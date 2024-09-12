// <copyright file="IngameMessaging.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Classes
{
    using System.Threading.Tasks;
    using TheIsland.Core.Classes;
    using TheIsland.Website.Interfaces;

    public class IngameMessaging : IIngameMessaging
    {
        private readonly IDUClient _client;

        public IngameMessaging(IDUClient client)
        {
            this._client = client;
        }

        public Task SendMessage(ulong who, string message)
        {
            return this._client.SendMessage(who, message);
        }
    }
}
