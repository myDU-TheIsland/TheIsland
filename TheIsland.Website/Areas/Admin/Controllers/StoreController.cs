// <copyright file="StoreController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Services;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Models.Admin;

    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("~/[area]/[controller]/[action]")]
    public class StoreController : IslandController
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService, PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
            this._storeService = storeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> List(StoreSearchModel? model)
        {
            if (model == null)
            {
                model ??= new StoreSearchModel();
            }
            else
            {
                model.StoreItems = await this._storeService.GetItemList(true).ConfigureAwait(false);
            }

            return this.View(model);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return this.View("Edit", new StoreItemModel());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(double id)
        {
            var model = new StoreItemModel();

            model.StoreItem = await this._storeService.GetItem(id).ConfigureAwait(false);

            return this.View(model);
        }

        [HttpPost]
        public IActionResult SaveItem(StoreItemModel model)
        {
            return this.View();
        }
    }
}
