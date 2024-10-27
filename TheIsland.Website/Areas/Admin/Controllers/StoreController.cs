// <copyright file="StoreController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Areas.Admin.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Models.Admin;
    using TheIsland.Website.Models.Store;

    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("~/[area]/[controller]/[action]")]
    public class StoreController : IslandController
    {
        private readonly IStoreService _storeService;
        private readonly StorePurchaseHistoryRepository _storePurchaseHistoryRepository;

        public StoreController(
            IStoreService storeService,
            StorePurchaseHistoryRepository storePurchaseHistoryRepository,
            PlayerLinkingService playerLinkingService) : base(playerLinkingService)
        {
            this._storeService = storeService;
            this._storePurchaseHistoryRepository = storePurchaseHistoryRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var model = (await this._storePurchaseHistoryRepository.GetAsync().ConfigureAwait(false))?.ToArray() ?? Array.Empty<StorePurchaseHistory>();

            return this.View(model);
        }

        [HttpGet]
        [Route("~/[area]/[controller]/[action]/{purchaseId}")]
        public async Task<IActionResult> ViewLog(double purchaseId)
        {
            var model = (await this._storePurchaseHistoryRepository.GetAsync(purchaseId).ConfigureAwait(false)) ?? new StorePurchaseHistory();

            return this.View(model);
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
        [HttpPost]
        public async Task<IActionResult> Edit(StoreItemModel model)
        {
            model.StoreItem = await this._storeService.GetItem(model.StoreItem.id).ConfigureAwait(false);

            model.StoreItem ??= new StoreItemEntity();

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveItem(StoreItemModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("Edit", model);
            }

            if (model.Images.Count != 0)
            {
                foreach (IFormFile image in model.Images)
                {
                    model.StoreItem.images.Add(await this._storeService.SaveImage(image).ConfigureAwait(false));
                }
            }

            if (model.StoreItem.id == 0)
            {
                await this._storeService.CreateItem(model.StoreItem).ConfigureAwait(false);
            }
            else
            {
                await this._storeService.EditItem(model.StoreItem).ConfigureAwait(false);
            }

            return this.RedirectToAction("List");
        }
    }
}
