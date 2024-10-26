// <copyright file="Initializer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core
{
    using System.Reflection;
    using Dapper;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Helpers.Database.TypeHandlers;
    using TheIsland.Core.Services;
    using TheIsland.Core.Services.SQL;

    public static class Initializer
    {
        public static void InitializeCore()
        {
            NQutils.Config.Config.ReadYamlFile("mod", "./dual.yaml");

            // Initialize Custom SQL Mappers.
            SqlMapper.AddTypeHandler(typeof(List<string>), new JsonTypeHandler<List<string>>());
            SqlMapper.AddTypeHandler(typeof(StoreItemContent), new JsonTypeHandler<StoreItemContent>());
        }

        public static IServiceCollection AddCoreDependencies(this IServiceCollection services)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assemblies => assemblies.GetTypes())
                .Where(type => (type.BaseType?.IsGenericType ?? false) && type.BaseType.GetGenericTypeDefinition() == typeof(EntityRepository<>))
                .ToArray();

            foreach (var typeDefinition in types)
            {
                Console.WriteLine($@"Adding Singleton Repository : {typeDefinition.Name}");
                services.AddSingleton(typeDefinition);
            }

            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assemblies => assemblies.GetTypes())
                .Where(type => typeof(IAppService).IsAssignableFrom(type) && !type.IsInterface)
                .ToArray();

            foreach (var typeDefinition in types)
            {
                var type = typeDefinition.GetInterfaces().First();

                if (type == typeof(IAppService))
                {
                    Console.WriteLine($@"Adding Singleton Repository : {typeDefinition.Name}");
                    services.AddSingleton(typeDefinition);
                }
                else
                {
                    Console.WriteLine($@"Adding Singleton Repository : {type.Name}");
                    services.AddSingleton(type, typeDefinition);
                }
            }

            services.AddSingleton<IMarketBot, MarketBot>();
            services.AddSingleton<IGeneralBot, GeneralBot>();

            return services;
        }
    }
}
