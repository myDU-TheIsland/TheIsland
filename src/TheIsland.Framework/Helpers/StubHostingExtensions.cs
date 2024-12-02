// <copyright file="StubHostingExtensions.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Framework.Helpers
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public static class StubHostingExtensions
    {
        public static async Task StartServicesV2(
            this IServiceProvider provider,
            CancellationToken externalToken = default)
        {
            Console.WriteLine(nameof(StartServicesV2));
            Log.Information(nameof(StartServicesV2));

            CancellationTokenSource? cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            cts.CancelAfter(TimeSpan.FromSeconds(600.0));
            foreach (IHostedService service in provider.GetServices<IHostedService>())
            {
                string output = $@"Starting {service.GetType()}";

                Console.WriteLine(output);
                Log.Information(output);
                try
                {
                    await service.StartAsync(cts.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }

            cts = null;
        }
    }
}
