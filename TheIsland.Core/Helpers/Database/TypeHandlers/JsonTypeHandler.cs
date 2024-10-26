// <copyright file="JsonTypeHandler.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>
// <remarks>https://dev.to/byme8/making-dapper-and-json-friends-5afc / https://github.com/byme8/Dapper.Json</remarks>

namespace TheIsland.Core.Helpers.Database.TypeHandlers
{
    using System.Data;
    using System.Text.Json;
    using Dapper;

    public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        public override void SetValue(IDbDataParameter parameter, T? value)
        {
            parameter.Value = JsonSerializer.Serialize(value);
        }

        public override T? Parse(object value)
        {
            if (value is string v)
            {
                return JsonSerializer.Deserialize<T>(v);
            }

            return default;
        }
    }
}
