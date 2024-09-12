// <copyright file="PlayerRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Services.SQL
{
    using TheIsland.Website.Classes;
    using TheIsland.Website.Services.SQL.Entities;

    public class PlayerRepository : EntityRepository<Player>
    {
        public PlayerRepository(PostgresSettings settings) : base(settings, settings.DualDatabase)
        {
        }
    }
}
