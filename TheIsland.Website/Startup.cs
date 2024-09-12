// <copyright file="Startup.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website
{
    using System.Globalization;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using TheIsland.Core.Classes;
    using TheIsland.Core.Settings;
    using TheIsland.Website.Classes;
    using TheIsland.Website.Interfaces;
    using TheIsland.Website.Services.SQL;

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
                .AddJsonFile("website.json", true, true)
                .AddJsonFile($"website.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();
            this.HostingEnvironment = env;

            // Build settings object (pulls from appsettings.*)
            SiteSettings setupSettings = new SiteSettings();
            this.Configuration.GetSection("Site").Bind(setupSettings);
            SiteSettings = setupSettings;
        }

        /// <summary>
        /// Gets or sets the site settings.
        /// </summary>
        /// <value>The setup settings.</value>
        public static ISiteSettings SiteSettings { get; set; } = new SiteSettings();

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
            services.AddSingleton<DualUniverseSettings>(SiteSettings.DualUniverse);
            services.AddSingleton<IDUClient, DUClient>();
            services.AddSingleton<UserMappingRepository>(new UserMappingRepository(SiteSettings.Postgres));
            services.AddSingleton<PlayerRepository>(new PlayerRepository(SiteSettings.Postgres));
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
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

            app.UseHttpsRedirection();

            // Required to serve files with no extension in the .well-known folder
            var options = new StaticFileOptions()
            {
                ServeUnknownFileTypes = true,
            };

            app.UseStaticFiles(options);

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "MyArea",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}