using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.RandomGen
{
    public interface IRandomGenSystem : IEntitySystem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="minLevel"></param>
        /// <param name="fltselect">Entity group. </param>
        /// <param name="tags"></param>
        /// <returns></returns>
        PrototypeId<EntityPrototype>? PickRandomEntityId(string tableName, Func<EntityPrototype, int, int> weightFunc, Func<EntityPrototype, bool>? extraFilter = null, int minLevel = 0, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null);

        int CalcObjectLevel(IMap map);
        int CalcObjectLevel(EntityUid uid);
        int CalcObjectLevel(int level);
        Quality CalcObjectQuality(Quality baseQuality = Quality.Bad);

        /// <summary>
        /// Gets the rarity of this entity according to one of its random generation tables.
        /// </summary>
        AdjustedRarity GetRarity(EntityUid uid, string randomTableName, IMap? map, RandomGenComponent? randomGen = null);

        /// <summary>
        /// Gets the rarity of this entity prototype according to one of its random generation tables.
        /// </summary>
        AdjustedRarity GetBaseRarity(EntityPrototype proto, string randomTableName, IMap? map);

        /// <summary>
        /// Gets the rarity of this entity prototype according to one of its random generation tables.
        /// </summary>
        AdjustedRarity GetBaseRarity(PrototypeId<EntityPrototype> protoId, string randomTableName, IMap? map);

        PrototypeId<TagPrototype> PickTag(PrototypeId<TagSetPrototype> tagSetID);
    }

    // TODO no hardcoding

    public record struct AdjustedRarity(int Rarity, int Coefficient)
    {
        public static readonly AdjustedRarity Default = new(RandomGenTable.DefaultRarity, 100);
    }

    public sealed class RandomGenSystem : EntitySystem, IRandomGenSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;

        public PrototypeId<EntityPrototype>? PickRandomEntityId(string tableName, Func<EntityPrototype, int, int> weightFunc, Func<EntityPrototype, bool>? extraFilter = null, int minLevel = 0, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null)
        {
            // TODO obvious optimization

            var found = new HashSet<PrototypeId<TagPrototype>>();
            var tagSet = tags?.ToHashSet();

            var filter = (EntityPrototype proto) =>
            {
                var comps = proto.Components;

                if (!comps.TryGetComponent<RandomGenComponent>(out var randomGenComp))
                    return false;

                if (!randomGenComp.Tables.TryGetValue(tableName, out var randomGenTable))
                    return false;

                if (randomGenTable.Rarity == 0)
                    return false;

                if (comps.TryGetComponent<LevelComponent>(out var level) && level.Level > minLevel)
                    return false;

                comps.TryGetComponent<TagComponent>(out var tagComp);

                if (tagSet != null)
                {
                    if (tagComp == null)
                        return false;

                    if (!tagSet.IsSubsetOf(tagComp.Tags))
                        return false;
                }

                if (tagComp != null && tagComp.Tags.Contains(Protos.Tag.NoGenerate))
                    return false;

                if (fltselect != null)
                {
                    // If we asked for an entity group, be sure this entity is a part of that group.
                    if (fltselect != randomGenTable.FltSelect)
                        return false;
                }
                else if (randomGenTable.FltSelect != null)
                {
                    // If there is an entity group on this prototype, but we did not ask for one,
                    // then this entity should be excluded. For example, snow trees have fltselect "Snow",
                    // but we don't want snow trees randomly generating anywhere there isn't snow
                    // (e.g. places we don't pass a fltselect)
                    return false;
                }

                if (extraFilter != null)
                {
                    if (!extraFilter(proto))
                        return false;
                }

                return true;
            };

            var candidates = _protos.EnumeratePrototypes<EntityPrototype>().Where(filter);
            var sampler = new WeightedSampler<EntityPrototype>();

            foreach (var cand in candidates)
            {
                var weight = weightFunc(cand, minLevel);
                if (weight <= 0)
                    continue;
                sampler.Add(cand, weight);
            }

            return sampler.Sample()?.GetStrongID();
        }

        public int CalcObjectLevel(IMap map)
        {
            var level = 1;
            if (EntityManager.TryGetComponent<LevelComponent>(map.MapEntityUid, out var levelComp))
                level = levelComp.Level;
            return CalcObjectLevel(level);
        }

        public int CalcObjectLevel(EntityUid uid)
        {
            return CalcObjectLevel(_levels.GetLevel(uid));
        }

        public int CalcObjectLevel(int level)
        {
            level = Math.Max(level, 1);

            for (var i = 1; i <= 3; i++)
            {
                if (_random.OneIn(30 + i * 5))
                {
                    level += _random.Next(10 + i);
                }
                else
                {
                    break;
                }
            }

            if (level <= 3 && !_random.OneIn(4))
                level = _random.Next(3) + 1;

            return level;
        }

        public Quality CalcObjectQuality(Quality quality = Quality.Bad)
        {
            for (var i = 1; i <= 3; i++)
            {
                var n = _random.Next(30 + i * 5);
                if (n == 0)
                    quality++;
                else if (n < 3)
                    quality--;
            }

            return EnumHelpers.Clamp(quality, Quality.Bad, Quality.God);
        }

        public AdjustedRarity GetRarity(EntityUid uid, string randomTableName, IMap? map, RandomGenComponent? randomGen = null)
        {
            if (!Resolve(uid, ref randomGen))
                return AdjustedRarity.Default;

            if (!randomGen.Tables.TryGetValue(randomTableName, out var randomGenTable))
                return AdjustedRarity.Default;

            var proto = MetaData(uid).EntityPrototype;
            if (proto != null)
                return AdjustRarityFromMap(randomGenTable, randomTableName, proto, map);
            else
                return new(randomGenTable.Rarity, randomGenTable.Coefficient);
        }

        public AdjustedRarity GetBaseRarity(PrototypeId<EntityPrototype> protoId, string randomTableName, IMap? map)
        {
            if (!_protos.TryIndex(protoId, out var proto))
                return AdjustedRarity.Default;

            return GetBaseRarity(proto, randomTableName, map);
        }

        public AdjustedRarity GetBaseRarity(EntityPrototype proto, string randomTableName, IMap? map)
        {
            if (!proto.Components.TryGetComponent<RandomGenComponent>(out var randomGen))
                return AdjustedRarity.Default;

            if (!randomGen.Tables.TryGetValue(randomTableName, out var randomGenTable))
                return AdjustedRarity.Default;

            return AdjustRarityFromMap(randomGenTable, randomTableName, proto, map);
        }

        private AdjustedRarity AdjustRarityFromMap(RandomGenTable curTable, string randomTableName, EntityPrototype proto, IMap? map)
        {
            // Probably one of the few instances in which associating data with EntityPrototype IDs makes sense.
            // These are prototypes, not entities, so can't iterate RandomGenComponents in the world.
            if (map != null && TryArea(map, out var area) && TryComp<AreaRandomGenTablesComponent>(area.AreaEntityUid, out var areaRandomGen))
            {
                if (areaRandomGen.Tables.TryGetValue(randomTableName, out var tables)
                    && tables.TryGetValue(proto.GetStrongID(), out var table))
                {
                    Logger.DebugS("randomGen", $"Area overrides generation chance for {proto.ID}: {curTable.Rarity} -> {table.Rarity}");
                    return new(table.Rarity, table.Coefficient);
                }
            }

            return new(curTable.Rarity, curTable.Coefficient);
        }

        public PrototypeId<TagPrototype> PickTag(PrototypeId<TagSetPrototype> tagSetID)
        {
            var tagSet = _protos.Index(tagSetID);

            // TODO extended prototype validation
            if (tagSet.Tags.Count == 0)
                throw new InvalidDataException($"Tag set {tagSetID} containss no tags.");

            var sampler = new WeightedSampler<PrototypeId<TagPrototype>>();
            foreach (var entry in tagSet.Tags)
                sampler.Add(entry.Tag, Math.Max(entry.Weight, 1));

            return sampler.Sample();
        }
    }
}
