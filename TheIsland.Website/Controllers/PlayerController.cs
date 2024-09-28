// <copyright file="PlayerController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Website.Classes;

    public class PlayerController : IslandController
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateAccount()
        {
            return this.View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordReset()
        {
            return this.View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult RequestPasswordReset()
        {
            return this.View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return this.View();
        }
    }
}
