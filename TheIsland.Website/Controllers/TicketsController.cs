// <copyright file="TicketsController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class TicketsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
