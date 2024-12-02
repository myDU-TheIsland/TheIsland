// <copyright file="Initializer.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Framework
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using TheIsland.Core.Interfaces;
    using TheIsland.Framework.Bots;
    using TheIsland.Framework.Services;

    public static class Initializer
    {
        public static void InitializeFramework()
        {
            NQutils.Config.Config.ReadYamlFile("mod", "./dual.yaml");
        }

        public static IServiceCollection AddFrameworkDependencies(this IServiceCollection services)
        {
            Type[] types = Assembly.Load("TheIsland.Framework").GetTypes()
                .Where(type => typeof(IAppService).IsAssignableFrom(type) && !type.IsInterface)
                .ToArray();

            foreach (Type? typeDefinition in types)
            {
                Type type = typeDefinition.GetInterfaces().First();

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
