using OpenNefia.Content.Areas;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.SaveGames;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Areas
{
    internal interface IGlobalAreaSystem : IEntitySystem
    {
        IReadOnlyDictionary<GlobalAreaId, PrototypeId<EntityPrototype>> GlobalAreaPrototypes { get; }

        void InitializeGlobalAreas(bool load);
        IArea GetOrCreateGlobalArea(GlobalAreaId globalAreaId);
    }

    internal sealed class GlobalAreaSystem : EntitySystem, IGlobalAreaSystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IMapTransferSystem _mapTransfer = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;

        private Dictionary<GlobalAreaId, PrototypeId<EntityPrototype>> _globalAreaPrototypes = new();
        public IReadOnlyDictionary<GlobalAreaId, PrototypeId<EntityPrototype>> GlobalAreaPrototypes => _globalAreaPrototypes;

        /// <summary>
        /// Does one-time setup of global areas. This is for setting up areas like towns to
        /// be able to generate escort/other quests between them when a new save is being 
        /// initialized.
        /// </summary>
        public void InitializeGlobalAreas(bool load)
        {
            _globalAreaPrototypes.Clear();

            foreach (var (globalAreaId, areaEntityProto) in EnumerateGlobalAreas())
            {
                if (_globalAreaPrototypes.TryGetValue(globalAreaId, out var proto))
                {
                    throw new InvalidDataException($"{globalAreaId} already registered as a global area (prototype: {proto}");
                }

                _globalAreaPrototypes[globalAreaId] = areaEntityProto.GetStrongID();
            }

            if (!load)
                return;

            foreach (var globalAreaId in GlobalAreaPrototypes.Keys)
            {
                GetOrCreateGlobalArea(globalAreaId);
            }
        }

        public IArea GetOrCreateGlobalArea(GlobalAreaId globalAreaId)
        {
            if (_areaManager.TryGetGlobalArea(globalAreaId, out var area))
                return area;

            if (!GlobalAreaPrototypes.TryGetValue(globalAreaId, out var areaId))
                throw new InvalidDataException($"{globalAreaId} does not exist");

            area = _areaManager.CreateArea(areaId, globalAreaId);

            // Initialize towns such that quests are generated between them.
            // TODO generalize this
            if (TryComp<AreaEntranceComponent>(area.AreaEntityUid, out var areaEntrance) && areaEntrance.StartingFloor != null)
            {
                var map = _areaManager.GetOrGenerateMapForFloor(area.Id, areaEntrance.StartingFloor.Value)!;
                if (map != null && TryComp<MapCommonComponent>(map.MapEntityUid, out var mapCommon) && !mapCommon.IsTemporary)
                {
                    _mapTransfer.RunMapInitializeEvents(map, MapLoadType.InitializeOnly);
                    _mapLoader.SaveMap(map.Id, _saveGameManager.CurrentSave!);
                    _mapManager.UnloadMap(map.Id);
                }
            }

            return area;
        }

        private IEnumerable<(GlobalAreaId, EntityPrototype)> EnumerateGlobalAreas()
        {
            foreach (var proto in _protos.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.Components.TryGetComponent<AreaEntranceComponent>(out var areaEntrance))
                {
                    if (areaEntrance.GlobalId != null)
                    {
                        yield return (areaEntrance.GlobalId.Value, proto);
                    }
                }
            }
        }
    }
}