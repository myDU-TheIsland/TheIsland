// <copyright file="AppViewPage.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Framework
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Razor.Internal;
    using TheIsland.Data.Entities;
    using TheIsland.Data.Repositories;
    using TheIsland.Framework.Services;

    public abstract class AppViewPage<TModel> : RazorPage<TModel> where TModel : class
    {
        public string DiscordId => this.Context?.User?.Claims?.FirstOrDefault(item => item.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "0";

        public double SelectedPlayer => this.GetPlayer();

        public bool IsAdmin => this.AuthorizationService.AuthorizeAsync(this.Context.User, "Admin").GetAwaiter().GetResult().Succeeded;

        public bool IsUser => this.AuthorizationService.AuthorizeAsync(this.Context.User, "Admin").GetAwaiter().GetResult().Succeeded;

        public bool IsLoggedIn => this.Context?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsInGame => this._dualPlayerRepository.GetAsync(this.SelectedPlayer).GetAwaiter().GetResult().connected;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [RazorInject]
        public IAuthorizationService AuthorizationService { get; set; }

        [RazorInject]
        public PlayerLinkingService PlayerLinkingService { get; set; }

        [RazorInject]
        public IDualPlayerRepository _dualPlayerRepository { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        private double GetPlayer()
        {
            if (!this.Context?.User?.Identity?.IsAuthenticated ?? false)
            {
                return 0;
            }

            double currentPlayer = double.Parse(this.Context?.Session?.GetString("Player") ?? "0");

            if (currentPlayer != 0)
            {
                return currentPlayer;
            }

            IEnumerable<UserMapping> players = this.PlayerLinkingService.GetPlayerMapping(this.DiscordId).GetAwaiter().GetResult();

            if (players.Any())
            {
                double first = players.First().dual_id;

                //set session variable
                this.Context?.Session?.SetString("Player", first.ToString());
                return first;
            }

            return 0;
        }
    }
}
