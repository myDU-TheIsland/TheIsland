// <copyright file="ApiKeyAttribute.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Framework.Attributes
{
    using Microsoft.AspNetCore.Mvc;
    using TheIsland.Website.Framework.Filters;

    public class ApiKeyAttribute : ServiceFilterAttribute
    {
        public ApiKeyAttribute()
            : base(typeof(ApiKeyAuthorizationFilter))
        {
        }
    }
}
