// <copyright file="ApiKeyAuthorizationFilter.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Framework.Filters
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using TheIsland.Website.Interfaces;

    public class ApiKeyAuthorizationFilter : IAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        private readonly ISiteSettings _siteSettings;

        public ApiKeyAuthorizationFilter(ISiteSettings siteSettings)
        {
            this._siteSettings = siteSettings;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string apiKey = context.HttpContext?.Request?.Headers[ApiKeyHeaderName].FirstOrDefault() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(apiKey) || !this._siteSettings.ApiKey.Contains(apiKey))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
