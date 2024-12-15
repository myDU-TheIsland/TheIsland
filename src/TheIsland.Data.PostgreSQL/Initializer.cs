// <copyright file="Initializer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Data.PostgreSQL
{
    using System.Reflection;
    using Dapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TheIsland.Core.Interfaces;
    using TheIsland.Data.Entities;
    using TheIsland.Data.PostgreSQL.Settings;
    using TheIsland.Data.PostgreSQL.TypeHandlers;

    public static class Initializer
    {
        public static void InitializePostgre()
        {
            // Initialize Custom SQL Mappers.
            SqlMapper.AddTypeHandler(typeof(List<string>), new JsonTypeHandler<List<string>>());
            SqlMapper.AddTypeHandler(typeof(Dictionary<double, double>), new JsonTypeHandler<Dictionary<double, double>>());
            SqlMapper.AddTypeHandler(typeof(StoreItemContent), new JsonTypeHandler<StoreItemContent>());
            SqlMapper.AddTypeHandler(typeof(StoreItem), new JsonTypeHandler<StoreItem>());
        }

        public static IDatabaseSettings GetDatabaseSettings(IConfigurationRoot config)
        {
            PostgresSettings databaseSettings = new PostgresSettings();
            config.GetSection("Database").Bind(databaseSettings);
            return databaseSettings;
        }

        public static IServiceCollection AddPostgreDependencies(this IServiceCollection services, IConfigurationRoot config)
        {
            services.AddSingleton(GetDatabaseSettings(config));

            Type[] types = Assembly.Load("TheIsland.Data.PostgreSQL").GetTypes()
                .Where(type => (type.BaseType?.IsGenericType ?? false) && type.BaseType.GetGenericTypeDefinition() == typeof(NpgsqlEntityRepository<>))
                .ToArray();

            foreach (Type? typeDefinition in types)
            {
                Type[] interfaces = typeDefinition.GetInterfaces();
                Console.WriteLine($@"Adding Singleton Repository : {typeDefinition.Name}");
                services.AddSingleton(interfaces.Last(), typeDefinition);
            }

            return services;
        }
    }
}
