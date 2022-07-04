using OpenNefia.Content.DisplayName;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Skills
{
    public sealed partial class SkillsSystem
    {
        public void DamageHP(EntityUid uid, int amount, string source, bool showMessage = true, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            // TODO damage source struct, kill checking
            amount = Math.Max(amount, 0);
            skills.HP -= amount;
        }
    }

    public interface IDamageSource
    {
        /// <summary>
        /// Displayed in-game when a character is killed.
        /// </summary>
        /// <remarks>
        /// "You were squashed by a putitoro."
        /// </remarks>
        string LocalizeDeathMessage(EntityUid target, IEntityManager entityManager);

        /// <summary>
        /// Displayed in the high-score (bones) menu when prompted to revive.
        /// </summary>
        /// <remarks>
        /// "was squashed by a putitoro."
        /// </remarks>
        string LocalizeDeathCauseMessage(EntityUid target, IEntityManager entityManager);

    }

    public enum CharaDeathType
    {
        Killed,
        Minced,
        TransformedIntoMeat,
        Destroyed,
    }

    public sealed class CharaDamageSource : IDamageSource
    {
        public EntityUid AttackedBy { get; }
        public CharaDeathType Type { get; }

        public CharaDamageSource(EntityUid attackedBy, CharaDeathType type)
        {
            AttackedBy = attackedBy;
            Type = type;
        }

        public string LocalizeDeathCauseMessage(EntityUid target, IEntityManager entityManager)
        {
            return Loc.GetString("Elona.DamageSource.Chara.DeathCause", ("attacker", AttackedBy));
        }

        public string LocalizeDeathMessage(EntityUid target, IEntityManager entityManager)
        {
            return Loc.GetString($"Elona.DamageSource.Chara.{Type}.Passive", ("entity", target), ("attacker", AttackedBy));
        }
    }

    public sealed class BurdenDamageSource : IDamageSource
    {
        public BurdenDamageSource(EntityUid itemSquashedBy)
        {
            ItemSquashedBy = itemSquashedBy;
        }

        public EntityUid? ItemSquashedBy { get; }

        public string LocalizeDeathCauseMessage(EntityUid target, IEntityManager entityManager)
        {
            var itemName = GetItemName(entityManager);
            return Loc.GetString("Elona.DamageSource.Burden.DeathCause", ("entity", target), ("itemName", itemName));
        }

        public string LocalizeDeathMessage(EntityUid target, IEntityManager entityManager)
        {
            var itemName = GetItemName(entityManager);
            return Loc.GetString($"Elona.DamageSource.Burden.Message", ("entity", target), ("itemName", itemName));
        }

        private string GetItemName(IEntityManager entityManager)
        {
            if (entityManager.IsAlive(ItemSquashedBy))
                return EntitySystem.Get<IDisplayNameSystem>().GetDisplayName(ItemSquashedBy.Value);
            else
                return Loc.GetString("Elona.DamageSource.Burden.Backpack");
        }
    }

    public sealed class GenericDamageSource : IDamageSource
    {
        /// <summary>
        /// Locale key like "Elona.DamageSource.Poison"
        /// </summary>
        public LocaleKey LocaleKey { get; }

        public GenericDamageSource(LocaleKey localeKey)
        {
            LocaleKey = localeKey;
        }

        public string LocalizeDeathCauseMessage(EntityUid target, IEntityManager entityManager)
        {
            return Loc.GetString(LocaleKey.With("DeathCause"), ("entity", target));
        }

        public string LocalizeDeathMessage(EntityUid target, IEntityManager entityManager)
        {
            return Loc.GetString(LocaleKey.With("Message"), ("entity", target));
        }
    }
}
