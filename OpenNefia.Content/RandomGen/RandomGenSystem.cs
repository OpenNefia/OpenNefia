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

        int GetRarity(EntityUid uid, string randomTableName, RandomGenComponent? randomGen = null);
    }

    // TODO no hardcoding
    /// <summary>
    /// Entities with these tags will not be generated unless the
    /// </summary>

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

                if (!comps.TryGetComponent<LevelComponent>(out var level) || level.Level > minLevel)
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

        public int GetRarity(EntityUid uid, string randomTableName, RandomGenComponent? randomGen = null)
        {
            if (!Resolve(uid, ref randomGen))
                return RandomGenTable.DefaultRarity;

            if (!randomGen.Tables.TryGetValue(randomTableName, out var randomGenTable))
                return RandomGenTable.DefaultRarity;

            return randomGenTable.Rarity;
        }
    }
}
