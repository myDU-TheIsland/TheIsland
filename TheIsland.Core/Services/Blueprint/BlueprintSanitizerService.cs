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
            using MemoryStream memoryStream = new MemoryStream(blueprintBytes);
            using StreamReader streamReader = new StreamReader(memoryStream);
#pragma warning disable CAC001
            await using JsonTextReader textReader = new JsonTextReader(streamReader);
#pragma warning restore CAC001

            // ReSharper disable once AccessToStaticMemberViaDerivedType
            JToken bp = await JObject.ReadFromAsync(textReader, cancellationToken).ConfigureAwait(false);

            JToken? model = bp["Model"];
            if (model == null)
            {
                return BlueprintSanitationResult.Failed("Not a valid BP");
            }

            model["FreeDeploy"] = false;

            if (bp["Model"]?["JsonProperties"] == null)
            {
                return BlueprintSanitationResult.Failed("BP is Missing JsonProperties");
            }

            JToken jsonPropObj = bp["Model"]?["JsonProperties"] !;
            jsonPropObj["isNPC"] = false;
            jsonPropObj["isUntargetable"] = false;
            jsonPropObj["planetProperties"] = null;

            JToken? serverProps = bp["Model"]?["JsonProperties"]?["serverProperties"];
            if (serverProps != null)
            {
                serverProps["isFixture"] = null;
                serverProps["isBase"] = null;
                serverProps["isFlaggedForModeration"] = null;
                serverProps["isDynamicWreck"] = false;
                serverProps["fuelType"] = null;
                serverProps["fuelAmount"] = null;
                serverProps["compacted"] = false;
                serverProps["dynamicFixture"] = null;
                serverProps["constructCloneSource"] = null;
                serverProps["rdmsTags"] = JObject.FromObject(new
                {
                    constructTags = Array.Empty<object>(),
                    elementsTags = Array.Empty<object>(),
                });
            }

            if (bp["Elements"] == null)
            {
                return BlueprintSanitationResult.Succeeded(blueprintBytes);
            }

            JToken elementsToken = bp["Elements"] !;

            foreach (JToken item in elementsToken)
            {
                JToken elementType = item["elementType"] !;
                ulong elementTypeULong = elementType.Value<ulong>();

                if (item["properties"] is not JArray properties)
                {
                    continue;
                }

                foreach (JToken prop in properties)
                {
                    if (prop is not JArray)
                    {
                        continue;
                    }

                    string propName = prop[0] !.ToString();
                    JToken propValue = prop[1] !;

                    prop[1] = this.GetDefaultValue(bank, elementTypeULong, propName, propValue);
                }
            }

            string jsonString = bp.ToString();
            byte[] result = Encoding.Default.GetBytes(jsonString);

            return BlueprintSanitationResult.Succeeded(result);
        }

        private JToken GetDefaultValue(IGameplayBank bank, ulong elementType, string propName, JToken value)
        {
            Element? obj = bank.GetBaseObject<Element>(elementType);
            IGameplayDefinition? def = bank.GetDefinition(elementType);

            if (def == null || obj == null)
            {
                return value;
            }

            if (obj.hidden)
            {
                throw new InvalidOperationException("BP has hidden element");
            }

            NQ.PropertyValue? propVal = def.GetStaticPropertyOpt(propName);
            if (propVal != null)
            {
                return JObject.FromObject(new { type = (int)propVal.type, propVal.value });
            }

            return value;
        }
    }
}