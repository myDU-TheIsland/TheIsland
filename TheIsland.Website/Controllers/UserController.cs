// <copyright file="UserController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Services;
    using TheIsland.Website.Models;

    public class UserController : Controller
    {
        private readonly PlayerLinkingService _playerLinkingService;

        public UserController(PlayerLinkingService playerLinkingService)
        {
            this._playerLinkingService = playerLinkingService;
        }

        [HttpGet("~/link")]
        [Authorize]
        public IActionResult Link(LinkUserModel? model = default)
        {
            if (model == null)
            {
                model = new LinkUserModel();
            }

            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LinkUser(LinkUserModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("Link", model);
            }

            double discordId = double.Parse(this.HttpContext.User.Claims.FirstOrDefault(item => item.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "-1");

            // find the players id;
            var player = await this._playerLinkingService.FindPlayer(model.PlayerName).ConfigureAwait(false);

            if (player == null)
            {
                model.ErrorMessage = "Could not find player.";
                return this.RedirectToAction("Link", model);
            }

            var result = await this._playerLinkingService.SendToken(player.id, discordId).ConfigureAwait(false);

            if (!result)
            {
                model.ErrorMessage = "Player is not online. Please login into the server first.";
                return this.RedirectToAction("Link", model);
            }

            return this.RedirectToAction("VerifyLink");
        }

        [HttpGet]
        [Authorize]
        public IActionResult VerifyLink(LinkUserConfirmationModel? model)
        {
            if (model == null)
            {
                model = new LinkUserConfirmationModel();
            }

            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmLink(LinkUserConfirmationModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.Token = string.Empty;
                model.ErrorMessage = "Something went wrong.";
                return this.RedirectToAction("VerifyLink", model);
            }

            var result = await this._playerLinkingService.VerifyToken(model.Token).ConfigureAwait(false);

            if (!result)
            {
                model.Token = string.Empty;
                model.ErrorMessage = "Couldn't verify token given";
                return this.RedirectToAction("VerifyLink", model);
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}
