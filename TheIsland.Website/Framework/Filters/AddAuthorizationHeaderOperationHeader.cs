// <copyright file="AddAuthorizationHeaderOperationHeader.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Framework.Filters
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using TheIsland.Website.Framework.Attributes;

    public class AddAuthorizationHeaderOperationHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            var isAuthorized = actionMetadata.Any(metadataItem => metadataItem is ApiKeyAttribute);
            var allowAnonymous = actionMetadata.Any(metadataItem => metadataItem is AllowAnonymousAttribute);

            if (!isAuthorized || allowAnonymous)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            operation.Security = new List<OpenApiSecurityRequirement>();

            operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Api Key" },
                        },
                        Array.Empty<string>()
                    },
                });
        }
    }
}
