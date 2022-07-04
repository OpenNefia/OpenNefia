using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Damage
{
    [ImplicitDataDefinitionForInheritors]
    public interface IDamageType
    {
        /// <summary>
        /// Displayed in-game when a character is killed.
        /// </summary>
        /// <remarks>
        /// "You were squashed by a putitoro."
        /// </remarks>
        string LocalizeDeathMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager);

        /// <summary>
        /// Displayed in the high-score (bones) menu when prompted to revive.
        /// </summary>
        /// <remarks>
        /// "was squashed by a putitoro."
        /// </remarks>
        string LocalizeDeathCauseMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager);
    }

    public enum CharaDeathType
    {
        Killed,
        Minced,
        TransformedIntoMeat,
        Destroyed,
    }

    public sealed class DefaultDamageType : IDamageType
    {
        public string LocalizeDeathCauseMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            // TODO?
            return "???";
        }

        public string LocalizeDeathMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            return Loc.GetString("Elona.DamageType.Default.Death.Passive", ("entity", target));

        }
    }

    public sealed class CharaDamageType : IDamageType
    {
        [DataField]
        public CharaDeathType CharaDeathType { get; }

        [DataField]
        public int AttackCount { get; }

        public CharaDamageType() {}

        public CharaDamageType(CharaDeathType type)
        {
            CharaDeathType = type;
        }

        public string LocalizeDeathCauseMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            return Loc.GetString("Elona.DamageType.Chara.DeathCause", ("attacker", attacker));
        }

        public string LocalizeDeathMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            return Loc.GetString($"Elona.DamageType.Chara.{CharaDeathType}.Passive", ("entity", target), ("attacker", attacker));
        }
    }

    public sealed class ElementalDamageType : IDamageType
    {
        [DataField]
        public PrototypeId<ElementPrototype> ElementID { get; }

        [DataField]
        public int Power { get; }

        public ElementalDamageType() {}

        public ElementalDamageType(PrototypeId<ElementPrototype> elementID, int power)
        {
            ElementID = elementID;
            Power = power;
        }

        public string LocalizeDeathCauseMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            return Loc.GetPrototypeString(ElementID, "Death.Passive", ("entity", target));
        }

        public string LocalizeDeathMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            return Loc.GetPrototypeString(ElementID, "Death.Active", ("entity", target), ("attacker", attacker));
        }
    }

    public sealed class BurdenDamageType : IDamageType
    {
        [DataField]
        public EntityUid? ItemSquashedBy { get; }

        public BurdenDamageType() {}

        public BurdenDamageType(EntityUid itemSquashedBy)
        {
            ItemSquashedBy = itemSquashedBy;
        }

        public string LocalizeDeathCauseMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            var itemName = GetItemName(entityManager);
            return Loc.GetString("Elona.DamageType.Burden.DeathCause", ("entity", target), ("itemName", itemName));
        }

        public string LocalizeDeathMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            var itemName = GetItemName(entityManager);
            return Loc.GetString($"Elona.DamageType.Burden.Message", ("entity", target), ("itemName", itemName));
        }

        private string GetItemName(IEntityManager entityManager)
        {
            if (entityManager.IsAlive(ItemSquashedBy))
                return EntitySystem.Get<IDisplayNameSystem>().GetDisplayName(ItemSquashedBy.Value);
            else
                return Loc.GetString("Elona.DamageType.Burden.Backpack");
        }
    }

    public sealed class GenericDamageType : IDamageType
    {
        /// <summary>
        /// Locale key like "Elona.DamageType.Poison"
        /// </summary>
        [DataField]
        public LocaleKey LocaleKey { get; }

        public GenericDamageType() {}

        public GenericDamageType(LocaleKey localeKey)
        {
            LocaleKey = localeKey;
        }

        public string LocalizeDeathCauseMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            return Loc.GetString(LocaleKey.With("DeathCause"), ("entity", target));
        }

        public string LocalizeDeathMessage(EntityUid target, EntityUid? attacker, IEntityManager entityManager)
        {
            return Loc.GetString(LocaleKey.With("Message"), ("entity", target), ("attacker", attacker));
        }
    }
}
