using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Log;

namespace OpenNefia.Core.Areas
{
    public interface IGlobalAreaSystem : IEntitySystem
    {
        IReadOnlyDictionary<GlobalAreaId, PrototypeId<EntityPrototype>> GlobalAreaPrototypes { get; }

        void InitializeGlobalAreas(IEnumerable<GlobalAreaId> globalAreaIds);
        IArea GetOrCreateGlobalArea(GlobalAreaId globalAreaId);
    }

    public sealed class GlobalAreaSystem : EntitySystem, IGlobalAreaSystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMapTransferSystem _mapTransfer = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;

        private Dictionary<GlobalAreaId, PrototypeId<EntityPrototype>> _globalAreaPrototypes = new();
        public IReadOnlyDictionary<GlobalAreaId, PrototypeId<EntityPrototype>> GlobalAreaPrototypes => _globalAreaPrototypes;

        public override void Initialize()
        {
            SubscribeBroadcast<BeforeNewGameStartedEventArgs>(InitializeGlobalAreas);

            InitializeGlobalAreaPrototypes();
        }

        private void InitializeGlobalAreaPrototypes()
        {
            _globalAreaPrototypes.Clear();

            foreach (var (globalAreaId, areaEntityProto) in EnumerateGlobalAreaPrototypes())
            {
                if (_globalAreaPrototypes.TryGetValue(globalAreaId, out var proto))
                {
                    throw new InvalidDataException($"{globalAreaId} already registered as a global area (prototype: {proto}");
                }

                _globalAreaPrototypes[globalAreaId] = areaEntityProto.GetStrongID();
            }
        }

        private void InitializeGlobalAreas(BeforeNewGameStartedEventArgs ev)
        {
            InitializeGlobalAreas(ev.Scenario.InitGlobalAreas);
        }

        /// <summary>
        /// Initializes a list of global areas and loads/saves their starting floors. For use with
        /// town/economy initialization when starting a new game.
        /// </summary>
        /// <remarks>
        /// This replicates some code called by *renew_economy in HSP
        /// </remarks>
        public void InitializeGlobalAreas(IEnumerable<GlobalAreaId> globalAreaIds)
        {
            var list = globalAreaIds.ToList();

            if (list.Count > 0)
            {
                Logger.DebugS("sys.globalAreas", "Initializing these global areas:");
                foreach (var globalAreaId in list)
                {
                    Logger.DebugS("sys.globalAreas", $"  - {globalAreaId}");
                }
            }

            foreach (var globalAreaId in list)
            {
                if (!_areaManager.TryGetGlobalArea(globalAreaId, out var area))
                {
                    area = GetOrCreateGlobalArea(globalAreaId);
                }
                if (area.ContainedMaps.Count == 0)
                {
                    // TODO might want to make this behavior configurable
                    InitializeStartingFloorOfArea(area); 
                }
            }
        }

        /// <summary>
        /// Do an initialize-only load of a map to generate initial quests, etc.
        /// </summary>
        /// <param name="area"></param>
        private void InitializeStartingFloorOfArea(IArea area)
        {
            if (TryComp<AreaEntranceComponent>(area.AreaEntityUid, out var areaEntrance))
            {
                var map = _areaManager.GetOrGenerateMapForFloor(area.Id, areaEntrance.StartingFloor)!;
                if (map != null && TryComp<MapCommonComponent>(map.MapEntityUid, out var mapCommon) && !mapCommon.IsTemporary)
                {
                    _mapTransfer.RunMapInitializeEvents(map, MapLoadType.InitializeOnly);
                    _mapLoader.SaveMap(map.Id, _saveGameManager.CurrentSave!);
                    _mapManager.UnloadMap(map.Id);
                }
            }
        }

        public IArea GetOrCreateGlobalArea(GlobalAreaId globalAreaId)
        {
            if (_areaManager.TryGetGlobalArea(globalAreaId, out var area))
                return area;

            if (!GlobalAreaPrototypes.TryGetValue(globalAreaId, out var areaId))
                throw new InvalidDataException($"{globalAreaId} does not exist");

            area = _areaManager.CreateArea(areaId, globalAreaId);

            return area;
        }

        private IEnumerable<(GlobalAreaId, EntityPrototype)> EnumerateGlobalAreaPrototypes()
        {
            foreach (var proto in _protos.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.Components.TryGetComponent<AreaEntranceComponent>(out var areaEntrance))
                {
                    if (areaEntrance.InitialGlobalId != null)
                    {
                        yield return (areaEntrance.InitialGlobalId.Value, proto);
                    }
                }
            }
        }
    }
}