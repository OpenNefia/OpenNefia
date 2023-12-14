using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Memory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.RandomGen
{
    public interface ICharaGen : IEntitySystem
    {
        PrototypeId<EntityPrototype>? PickRandomCharaIdRaw(IMap? map, int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, string? category = null);
        PrototypeId<EntityPrototype> PickRandomCharaId(IMap? map, EntityGenArgSet args, int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null);

        // TODO TryGenerateChara
        EntityUid? GenerateChara(MapCoordinates coords, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null);
        EntityUid? GenerateChara(EntityCoordinates coords, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null);
        EntityUid? GenerateChara(EntityUid ent, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null);
        EntityUid? GenerateChara(IMap map, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null);
        EntityUid? GenerateChara(IContainer container, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null);
        EntityUid? GenerateChara(MapCoordinates coords, CharaFilter filter);
        EntityUid? GenerateChara(EntityUid ent, CharaFilter filter);
        EntityUid? GenerateChara(IMap map, CharaFilter filter);

        CharaFilter GenerateCharaFilter(IMap map);
        EntityUid? GenerateCharaFromMapFilter(MapCoordinates coords);
        EntityUid? GenerateCharaFromMapFilter(IMap map);

        int GetMaxCrowdDensity(IMap map);
        PrototypeId<ChipPrototype> PickRandomHumanChipID(Gender gender);
    }

    public sealed partial class CharaGenSystem : EntitySystem, ICharaGen
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IEntityGenMemorySystem _memory = default!;
        [Dependency] private readonly IMapPlacement _placement = default!;

        public PrototypeId<EntityPrototype>? PickRandomCharaIdRaw(IMap? map, int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null, string? category = null)
        {
            var extraFilter = (EntityPrototype proto) =>
            {
                var charaComp = proto.Components.GetComponent<CharaComponent>();

                if (raceFilter != null && charaComp.Race != raceFilter.Value)
                    return false;

                // TODO use TagComponent instead...
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

            int GetWeight(EntityPrototype proto, int minLevel)
            {
                var comps = proto.Components;
                var baseRarity = _randomGen.GetBaseRarity(proto, RandomGenTables.Chara, map);
                var level = comps.GetComponent<LevelComponent>().Level;

                return baseRarity.Rarity / (500 + Math.Abs(minLevel - level) * baseRarity.Coefficient) + 1;
            }

            var id = _randomGen.PickRandomEntityId(RandomGenTables.Chara, GetWeight, extraFilter, minLevel, tags, fltselect);

            if (Logger.GetSawmill("randomgen.chara").Level <= LogLevel.Debug)
            {
                var tagString = string.Join(", ", tags ?? new PrototypeId<TagPrototype>[] { });
                Logger.DebugS("randomgen.chara", $"ID: minLevel={minLevel} tags={tagString} fltselect={fltselect} raceFilter={raceFilter} category={category} -> {id}");
            }

            return id;
        }

        public PrototypeId<EntityPrototype> PickRandomCharaId(IMap? map, EntityGenArgSet args, int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null)
        {
            var commonArgs = args.Ensure<EntityGenCommonArgs>();

            string? category = null;
            if (args.TryGet<CharaGenArgs>(out var charaArgs))
                category ??= charaArgs.Category;

            if (category == null && tags?.Length == 0 && raceFilter == null)
            {
                if (commonArgs.Quality == Quality.Good && _rand.OneIn(20))
                    commonArgs.Quality = Quality.Unique;
                if (commonArgs.Quality == Quality.Great && _rand.OneIn(10))
                    commonArgs.Quality = Quality.Unique;
            }

            var raw = PickRandomCharaIdRaw(map, minLevel, tags, fltselect, raceFilter, category);

            if (raw == null)
            {
                if (fltselect == FltSelects.Unique || commonArgs.Quality == Quality.Unique)
                {
                    commonArgs.Quality = Quality.Great;
                    fltselect = null;
                }
                minLevel += 10;
                raw = PickRandomCharaIdRaw(map, minLevel, tags, fltselect, raceFilter, category);
            }

            if (raw == null)
            {
                var tagString = tags != null ? string.Join(", ", tags.ToArray()) : "<no tags>";
                raw = Protos.Chara.Bug;
                Logger.ErrorS("randomgen.chara", $"No character generation candidates found: minLevel={minLevel} tags={tagString} fltselect={fltselect} raceFilter={raceFilter} category={category}");
            }

            return raw!.Value;
            // <<<<<<<< shade2/item.hsp:609 		} ..
        }

        public EntityUid? GenerateChara(EntityCoordinates coords, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null)
        {
            args ??= EntityGenArgSet.Make();

            if (id == null)
            {
                TryMap(coords, out var map);
                id = PickRandomCharaId(map, args, minLevel, tags, fltselect, raceFilter);
            }

            var commonArgs = args.Get<EntityGenCommonArgs>();
            commonArgs.MinLevel = minLevel;
            if (quality != null)
                commonArgs.Quality = quality.Value;

            return _entityGen.SpawnEntity(id, coords, args: args);
        }

        public EntityUid? GenerateChara(MapCoordinates coords, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null)
        {
            if (!coords.TryToEntity(_mapManager, out var entityCoords))
                return null;

            return GenerateChara(entityCoords, id, minLevel, tags, fltselect, raceFilter, quality, args);
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


       public EntityUid? GenerateChara(IContainer container, PrototypeId<EntityPrototype>? id = null,
            int minLevel = 1, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null,
            PrototypeId<RacePrototype>? raceFilter = null, Quality? quality = null, EntityGenArgSet? args = null)
        {
            args ??= EntityGenArgSet.Make();

            if (id == null)
            {
                TryMap(container.Owner, out var map); // TODO should this be configurable?
                id = PickRandomCharaId(map, args, minLevel, tags, fltselect, raceFilter);
            }

            var commonArgs = args.Get<EntityGenCommonArgs>();
            commonArgs.MinLevel = minLevel;
            if (quality != null)
                commonArgs.Quality = quality.Value;

            return _entityGen.SpawnEntity(id, container, args: args);
        }

        public EntityUid? GenerateChara(MapCoordinates coords, CharaFilter filter)
        {
            return GenerateChara(coords, filter.Id, filter.MinLevel, filter.Tags, filter.FltSelect, filter.RaceFilter, filter.Quality, filter.Args);
        }

        public EntityUid? GenerateChara(EntityUid ent, CharaFilter filter)
        {
            return GenerateChara(ent, filter.Id, filter.MinLevel, filter.Tags, filter.FltSelect, filter.RaceFilter, filter.Quality, filter.Args);
        }

        public EntityUid? GenerateChara(IMap map, CharaFilter filter)
        {
            return GenerateChara(map, filter.Id, filter.MinLevel, filter.Tags, filter.FltSelect, filter.RaceFilter, filter.Quality, filter.Args);
        }

        public CharaFilter GenerateCharaFilter(IMap map)
        {
            var ev = new GenerateCharaFilterEvent(map);
            RaiseEvent(map.MapEntityUid, ref ev);
            return ev.OutCharaFilter;
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

        public PrototypeId<ChipPrototype> PickRandomHumanChipID(Gender gender)
        {
            if (gender == Gender.Male)
                return _rand.Pick(MaleHumanChips);
            else
                return _rand.Pick(FemaleHumanChips);
        }
    }

    [ByRefEvent]
    public struct GenerateCharaFilterEvent
    {
        public GenerateCharaFilterEvent(IMap map) { Map = map; }

        public IMap Map { get; }
        public CharaFilter OutCharaFilter { get; set; } = new();
    }

    [DataDefinition]
    public sealed class CharaFilter
    {
        [DataField]
        public PrototypeId<EntityPrototype>? Id { get; set; } = null;

        [DataField]
        public int MinLevel { get; set; } = 1;

        [DataField]
        public PrototypeId<TagPrototype>[]? Tags { get; set; } = null;

        [DataField]
        public string? FltSelect { get; set; } = null;

        [DataField]
        public PrototypeId<RacePrototype>? RaceFilter = null;

        // TODO: args are not serializable yet
        public EntityGenArgSet Args { get; set; } = EntityGenArgSet.Make();

        public EntityGenCommonArgs CommonArgs => Args.Get<EntityGenCommonArgs>();

        [DataField]
        public Quality? Quality { get => CommonArgs.Quality; set => CommonArgs.Quality = value; }

        /// <summary>
        /// Level to set the generated entity to, irrespective of the entity's level in the prototype.
        /// </summary>
        [DataField]
        public int? LevelOverride { get => CommonArgs.LevelOverride; set => CommonArgs.LevelOverride = value; }
    }
}
