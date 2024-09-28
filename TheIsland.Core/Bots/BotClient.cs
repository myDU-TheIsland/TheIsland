// <copyright file="BotClient.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Bots
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Backend;
    using Backend.Business;
    using Backend.Database;
    using BotLib.BotClient;
    using BotLib.Generated;
    using BotLib.Protocols;
    using BotLib.Protocols.Queuing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NQ;
    using NQ.Router;
    using NQutils;
    using NQutils.Sql;
    using Orleans;
    using TheIsland.Core.Settings;

    public interface IBotClient
    {
        Client Bot { get; }

        Task BotConnectionTest();

        Task SendMessage(ulong who, string message);

        Task<string> GetConstructName(ulong constructId);

        Task<string> SetConstructName(ulong constructId, string name);
    }

    public class BotClient : IBotClient
    {
        public Client Bot { get; set; }

        internal IDuClientFactory RestDuClientFactory => this.ServiceProvider.GetRequiredService<IDuClientFactory>();

        internal IServiceProvider ServiceProvider { get; }

        internal IClusterClient Orleans { get; }

        internal IDataAccessor DataAccessor { get; }

        internal BotSettings Settings { get; }

        protected virtual ILogger<IBotClient> Logger { get; }

        public BotClient(BotSettings settings, ILogger<IBotClient> logger)
        {
            this.Settings = settings;
            this.Logger = logger;

            ServiceCollection services = new ServiceCollection();

            if (string.IsNullOrEmpty(this.Settings.QueueingUri))
            {
                this.Settings.QueueingUri = "http://queueing:9630";
            }

            services
                .AddSingleton<ISql, Sql>()
                .AddInitializableSingleton<IGameplayBank, GameplayBank>()
                .AddSingleton<ILocalizationManager, LocalizationManager>()
                .AddTransient<IDataAccessor, DataAccessor>()
                .AddOrleansClient("IntegrationTests")
                .AddHttpClient()
                .AddTransient<NQutils.Stats.IStats, NQutils.Stats.FakeIStats>()
                .AddSingleton<IQueuing, RealQueuing>(sp => new RealQueuing(this.Settings.QueueingUri, sp.GetRequiredService<IHttpClientFactory>().CreateClient()))
                .AddSingleton<IDuClientFactory, BotLib.Protocols.GrpcClient.DuClientFactory>();

            ServiceProvider servProvider = services.BuildServiceProvider();
            this.ServiceProvider = servProvider;
            this.ServiceProvider.StartServices().Wait();
            ClientExtensions.SetSingletons(servProvider);
            ClientExtensions.UseFactory(servProvider.GetRequiredService<IDuClientFactory>());
            this.Orleans = this.ServiceProvider.GetRequiredService<IClusterClient>();
            this.DataAccessor = this.ServiceProvider.GetRequiredService<IDataAccessor>();

            this.Bot = this.CreateBotUser(this.Settings).GetAwaiter().GetResult();
        }

        internal Task<Client> CreateBotUser(BotSettings settings)
        {
            LoginInformations pi = LoginInformations.BotLogin(settings.PlayerName, settings.BotUser, settings.BotPassword);
            return Client.FromFactory(this.RestDuClientFactory, pi, allowExising: true);
        }

        public async Task BotConnectionTest()
        {
            try
            {
                await this.Bot.Req.GetWallet().ConfigureAwait(false);
            }
            catch (NQutils.Exceptions.BusinessException be) when (be.error.code == NQ.ErrorCode.InvalidSession)
            {
                Console.WriteLine("reconnecting");
                this.Bot = await this.CreateBotUser(this.Settings).ConfigureAwait(false);
                await Task.Delay(10000).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in mod action: {e}\n{e.StackTrace}");
                await Task.Delay(10000).ConfigureAwait(false);
            }
        }

        public async Task SendMessage(ulong who, string message)
        {
            await this.BotConnectionTest().ConfigureAwait(false);
            await this.Bot.Req.ChatMessageSend(new MessageContent
            {
                channel = new MessageChannel
                {
                    channel = MessageChannelType.PRIVATE,
                    targetId = who,
                },
                message = message,
            }).ConfigureAwait(false);
        }

        public async Task<string> GetConstructName(ulong constructId)
        {
            await this.BotConnectionTest().ConfigureAwait(false);
            try
            {
                ConstructTree results = await this.Bot.Req.ConstructTreeGet(constructId).ConfigureAwait(false);

                return results.constructs[0].name;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public async Task<string> SetConstructName(ulong constructId, string name)
        {
            await this.BotConnectionTest().ConfigureAwait(false);
            try
            {
                await this.Bot.Req.ConstructRename(new ConstructNameSet()
                {
                    constructId = constructId,
                    newName = name,
                }).ConfigureAwait(false);

                ConstructTree results = await this.Bot.Req.ConstructTreeGet(constructId).ConfigureAwait(false);
                return results.constructs[0].name;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
    }
}