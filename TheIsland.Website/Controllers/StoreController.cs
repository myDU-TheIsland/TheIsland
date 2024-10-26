// <copyright file="StoreController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Models;

    [Authorize(Policy = "User")]
    [Route("~/[controller]/[action]")]
    public class StoreController : IslandController
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService, PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
            this._storeService = storeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(StoreSearchModel model)
        {
            model ??= new StoreSearchModel();

            model.StoreItems = await this._storeService.GetItemList(false).ConfigureAwait(false);

            return this.View(model);
        }

        [HttpGet]
        [Route("~/[controller]/[action]/{itemId:double}")]
        public async Task<IActionResult> View(double itemId)
        {
            var model = new ViewStoreItemModel();

            model.StoreItem = await this._storeService.GetItem(itemId).ConfigureAwait(false);

            return this.View(model);
        }
    }
}
