// <copyright file="Startup.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website
{
    using System;
    using System.Globalization;
    using Hangfire;
    using Hangfire.Dashboard;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using TheIsland.Core;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Services;
    using TheIsland.Core.Settings;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Framework.Activators;
    using TheIsland.Website.Framework.Filters;
    using TheIsland.Website.Interfaces;

    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        public Startup(IWebHostEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("websiteSettings.json", true, true)
                .AddJsonFile($"websiteSettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();
            this.HostingEnvironment = env;

            // Build settings object (pulls from appsettings.*)
            SiteSettings setupSettings = new SiteSettings();
            this.Configuration.GetSection("Site").Bind(setupSettings);
            SiteSettings = setupSettings;

            IConfigurationBuilder builder2 = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("marketBotSettings.json", true, true)
                .AddJsonFile($"marketBotSettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            IConfigurationRoot botConfig = builder2.Build();

            // Build settings object (pulls from appsettings.*)
            MarketBotConfig marketBotConfig = new MarketBotConfig();
            botConfig.Bind(marketBotConfig);
            MarketBotConfig = marketBotConfig;

            Core.Initializer.InitializeCore();
        }

        /// <summary>
        /// Gets or sets the site settings.
        /// </summary>
        /// <value>The setup settings.</value>
        public static ISiteSettings SiteSettings { get; set; } = new SiteSettings();

        /// <summary>
        /// Gets or sets the site settings.
        /// </summary>
        /// <value>The setup settings.</value>
        public static MarketBotConfig MarketBotConfig { get; set; } = new MarketBotConfig();

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Gets the current HostingEnvironment.
        /// </summary>
        public IWebHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
            services.AddMemoryCache();
            services.AddRouting();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/signin";
                options.LogoutPath = "/signout";
            })
            .AddDiscord(options =>
            {
                options.ClientId = SiteSettings.Discord.ClientId;
                options.ClientSecret = SiteSettings.Discord.ClientSecret;
                options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                        user.GetString("id"),
                        user.GetString("avatar"),
                        (user.GetString("avatar") ?? string.Empty).StartsWith("a_") ? "gif" : "png"));
            });

            services.AddSession();

            services.AddAuthorizationBuilder()
                .AddPolicy("Admin", policy => policy.RequireClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", SiteSettings.Admins))
                .AddPolicy("User", policy => policy.RequireClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"));

            if (this.HostingEnvironment.IsDevelopment())
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(@"./dpk"));
            }
            else
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(SiteSettings.DPKPath));

                services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;
                });
            }

            // settings
            services.AddSingleton(SiteSettings);
            services.AddSingleton(SiteSettings.DualUniverse);
            services.AddSingleton(SiteSettings.Postgres);
            services.AddSingleton(MarketBotConfig);
            services.AddSingleton<ApiKeyAuthorizationFilter>();

            // repositories
            services.AddCoreDependencies();

            if (this.HostingEnvironment.IsDevelopment())
            {
                services.AddMvc().AddRazorRuntimeCompilation();
            }
            else
            {
                services.AddMvc();
            }

            services.AddHangfire(opts => opts
               .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UseInMemoryStorage());

            // Add the processing server as IHostedService
            services.AddHangfireServer();
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            GlobalConfiguration.Configuration
                .UseActivator(new HangfireActivator(serviceProvider));

            // Configure the HTTP request pipeline.
            if (!this.HostingEnvironment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (this.HostingEnvironment.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseResponseCompression();
            }

            app.UseHttpsRedirection();
            app.UseSession();

            // Required to serve files with no extension in the .well-known folder
            StaticFileOptions options = new StaticFileOptions()
            {
                ServeUnknownFileTypes = true,
            };

            app.UseStaticFiles(options);

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "MyArea",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseHangfireDashboard("/admin/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthenticationFilter() },
                IsReadOnlyFunc = (DashboardContext context) => true,
            });

            if (!this.HostingEnvironment.IsDevelopment())
            {
                RecurringJob.AddOrUpdate("buyStuff", (IMarketBot client) => client.BuyStuff(0), Cron.Minutely);
                RecurringJob.AddOrUpdate("sellStuff", (MarketService service) => service.SellAllMarketsContainerContents(), Cron.Hourly);
                RecurringJob.AddOrUpdate("importMarketData", (IImportMarketService service) => service.ImportAsync(), "*/5 * * * *");
                RecurringJob.AddOrUpdate("hotTime", (MarketService service) => service.HotTimeEvent(), "0 */3 * * *");
            }
        }
    }
}