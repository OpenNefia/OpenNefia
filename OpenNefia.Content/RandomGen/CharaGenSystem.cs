using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Content.Skills;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Memory;
using OpenNefia.Content.Maps;
using OpenNefia.Analyzers;

namespace OpenNefia.Content.RandomGen
{
    public interface ICharaGen : IEntitySystem
    {
        PrototypeId<EntityPrototype>? PickRandomCharaIdRaw(int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, 
            PrototypeId<RacePrototype>? raceFilter = null, string? category = null);
        PrototypeId<EntityPrototype> PickRandomCharaId(EntityGenArgSet args, int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, string? category = null);

        EntityUid? GenerateChara(MapCoordinates coords, PrototypeId<EntityPrototype>? id = null, 
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, 
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null);
        EntityUid? GenerateChara(EntityUid ent, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null);
        EntityUid? GenerateChara(IMap map, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null);
        EntityUid? GenerateChara(MapCoordinates coords, CharaFilter filter);
        EntityUid? GenerateChara(EntityUid ent, CharaFilter filter);
        EntityUid? GenerateChara(IMap map, CharaFilter filter);

        CharaFilter GenerateCharaFilter(IMap map);
        EntityUid? GenerateCharaFromMapFilter(MapCoordinates coords);
        EntityUid? GenerateCharaFromMapFilter(IMap map);

        int GetMaxCrowdDensity(IMap map);
    }

    public sealed class CharaGenSystem : EntitySystem, ICharaGen
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IEntityGenMemorySystem _memory = default!;
        [Dependency] private readonly IMapPlacement _placement = default!;

        public PrototypeId<EntityPrototype>? PickRandomCharaIdRaw(int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null, string? category = null)
        {
            var extraFilter = (EntityPrototype proto) =>
            {
                var charaComp = proto.Components.GetComponent<CharaComponent>();

                if (raceFilter != null && charaComp.Race != raceFilter.Value)
                    return false;

                if (category != null)
                {
                    if (!proto.Components.TryGetComponent<CreaturePackComponent>(out var creaturePack) || creaturePack.Category != category)
                        return false;
                }

                // Don't generate unique companions twice (Isca, mad scientist...)
                if (proto.Components.HasComponent<UniqueCompanionComponent>() && _memory.Generated(proto.GetStrongID()) > 0)
                    return false;

                return true;
            };

            return _randomGen.PickRandomEntityId(RandomGenTables.Chara, GetWeight, extraFilter, minLevel, tags, fltselect);
        }

        private int GetWeight(EntityPrototype proto, int minLevel)
        {
            var comps = proto.Components;
            var level = comps.GetComponent<LevelComponent>().Level;
            var table = comps.GetComponent<RandomGenComponent>().Tables[RandomGenTables.Chara];

            return table.Rarity / (500 + Math.Abs(minLevel - level) * table.Coefficient) + 1;
        }

        public PrototypeId<EntityPrototype> PickRandomCharaId(EntityGenArgSet args, int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null, string? category = null)
        {
            var commonArgs = args.Ensure<EntityGenCommonArgs>();

            if (category == null && tags?.Length == 0 && raceFilter == null)
            {
                if (commonArgs.Quality == Quality.Good && _rand.OneIn(20))
                    commonArgs.Quality = Quality.Unique;
                if (commonArgs.Quality == Quality.Great && _rand.OneIn(10))
                    commonArgs.Quality = Quality.Unique;
            }

            var raw = PickRandomCharaIdRaw(minLevel, tags, fltselect, raceFilter, category);

            if (raw == null)
            {
                if (fltselect == FltSelects.Unique || commonArgs.Quality == Quality.Unique)
                {
                    commonArgs.Quality = Quality.Great;
                    fltselect = null;
                }
                minLevel += 10;
                raw = PickRandomCharaIdRaw(minLevel, tags, fltselect, raceFilter, category);
            }

            if (raw == null)
            {
                var tagString = tags != null ? string.Join(", ", tags.ToArray()) : "<no tags>";
                raw = Protos.Chara.Bug;
                Logger.WarningS("randomgen.item", $"No character generation candidates found: {minLevel} {tagString} {fltselect}");
            }

            return raw!.Value;
            // <<<<<<<< shade2/item.hsp:609 		} ..
        }

        public EntityUid? GenerateChara(MapCoordinates coords, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null)
        {
            args ??= EntityGenArgSet.Make();

            if (id == null)
                id = PickRandomCharaId(args, minLevel, tags, fltselect, raceFilter);

            var commonArgs = args.Get<EntityGenCommonArgs>();
            if (quality != null)
                commonArgs.Quality = quality.Value;

            return _entityGen.SpawnEntity(id, coords, args: args);
        }

        public EntityUid? GenerateChara(EntityUid ent, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null)
        {
            if (!EntityManager.TryGetComponent<SpatialComponent>(ent, out var spatial))
                return null;

            return GenerateChara(spatial.MapPosition, id, minLevel, tags, fltselect, raceFilter, quality, args);
        }

        public EntityUid? GenerateChara(IMap map, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null)
        {
            var pos = _placement.FindFreePosition(map);
            if (pos == null)
                return null;

            return GenerateChara(pos.Value, id, minLevel, tags, fltselect, raceFilter, quality, args);
        }

        public EntityUid? GenerateChara(MapCoordinates coords, CharaFilter filter)
        {
            return GenerateChara(coords, filter.Id, filter.MinLevel, filter.Tags, filter.Fltselect, filter.RaceFilter, filter.Quality, filter.Args);
        }

        public EntityUid? GenerateChara(EntityUid ent, CharaFilter filter)
        {
            return GenerateChara(ent, filter.Id, filter.MinLevel, filter.Tags, filter.Fltselect, filter.RaceFilter, filter.Quality, filter.Args);
        }

        public EntityUid? GenerateChara(IMap map, CharaFilter filter)
        {
            return GenerateChara(map, filter.Id, filter.MinLevel, filter.Tags, filter.Fltselect, filter.RaceFilter, filter.Quality, filter.Args);
        }

        public CharaFilter GenerateCharaFilter(IMap map)
        {
            var ev = new GetCharaFilterEvent(map);
            RaiseLocalEvent(map.MapEntityUid, ref ev);
            return ev.CharaFilter;
        }

        public EntityUid? GenerateCharaFromMapFilter(MapCoordinates coords)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                return null;

            return GenerateChara(coords, GenerateCharaFilter(map));
        }

        public EntityUid? GenerateCharaFromMapFilter(IMap map)
        {
            return GenerateChara(map, GenerateCharaFilter(map));
        }

        public int GetMaxCrowdDensity(IMap map)
        {
            if (!EntityManager.TryGetComponent<MapCharaGenComponent>(map.MapEntityUid, out var mapCharaGen))
                return 0;

            return mapCharaGen.MaxCharaCount;
        }
    }

    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct GetCharaFilterEvent
    {
        public GetCharaFilterEvent(IMap map) { Map = map; }

        public IMap Map { get; }
        public CharaFilter CharaFilter { get; set; } = new();
    }

    public struct CharaFilter
    {
        public CharaFilter()
        {
            Args = EntityGenArgSet.Make();
        }

        public PrototypeId<EntityPrototype>? Id { get; set; } = null;
        public int MinLevel { get; set; } = 1;
        public PrototypeId<TagPrototype>[]? Tags { get; set; } = null;
        public string? Fltselect { get; set; } = null;
        public PrototypeId<RacePrototype>? RaceFilter = null;
        public EntityGenArgSet Args { get; set; }

        public EntityGenCommonArgs CommonArgs => Args.Get<EntityGenCommonArgs>();

        public Quality? Quality { get => CommonArgs.Quality; set => CommonArgs.Quality = value; }
        public int? LevelOverride { get => CommonArgs.LevelOverride; set => CommonArgs.LevelOverride = value; }
    }
}
