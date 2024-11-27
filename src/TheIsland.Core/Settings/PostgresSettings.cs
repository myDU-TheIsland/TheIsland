// <copyright file="PostgresSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Settings
{
    public class PostgresSettings
    {
        public string Server { get; set; } = "localhost";

        public int Port { get; set; } = 5432;

        public string UserName { get; set; } = "dual";

        public string Password { get; set; } = "dual";

        public string Database { get; set; } = "TheIsland";

        public string DualDatabase { get; set; } = "dual";

        public string ConnectionString { get; set; } = string.Empty;

        public string GetConnectionString()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString))
            {
                this.ConnectionString = $@"Host={this.Server};Port={this.Port};Username={this.UserName};Password={this.Password};";
            }

            return this.ConnectionString;
        }
    }
}
