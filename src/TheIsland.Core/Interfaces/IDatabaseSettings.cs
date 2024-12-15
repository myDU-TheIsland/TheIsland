// <copyright file="IDatabaseSettings.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Interfaces
{
    public interface IDatabaseSettings
    {
        string Server { get; set; }

        int Port { get; set; }

        string UserName { get; set; }

        string Password { get; set; }

        string Database { get; set; }

        string DualDatabase { get; set; }

        string ConnectionString { get; set; }

        string GetConnectionString();
    }
}
