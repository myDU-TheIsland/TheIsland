// <copyright file="BlueprintService.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

namespace TheIsland.Core.Services
{
    using System.Threading.Tasks;
    using Backend;
    using Backend.Business;
    using Backend.Database;
    using Microsoft.Extensions.DependencyInjection;
    using NQ;
    using NQ.Interfaces;
    using NQutils.Sql;
    using Orleans;
    using TheIsland.Core.Bots;
    using TheIsland.Core.Entities;
    using TheIsland.Core.Services.Blueprint;
    using TheIsland.Core.Services.SQL;
    using TheIsland.Core.Settings;

    public interface IBlueprintService
    {
        Task<string> ImportBP(ulong playerId, byte[] bp);

        Task<Dictionary<string, ulong>> GetExportableBPs(ulong playerId);

        Task<bool> IsExportAllowed(ulong blueprintId, ulong playerId);

        Task<BluePrintExport?> SaveBP(ulong playerId, ulong blueprintId, string blueprintName);

        Task<BluePrintExport?> GetBP(Guid uuid, ulong playerId);

        Task<BluePrintExport[]> GetMyExportedBps(ulong playerId);

        string GetBPPath(Guid uuid);
    }

    public class BlueprintService : IBlueprintService
    {
        private readonly IGeneralBot _bot;
        private readonly ISql _sql;
        private readonly IGameplayBank _gameplayBank;
        private readonly IDataAccessor _dataAccessor;
        private readonly IClusterClient _orleans;
        private readonly BlueprintExportRepository _blueprintExportRepository;
        private readonly DualUniverseSettings _settings;

        private Task ConnectionTest() => this._bot.BotConnectionTest();

        public BlueprintService(BlueprintExportRepository blueprintExportRepository, DualUniverseSettings settings, IGeneralBot bot)
        {
            this._bot = bot;
            this._sql = bot.ServiceProvider.GetRequiredService<ISql>();
            this._gameplayBank = bot.ServiceProvider.GetRequiredService<IGameplayBank>();
            this._dataAccessor = bot.DataAccessor;
            this._orleans = bot.Orleans;
            this._settings = settings;
            this._blueprintExportRepository = blueprintExportRepository;
        }

        internal bool IsBlueprint(StorageSlot slot)
        {
            if (slot.content.id == 0)
            {
                return false;
            }

            ulong type = slot.content.type;
            IGameplayDefinition? bd = this._gameplayBank.GetDefinition(type);

            bool isValid = false;

            if (bd == null)
            {
                return false;
            }

            if (bd.Is<NQutils.Def.Blueprint>())
            {
                isValid = true;
            }

            return isValid;
        }

        public async Task<bool> IsExportAllowed(ulong blueprintId, ulong playerId)
        {
            ulong coreType = 0;

            var blueprintModel = await this._sql.Read(blueprintId).ConfigureAwait(false);

            var creator = blueprintModel.JsonProperties.serverProperties.creatorId;

            bool isMe = false;

            if (creator.playerId == 0 && creator.organizationId == 0)
            {
                //no known drm on it.
                isMe = true;
            }

            if (creator.playerId != 0 && creator.playerId == playerId)
            {
                //player is drm holder
                isMe = true;
            }

            if (creator.organizationId != 0)
            {
                // you are leagate of the org creator
                isMe = await this._orleans.GetOrganizationGrain(creator.organizationId).IsLegate(playerId).ConfigureAwait(false);
            }

            if (!isMe)
            {
                var requiredItems = await this._sql.GetIngredients(blueprintId).ConfigureAwait(false);
                foreach (var ri in requiredItems)
                {
                    if (this._gameplayBank.GetBaseObject<NQutils.Def.CoreUnit>(ri.id) != null)
                    {
                        coreType = ri.id;
                        break;
                    }
                }

                var coreDrm = await this._sql.BlueprintCoreDRMGet(blueprintId, coreType).ConfigureAwait(false);

                if (coreDrm)
                {
                    //core is drmed and you are not the creator.
                    return false;
                }
            }

            return true;
        }

        private bool IsBlueprintSanitationEnabled()
          => Environment.GetEnvironmentVariable("BP_SANITATION_ENABLED") == "true";

        public async Task<string> ImportBP(ulong playerId, byte[] bp)
        {
            if (this.IsBlueprintSanitationEnabled())
            {
                var sanitizer = new BlueprintSanitizerService();

                try
                {
                    var result = await sanitizer.SanitizeAsync(this._gameplayBank, bp, CancellationToken.None)
                        .ConfigureAwait(false);

                    if (!result.Success)
                    {
                        return result.Message;
                    }

                    bp = result.BlueprintBytes;
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }

            await this.ConnectionTest().ConfigureAwait(false);
            BlueprintId blueprintId = 0;
            try
            {
                blueprintId = await this._dataAccessor.BlueprintImport(bp, new EntityId { playerId = playerId }).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                return @$"Failed to import BP. ({exception.ToString()})";
            }

            BlueprintProperties blueprintInfo = await this._orleans.GetBlueprintGrain().GetBlueprintInfo(blueprintId).ConfigureAwait(false);
            BlueprintModel bluepprintModel = await this._sql.Read(blueprintId).ConfigureAwait(false);

            if (bluepprintModel.FreeDeploy && !await this._orleans.GetPlayerGrain(playerId).IsAdmin().ConfigureAwait(false))
            {
                return "You are not allowed to import free deploy blueprints";
            }

            IInventoryGrain pig = this._orleans.GetInventoryGrain(playerId);
            IGameplayDefinition? blueprintTypeInfo = this._gameplayBank.GetDefinition("Blueprint");

            if (blueprintTypeInfo == null)
            {
                return "System problem. Can't identify blueprint type id";
            }

            ItemInfo item = new ItemInfo
            {
                type = blueprintTypeInfo.Id,
                id = blueprintId,
            };
            item.properties.Add("name", new PropertyValue { stringValue = blueprintInfo.name });
            item.properties.Add("size", new PropertyValue { intValue = (long)blueprintInfo.size.x });
            item.properties.Add("static", new PropertyValue { boolValue = blueprintInfo.kind != ConstructKind.DYNAMIC });
            item.properties.Add("kind", new PropertyValue { intValue = (int)blueprintInfo.kind });

            await this._dataAccessor.PlayerInventoryGiveAsync(
                    playerId,
                    new ItemAndQuantity
                    {
                        item = item,
                        quantity = 1,
                    }).ConfigureAwait(false);

            return "Blueprint '" + blueprintInfo.name + "' imported and should be in your nano pack.";
        }

        public async Task<Dictionary<string, ulong>> GetExportableBPs(ulong playerId)
        {
            IInventoryGrain inventoryGrain = this._orleans.GetInventoryGrain(playerId);
            StorageInfo? inventory = await inventoryGrain.Get(playerId).ConfigureAwait(false);
            Dictionary<string, ulong> output = new Dictionary<string, ulong>();

            foreach (StorageSlot? item in inventory.content)
            {
                if (item == null)
                {
                    continue;
                }

                if (!this.IsBlueprint(item))
                {
                    //not a blueprint
                    continue;
                }

                var blueprintId = item.content.id;

                if (!await this.IsExportAllowed(blueprintId, playerId).ConfigureAwait(false))
                {
                    continue;
                }

                var blueprintModel = await this._sql.Read(blueprintId).ConfigureAwait(false);

                output.Add(blueprintModel.Name, blueprintId);
            }

            return output;
        }

        public async Task<BluePrintExport?> SaveBP(ulong playerId, ulong blueprintId, string blueprintName)
        {
            if (!await this.IsExportAllowed(blueprintId, playerId).ConfigureAwait(false))
            {
                return null;
            }

            try
            {
                var bin = await this._dataAccessor.BlueprintExport((long)blueprintId).ConfigureAwait(false);
                var uuid = Guid.NewGuid();

                if (!System.IO.Directory.Exists(Path.GetFullPath(this._settings.ExportPath)))
                {
                    System.IO.Directory.CreateDirectory(Path.GetFullPath(this._settings.ExportPath));
                }

                await System.IO.File.WriteAllBytesAsync(this.GetBPPath(uuid), bin).ConfigureAwait(false);
                BluePrintExport blueprintEntry = new BluePrintExport() { uuid = uuid, blueprint_id = blueprintId, player_id = playerId, blueprint_name = blueprintName,  };
                await this._blueprintExportRepository.AddAsync(blueprintEntry).ConfigureAwait(false);
                return blueprintEntry;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public async Task<BluePrintExport?> GetBP(Guid uuid, ulong playerId)
        {
            var blueprintData = await this._blueprintExportRepository.GetByUuidAsync(uuid).ConfigureAwait(false);

            if (blueprintData == null)
            {
                return null;
            }

            if (blueprintData.player_id != playerId)
            {
                return null;
            }

            return blueprintData;
        }

        public async Task<BluePrintExport[]> GetMyExportedBps(ulong playerId)
        {
            var results = await this._blueprintExportRepository.GetByPlayerIdAsync(playerId).ConfigureAwait(false);
            return results.ToArray();
        }

        public string GetBPPath(Guid uuid)
        {
            return Path.Combine(Path.GetFullPath(this._settings.ExportPath), $@"{uuid}.json");
        }
    }
}
