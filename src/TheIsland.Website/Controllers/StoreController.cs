// <copyright file="StoreController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Models.Store;

    [Authorize(Policy = "User")]
    [Route("~/[controller]/[action]")]
    public class StoreController : IslandController
    {
        private readonly IStoreService _storeService;
        private readonly StorePurchaseHistoryRepository _storePurchaseHistoryRepository;

        public StoreController(
            IStoreService storeService,
            StorePurchaseHistoryRepository storePurchaseHistoryRepository,
            PlayerLinkingService playerLinkingService,
            IAuthorizationService authorizationService) : base(playerLinkingService, authorizationService)
        {
            this._storeService = storeService;
            this._storePurchaseHistoryRepository = storePurchaseHistoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(StoreSearchModel model)
        {
            model ??= new StoreSearchModel();

            model.StoreItems = await this._storeService.GetItemList(false).ConfigureAwait(false);

            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var model = (await this._storePurchaseHistoryRepository.GetPlayerPurchaseHistory(this.SelectedPlayer).ConfigureAwait(false))?.ToArray() ?? Array.Empty<StorePurchaseHistory>();

            return this.View(model);
        }

        [HttpGet]
        [Route("~/[controller]/[action]/{itemId:double}")]
        public async Task<IActionResult> View(double itemId)
        {
            ViewStoreItemModel model = new ViewStoreItemModel();

            model.StoreItem = await this._storeService.GetItem(itemId).ConfigureAwait(false);

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(StorePurchaseModel purchaseModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("Index");
            }

            var result = await this._storeService.PurchaseItem(this.SelectedPlayer, this.DiscordId, purchaseModel.ItemId, purchaseModel.Quantity).ConfigureAwait(false);

            if (result.success)
            {
                return this.RedirectToAction("CompletedPurchase", new { purchaseId = result.id });
            }

            return this.RedirectToAction("FailedPurchase", new { purchaseId = result.id });
        }

        [HttpGet]
        public async Task<IActionResult> CompletedPurchase(double purchaseId)
        {
            var model = await this._storePurchaseHistoryRepository.GetAsync(purchaseId).ConfigureAwait(false);

            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FailedPurchase(double purchaseId)
        {
            var model = await this._storePurchaseHistoryRepository.GetAsync(purchaseId).ConfigureAwait(false);

            return this.View(model);
        }
    }
}
