// <copyright file="BlueprintSanitizerService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services.Blueprint
{
    using System.Text;
    using Backend;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NQutils.Def;

    public class BlueprintSanitizerService : IBlueprintSanitizerService
    {
        public async Task<BlueprintSanitationResult> SanitizeAsync(IGameplayBank bank, byte[] blueprintBytes, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream(blueprintBytes);
            using var streamReader = new StreamReader(memoryStream);
#pragma warning disable CAC001
            await using var textReader = new JsonTextReader(streamReader);
#pragma warning restore CAC001

            // ReSharper disable once AccessToStaticMemberViaDerivedType
            var bp = await JObject.ReadFromAsync(textReader, cancellationToken).ConfigureAwait(false);

            if (bp["Elements"] == null)
            {
                return BlueprintSanitationResult.Succeeded(blueprintBytes);
            }

            var elementsToken = bp["Elements"] !;

            foreach (var item in elementsToken)
            {
                var elementType = item["elementType"] !;
                var elementTypeULong = elementType.Value<ulong>();

                if (item["properties"] is not JArray properties)
                {
                    continue;
                }

                foreach (var prop in properties)
                {
                    if (prop is not JArray)
                    {
                        continue;
                    }

                    var propName = prop[0] !.ToString();
                    var propValue = prop[1] !;

                    prop[1] = this.GetDefaultValue(bank, elementTypeULong, propName, propValue);
                }
            }

            var jsonString = bp.ToString();
            var result = Encoding.Default.GetBytes(jsonString);

            return BlueprintSanitationResult.Succeeded(result);
        }

        private JToken GetDefaultValue(IGameplayBank bank, ulong elementType, string propName, JToken value)
        {
            var obj = bank.GetBaseObject<Element>(elementType);
            var def = bank.GetDefinition(elementType);

            if (def == null || obj == null)
            {
                return value;
            }

            if (obj.hidden)
            {
                throw new InvalidOperationException("BP has hidden element");
            }

            var propVal = def.GetStaticPropertyOpt(propName);
            if (propVal != null)
            {
                return JObject.FromObject(new { type = (int)propVal.type, propVal.value });
            }

            return value;
        }
    }
}