using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Log;
using OpenNefia.Core.Random;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Charas;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Areas;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Dungeons;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Utility;
using Love;
using System.ComponentModel;

namespace OpenNefia.Content.Nefia
{
    public interface IVanillaNefiaGenSystem : IEntitySystem
    {
        bool TryToGenerateFloor(IArea area, MapId mapId, int floorNumber, Blackboard<NefiaGenParams> data, [NotNullWhen(true)] out IMap? map);

        void ConnectStairs(IMap map, IArea area, int floorNumber, MapCoordinates previousCoords);
    }

    public sealed partial class VanillaNefiaGenSystem : EntitySystem, IVanillaNefiaGenSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapTilesetSystem _mapTilesets = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IMapPlacement _placement = default!;

        /// <summary>
        /// Maximum number of times the layout should be generated before giving up.
        /// </summary>
        private const int MAX_GENERATION_ATTEMPTS = 2000;

        public override void Initialize()
        {
            SubscribeLocalEvent<NefiaVanillaComponent, NefiaFloorGenerateEvent>(OnNefiaFloorGenerate, nameof(OnNefiaFloorGenerate));
            SubscribeLocalEvent<NefiaVanillaComponent, GenerateNefiaFloorParamsEvent>(SetupBaseParams, nameof(SetupBaseParams));
            SubscribeLocalEvent<NefiaVanillaComponent, GenerateNefiaFloorAttemptEvent>(GenerateFloorAttempt, nameof(GenerateFloorAttempt));
            SubscribeLocalEvent<NefiaVanillaComponent, AfterGenerateNefiaFloorEvent>(FinalizeNefia, nameof(FinalizeNefia));
        }

        /// <summary>
        /// Main event handler that generates the next nefia floor.
        /// </summary>
        private void OnNefiaFloorGenerate(EntityUid areaEnt, NefiaVanillaComponent nefiaStd, NefiaFloorGenerateEvent args)
        {
            if (args.Handled)
                return;

            // TODO maybe not hide information inside string IDs
            var floorNumber = AreaNefiaSystem.AreaIdToFloorNumber(args.FloorId);
            var mapId = _mapManager.GenerateMapId();

            var data = new Blackboard<NefiaGenParams>();
            data.Add(new BaseNefiaGenParams());

            EntitySystem.InjectDependencies(nefiaStd.Template);
            var layout = nefiaStd.Template.GetLayout(floorNumber, data);
            EntitySystem.InjectDependencies(layout);

            data.Add(new StandardNefiaGenParams(nefiaStd.Template, layout));

            if (!TryToGenerateFloor(args.Area, mapId, floorNumber, data, out var map))
            {
                Logger.ErrorS("nefia.gen", $"Failed to generate Nefia floor!");
                return;
            }

            ConnectStairs(map, args.Area, floorNumber, args.PreviousCoords);

            args.Handle(map);
        }

        /// <summary>
        /// Tries repeatedly to generate a nefia floor using a template.
        /// NOTE: <paramref name="data"/> must have <see cref="NefiaGenParams"/> and <see cref="StandardNefiaGenParams"/>!
        /// </summary>
        public bool TryToGenerateFloor(IArea area, MapId mapId, int floorNumber, Blackboard<NefiaGenParams> data, [NotNullWhen(true)] out IMap? map)
        {
            if (!TryToGenerateRaw(area, mapId, floorNumber, data, out map))
                return false;

            var ev = new AfterGenerateNefiaFloorEvent(area, map, data, floorNumber);
            EntityManager.EventBus.RaiseLocalEvent(area.AreaEntityUid, ev);

            return true;
        }

        private bool TryToGenerateRaw(IArea area, MapId mapId, int floorNumber, Blackboard<NefiaGenParams> data, [NotNullWhen(true)] out IMap? map)
        {
            for (var i = 0; i < MAX_GENERATION_ATTEMPTS; i++)
            {
                _mapManager.UnloadMap(mapId);
                _rand.RandomizeSeed();

                var width = 34 + _rand.Next(15);
                var height = 22 + _rand.Next(15);

                data.Get<BaseNefiaGenParams>().MapSize = (width, height);

                var paramsEv = new GenerateNefiaFloorParamsEvent(area, mapId, data, floorNumber, i);
                EntityManager.EventBus.RaiseLocalEvent(area.AreaEntityUid, paramsEv);

                var genEv = new GenerateNefiaFloorAttemptEvent(area, mapId, data, floorNumber, i);
                EntityManager.EventBus.RaiseLocalEvent(area.AreaEntityUid, genEv);

                if (genEv.Handled)
                {
                    if (_mapManager.MapIsLoaded(mapId))
                    {
                        map = _mapManager.GetMap(mapId);
                        return true;
                    }
                    else
                    {
                        Logger.ErrorS("nefia.gen.floor", $"Map for nefia floor {mapId} not generated!");
                    }
                }
            }

            map = null;
            return false;
        }

        public void ConnectStairs(IMap map, IArea area, int floorNumber, MapCoordinates previousCoords)
        {
            // Note: Delving always means "closer to the boss level" regardless of stairs up/down
            var delving = _tags.EntityWithTagInMap(map.Id, Protos.Tag.DungeonStairsDelving);
            if (delving != null)
            {
                var areaDungeon = EntityManager.GetComponent<AreaDungeonComponent>(area.AreaEntityUid);

                if (floorNumber >= areaDungeon.DeepestFloor)
                {
                    EntityManager.DeleteEntity(delving.Owner);
                }
                else
                {
                    var nextFloorId = AreaNefiaSystem.FloorNumberToAreaId(floorNumber + 1);
                    var stairs = EntityManager.GetComponent<StairsComponent>(delving.Owner);
                    stairs.Entrance = new MapEntrance()
                    {
                        MapIdSpecifier = new AreaFloorMapIdSpecifier(area.Id, nextFloorId),
                        StartLocation = new TaggedEntityMapLocation(Protos.Tag.DungeonStairsSurfacing)
                    };

                    Logger.InfoS("nefia.gen.floor", $"Delving stairs to: {nextFloorId}");
                }
            }
            else
            {
                Logger.WarningS("nefia.gen.floor", "No stairs delving in dungeon.");
            }

            var surfacing = _tags.EntityWithTagInMap(map.Id, Protos.Tag.DungeonStairsSurfacing);
            if (surfacing != null)
            {
                var stairs = EntityManager.GetComponent<StairsComponent>(surfacing.Owner);

                if (floorNumber <= 1)
                {
                    // TODO for better precision, the entity UID of the entrance needs to be passed here.
                    // but MapEntrance doesn't attach any entity information.
                    stairs.Entrance = MapEntrance.FromMapCoordinates(previousCoords);

                    Logger.InfoS("nefia.gen.floor", $"Surfacing stairs to coords: {previousCoords}");
                }
                else
                {
                    var prevFloorId = AreaNefiaSystem.FloorNumberToAreaId(floorNumber - 1);
                    stairs.Entrance = new MapEntrance()
                    {
                        MapIdSpecifier = new AreaFloorMapIdSpecifier(area.Id, prevFloorId),
                        StartLocation = new TaggedEntityMapLocation(Protos.Tag.DungeonStairsDelving)
                    };

                    Logger.InfoS("nefia.gen.floor", $"Surfacing stairs to: {prevFloorId}");
                }
            }
            else
            {
                // Weird, we should always have surfacing stairs (in standard nefia).
                Logger.WarningS("nefia.gen.floor", "No stairs surfacing in dungeon.");
            }
        }
    }
}
