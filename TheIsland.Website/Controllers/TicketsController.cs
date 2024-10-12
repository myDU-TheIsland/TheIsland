// <copyright file="TicketsController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Website.Classes;

    public class TicketsController : IslandController
    {
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
