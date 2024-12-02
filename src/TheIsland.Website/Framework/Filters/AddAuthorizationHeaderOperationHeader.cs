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
            IList<object> actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            bool isAPIKey = actionMetadata.Any(metadataItem => metadataItem is ApiKeyAttribute);
            bool isAuthorize = actionMetadata.Any(metadataItem => metadataItem is AuthorizeAttribute);
            bool allowAnonymous = actionMetadata.Any(metadataItem => metadataItem is AllowAnonymousAttribute);

            if ((!isAPIKey && !isAuthorize) || allowAnonymous)
            {
                return;
            }

            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Security = new List<OpenApiSecurityRequirement>();

            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            if (isAPIKey)
            {
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
}
