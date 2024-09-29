// <copyright file="UserController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Models;

    public class UserController : IslandController
    {
        private readonly PlayerLinkingService _playerLinkingService;
        private readonly IGeneralBot _generalBot;

        public UserController(PlayerLinkingService playerLinkingService, IGeneralBot generalBot)
        {
            this._playerLinkingService = playerLinkingService;
            this._generalBot = generalBot;
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

            // find the players id;
            DualPlayer? player = await this._playerLinkingService.FindPlayer(model.PlayerName).ConfigureAwait(false);

            if (player == null)
            {
                model.ErrorMessage = "Could not find player.";
                return this.RedirectToAction("Link", model);
            }

            bool result = await this._playerLinkingService.SendToken(player.id, this.DiscordId).ConfigureAwait(false);

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

            bool result = await this._playerLinkingService.VerifyToken(model.Token).ConfigureAwait(false);

            if (!result)
            {
                model.Token = string.Empty;
                model.ErrorMessage = "Couldn't verify token given";
                return this.RedirectToAction("VerifyLink", model);
            }

            return this.RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ImportBP(ImportBPModel? model)
        {
            if (model == null)
            {
                model = new ImportBPModel();
            }

            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ImportBPFile(ImportBPModel model)
        {
            if (this.ModelState.IsValid && model.BluePrint != null)
            {
                UserMapping? result = await this._playerLinkingService.GetPlayerMapping(this.DiscordId).ConfigureAwait(false);
                if (result == null)
                {
                    model.ErrorMessage = "Couldn't find your player account. Is it linked?";
                    return this.RedirectToAction("ImportBP", model);
                }

                foreach (IFormFile blueprint in model.BluePrint)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            blueprint.CopyTo(ms);
                            byte[] fileBytes = ms.ToArray();
                            model.ErrorMessage += await this._generalBot.ImportBP(Convert.ToUInt64(result.dual_id), fileBytes).ConfigureAwait(false) + "<br />";
                        }
                    }
                    catch (Exception exception)
                    {
                        model.ErrorMessage += @$"Failed '{blueprint.FileName}' ({exception.Message}). <br />";
                    }
                }

                return this.RedirectToAction("ImportBP", model);
            }

            model.ErrorMessage = "Not sure what happened. Contact an admin";
            return this.RedirectToAction("ImportBP", model);
        }
    }
}
