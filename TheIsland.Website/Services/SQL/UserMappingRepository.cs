// <copyright file="UserMappingRepository.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Website.Services.SQL
{
    using TheIsland.Website.Classes;
    using TheIsland.Website.Services.SQL.Entities;

    public class UserMappingRepository : EntityRepository<UserMapping>
    {
        public UserMappingRepository(PostgresSettings settings) : base(settings, settings.Database)
        {
        }
    }
}
