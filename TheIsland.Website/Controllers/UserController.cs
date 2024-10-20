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
        private readonly IBlueprintService _bluePrintService;

        public UserController(IBlueprintService bluePrintService, PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
            this._bluePrintService = bluePrintService;
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
            DualPlayer? player = await this.PlayerLinkingService.FindPlayer(model.PlayerName).ConfigureAwait(false);

            if (player == null)
            {
                model.ErrorMessage = "Could not find player.";
                return this.RedirectToAction("Link", model);
            }

            bool result = await this.PlayerLinkingService.SendToken(player.id, this.DiscordId).ConfigureAwait(false);

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

            bool result = await this.PlayerLinkingService.VerifyToken(model.Token).ConfigureAwait(false);

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
        public async Task<IActionResult> ExportBP(ExportBPModel? model)
        {
            if (model == null)
            {
                model = new ExportBPModel();
            }

            if (this.SelectedPlayer > 0)
            {
                model.ExportedBPs = await this._bluePrintService.GetMyExportedBps(Convert.ToUInt64(this.SelectedPlayer)).ConfigureAwait(false);
                model.ExportableBPs = await this._bluePrintService.GetExportableBPs(Convert.ToUInt64(this.SelectedPlayer)).ConfigureAwait(false);
            }

            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GenerateBP(ExportBPModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.ErrorMessage = "Something went wrong!";
                this.RedirectToAction("ExportBP", model);
            }

            await this._bluePrintService.SaveBP(Convert.ToUInt64(this.SelectedPlayer), Convert.ToUInt64(model.SelectedBP), model.SelectedBPName).ConfigureAwait(false);

            return this.RedirectToAction("ExportBP");
        }

        [HttpGet]
        [Authorize]
        [Route("~/BP/{guid}")]
        public async Task<IActionResult> GetBP(Guid guid)
        {
            var blueprintResult = await this._bluePrintService.GetBP(guid, Convert.ToUInt64(this.SelectedPlayer)).ConfigureAwait(false);

            if (blueprintResult == null)
            {
                return this.RedirectToAction("ExportBP");
            }

            var byteArray = await System.IO.File.ReadAllBytesAsync(this._bluePrintService.GetBPPath(blueprintResult.uuid)).ConfigureAwait(false);

            return this.File(byteArray, "text/json", $@"{blueprintResult.blueprint_name.Replace(' ', '_').ToLower()}.json");
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
                var player = this.SelectedPlayer;

                if (player == 0)
                {
                    player = (await this.PlayerLinkingService.GetPlayerMapping(this.DiscordId).ConfigureAwait(false)).FirstOrDefault()?.dual_id ?? -1;
                }

                if (player == 0)
                {
                    model.ErrorMessage = "Failed to find player id, did you link your player yet?";
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
                            model.ErrorMessage += await this._bluePrintService.ImportBP(Convert.ToUInt64(player), fileBytes).ConfigureAwait(false) + "<br />";
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SetPlayerId(double playerId)
        {
            string? urlReferrer = null;
            if (this.HttpContext.Request.Headers.ContainsKey("Referer"))
            {
                urlReferrer = this.HttpContext.Request.Headers.Referer!.ToString();
            }

            if (urlReferrer == null)
            {
                urlReferrer = "~/";
            }

            var players = await this.PlayerLinkingService.GetPlayerMapping(this.DiscordId).ConfigureAwait(false);

            if (players.Any(item => item.dual_id == playerId))
            {
                //set session variable
                this.HttpContext.Session.SetString("Player", playerId.ToString());
            }

            return this.Redirect(urlReferrer);
        }
    }
}
