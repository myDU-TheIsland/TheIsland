// <copyright file="IslandController.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Classes
{
    using Microsoft.AspNetCore.Mvc;

    public class IslandController : Controller
    {
        protected double DiscordId => double.Parse(this.HttpContext.User.Claims.FirstOrDefault(item => item.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "-1");
    }
}
