// <copyright file="BuildService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using NQ;
    using TheIsland.Core.Bots;

    public class BuildService : IAppService
    {
        private readonly IMarketBot _marketBot;

        public BuildService(IMarketBot dualClient)
        {
            this._marketBot = dualClient;
        }

        #region ItemCosts
        public void PriceItems()
        {
            //price pure
            //price product
            //price parts
        }

        #endregion
    }
}
