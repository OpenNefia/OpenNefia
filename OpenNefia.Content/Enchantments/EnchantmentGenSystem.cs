using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Game;
using OpenNefia.Content.Feats;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.RandomGen;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Identify;
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.Enchantments
{
    public interface IEnchantmentGenSystem : IEntitySystem
    {
        PrototypeId<EntityPrototype>? PickRandomEnchantmentID(EntityUid item, int level = 0);

        /// <hsp>randomEncLv(int level)</hsp>
        int CalcRandomEnchantmentLevel(int level = EnchantmentGenSystem.MaxRandomEnchantmentLevel);

        /// <hsp>randomEncP(int level)</hsp>
        int CalcRandomEnchantmentPower(int level = 0);

        EntityUid? GenerateEnchantment(EntityUid item, int egoLevel, int? power = null, int cursePower = 0, string source = EnchantmentSources.Generated);
        EntityUid? GenerateFixedLevelEnchantment(EntityUid item, int level, int? power = null, int cursePower = 0, string source = EnchantmentSources.Generated);

        bool TryGenerateEnchantment(EntityUid item, int egoLevel, [NotNullWhen(true)] out EntityUid? enchantment, int? power = null, int cursePower = 0, string source = EnchantmentSources.Generated);
        bool TryGenerateFixedLevelEnchantment(EntityUid item, int level, [NotNullWhen(true)] out EntityUid? enchantment, int? power = null, int cursePower = 0, string source = EnchantmentSources.Generated);
        int CalcRandomEnchantmentEgoLevel(EntityUid item, int objectLevel);
        void AddEgoMinorEnchantments(EntityUid item, int egoLevel);
        void AddEgoMajorEnchantments(EntityUid item, int egoLevel);
    }

    public sealed partial class EnchantmentGenSystem : EntitySystem, IEnchantmentGenSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IEnchantmentSystem _enchantments = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;

        public PrototypeId<EntityPrototype>? PickRandomEnchantmentID(EntityUid item, int level = 0)
        {
            // >>>>>>>> shade2/item_data.hsp:463 	#module ...
            bool Filter(EntityPrototype proto)
            {
                if (!proto.Components.TryGetComponent<EnchantmentComponent>(out var enc))
                    return false;

                if (enc.Level > level)
                    return false;

                if (level >= 0 && enc.Level < 0)
                {
                    // This is a negative enchantment, but we asked for a positive enchantment.
                    return false;
                }

                if (enc.ValidItemCategories != null)
                {
                    var hasValidCategory = false;
                    foreach (var tag in _tags.GetTags(item))
                    {
                        if (enc.ValidItemCategories.Contains(tag))
                        {
                            hasValidCategory = true;
                            break;
                        }
                    }
                    if (!hasValidCategory)
                        return false;
                }

                return true;
            }

            var sampler = new WeightedSampler<EntityPrototype>();
            foreach (var candidate in _protos.EnumeratePrototypes<EntityPrototype>().Where(Filter))
            {
                var enc = candidate.Components.GetComponent<EnchantmentComponent>();
                sampler.Add(candidate, enc.RandomWeight);
            }

            // This may return null, in which case no enchantment is generated..
            return sampler.Sample()?.GetStrongID();
            // <<<<<<<< shade2/item_data.hsp:481 	return i ..
        }

        public int CalcRandomEnchantmentEgoLevel(EntityUid item, int objectLevel)
        {
            if (_qualities.GetQuality(item) == Quality.Unique)
                return MaxRandomEnchantmentLevel;

            return _rand.Next(Math.Clamp(_rand.Next(objectLevel / 10 + 3), 0, MaxRandomEnchantmentLevel));
        }

        public const int MaxRandomEnchantmentLevel = 4;

        public int CalcRandomEnchantmentLevel(int level = MaxRandomEnchantmentLevel)
        {
            // >>>>>>>> shade2/item_data.hsp:483 	#defcfunc randomEncLv int refLv ...
            level = Math.Clamp(level, 0, MaxRandomEnchantmentLevel);
            return _rand.Next(level + 1);
            // <<<<<<<< shade2/item_data.hsp:486 	return encLv ..
        }

        public int CalcRandomEnchantmentPower(EntityUid item)
        {
            return CalcRandomEnchantmentPower(_levels.GetLevel(item));
        }

        public int CalcRandomEnchantmentPower(int level = 0)
        {
            // >>>>>>>> shade2/item_data.hsp:489 	#defcfunc randomEncP int refLv ...
            var hasGodLuck = false;
            if (IsAlive(_gameSession.Player) && _feats.HasFeat(_gameSession.Player, Protos.Feat.GodLuck))
                hasGodLuck = true;

            var power = _rand.Next(_rand.Next(500 + (hasGodLuck ? 50 : 0)) + 1) + 1;
            if (level > 0)
                power = power * level / 100;

            return power;
            // <<<<<<<< shade2/item_data.hsp:492 	return encP ..
        }

        public EntityUid? GenerateEnchantment(EntityUid item, int egoLevel, int? power = null, int cursePower = 0, string source = EnchantmentSources.Generated)
        {
            TryGenerateEnchantment(item, egoLevel, out var enchantment, power, cursePower, source);
            return enchantment;
        }

        public EntityUid? GenerateFixedLevelEnchantment(EntityUid item, int level, int? power = null, int cursePower = 0, string source = EnchantmentSources.Generated)
        {
            TryGenerateFixedLevelEnchantment(item, level, out var enchantment, power, cursePower, source);
            return enchantment;
        }

        public bool TryGenerateEnchantment(EntityUid item, int egoLevel, [NotNullWhen(true)] out EntityUid? enchantment, int? power = null, int cursePower = 0, string source = EnchantmentSources.Generated)
        {
            var level = CalcRandomEnchantmentLevel(egoLevel);
            return TryGenerateFixedLevelEnchantment(item, level, out enchantment, power, cursePower, source);
        }

        public bool TryGenerateFixedLevelEnchantment(EntityUid item, int level, [NotNullWhen(true)] out EntityUid? enchantment, int? power = null, int cursePower = 0, string source = EnchantmentSources.Generated)
        {
            var id = PickRandomEnchantmentID(item, level);
            if (id == null)
            {
                enchantment = null;
                return false;
            }

            power ??= CalcRandomEnchantmentPower();

            return _enchantments.TryAddEnchantment(item, id.Value, power.Value, out enchantment, cursePower, source: source);
        }

        public const int MaxEgoMinorEnchantments = 5;

        public void AddEgoMajorEnchantments(EntityUid item, int egoLevel)
        {
            // >>>>>>>> shade2/item_data.hsp:732 *item_ego ...
            if (!TryComp<EnchantmentsComponent>(item, out var encs))
                return;

            var candidates = _protos.EnumeratePrototypes<EgoMajorEnchantmentPrototype>()
                .Where(p =>
                {
                    if (p.Filter == null)
                        return true;

                    // TODO automatic injection
                    EntitySystem.InjectDependencies(p.Filter);
                    return p.Filter.CanApplyTo(item);
                })
                .ToList();

            var egoMajor = _rand.PickOrDefault(candidates);

            if (egoMajor == null)
                return;

            encs.EgoMajorEnchantment = egoMajor.GetStrongID();

            foreach (var encSpecifier in egoMajor.Enchantments)
            {
                var newPower = CalcRandomEnchantmentPower(encSpecifier.Power);
                var newSpec = new EnchantmentSpecifer(encSpecifier.ProtoID, newPower, cursePower: 8, encSpecifier.Randomize, encSpecifier.Components);
                _enchantments.AddEnchantmentFromSpecifier(item, newSpec, EnchantmentSources.EgoMajor, encs);
            }

            if (_rand.OneIn(2))
                GenerateEnchantment(item, egoLevel, 0, 20, EnchantmentSources.EgoMajor);
            if (_rand.OneIn(4))
                GenerateEnchantment(item, egoLevel, 0, 25, EnchantmentSources.EgoMajor);
            // <<<<<<<< shade2/item_data.hsp:751 	return ..
        }

        public void AddEgoMinorEnchantments(EntityUid item, int egoLevel)
        {
            // >>>>>>>> shade2/item_data.hsp:723 *item_egoMinor ...
            var count = _rand.Next(_rand.Next(MaxEgoMinorEnchantments) + 1) + 1;

            for (var i = 0; i < count; i++)
            {
                GenerateEnchantment(item, egoLevel, cursePower: 8, source: EnchantmentSources.EgoMinor);
            }

            if (TryComp<EnchantmentsComponent>(item, out var encs))
            {
                encs.EgoMinorEnchantment = _rand.Pick(_protos.EnumeratePrototypes<EgoMinorEnchantmentPrototype>().ToList()).GetStrongID();
            }
            // <<<<<<<< shade2/item_data.hsp:729 	return ..
        }

        public void AddRandomEnchantments(EntityUid item, int? objectLevel = null, Quality? objectQuality = null, EnchantmentsComponent? encs = null)
        {
            if (!Resolve(item, ref encs))
                return;

            objectLevel ??= _levels.GetLevel(item);
            objectQuality ??= _qualities.GetQuality(item);

            if (objectQuality <= Quality.Normal)
                return;

            if (TryComp<AmmoComponent>(item, out var ammo))
                ammo.ActiveAmmoEnchantment = null;

            var egoLevel = CalcRandomEnchantmentEgoLevel(item, objectLevel.Value);

            if (objectQuality < Quality.Unique)
            {
                encs.HasRandomEnchantments = true;

                if (TryComp<IdentifyComponent>(item, out var identify))
                {
                    identify.IdentifyDifficulty = 50 + _rand.Next(Math.Abs((int)objectQuality - (int)Quality.Normal) * 100 + 100);
                }
            }

            var ev = new OnAddRandomEnchantmentsEventArgs(egoLevel, objectLevel.Value, objectQuality.Value);
            RaiseEvent(item, ev);
        }
    }

    public sealed class OnAddRandomEnchantmentsEventArgs : EntityEventArgs
    {
        public int EgoLevel { get; }
        public int ObjectLevel { get; }
        public Quality ObjectQuality { get; }

        public OnAddRandomEnchantmentsEventArgs(int egoLevel, int objectLevel, Quality objectQuality)
        {
            EgoLevel = egoLevel;
            ObjectLevel = objectLevel;
            ObjectQuality = objectQuality;
        }
    }
}