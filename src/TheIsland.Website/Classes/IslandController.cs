// <copyright file="IslandController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Classes
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;

    [ApiExplorerSettings(IgnoreApi = true)]
    public class IslandController : Controller
    {
        protected string DiscordId => this.HttpContext?.User?.Claims?.FirstOrDefault(item => item.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "0";

        protected double SelectedPlayer => this.GetPlayer();

        protected bool IsAdmin => this.AuthorizationService.AuthorizeAsync(this.HttpContext.User, "Admin").GetAwaiter().GetResult().Succeeded;

        protected bool IsUser => this.AuthorizationService.AuthorizeAsync(this.HttpContext.User, "User").GetAwaiter().GetResult().Succeeded;

        protected bool IsLoggedIn => this.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        private IAuthorizationService AuthorizationService { get; set; }

        protected PlayerLinkingService PlayerLinkingService { get; }

        public IslandController(PlayerLinkingService playerLinkingService, IAuthorizationService authorizationService)
        {
            this.PlayerLinkingService = playerLinkingService;
            this.AuthorizationService = authorizationService;
        }

        internal double GetPlayer()
        {
            if (!this.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
            {
                return 0;
            }

            double currentPlayer = double.Parse(this.HttpContext?.Session?.GetString("Player") ?? "0");

            if (currentPlayer != 0)
            {
                return currentPlayer;
            }

            IEnumerable<Core.Entities.UserMapping> players = this.PlayerLinkingService.GetPlayerMapping(this.DiscordId).GetAwaiter().GetResult();

            if (players.Any())
            {
                double first = players.First().dual_id;

                //set session variable
                this.HttpContext?.Session?.SetString("Player", first.ToString());
                return first;
            }

            return 0;
        }
    }
}
