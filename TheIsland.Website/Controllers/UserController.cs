// <copyright file="UserController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Core.Classes;

    public class UserController : Controller
    {
        private readonly IDUClient _client;

        public UserController(IDUClient client)
        {
            this._client = client;
        }

        public IActionResult Index()
        {
            this._client.SendMessage(10000, "hello");
            return this.RedirectToAction("Index", "Home");
        }
    }
}
