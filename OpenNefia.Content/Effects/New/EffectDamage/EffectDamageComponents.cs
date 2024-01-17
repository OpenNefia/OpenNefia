using OpenNefia.Content.Currency;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Religion;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// Randomly selects damage based on dice.
    /// Arguments you can use in the formulas:
    /// - "power": Power of the effect
    /// - "skillLevel": Level of the associated skill.
    /// - "casterLevel": Experience level of the caster/user.
    /// - "targetLevel": Experience level of the target, or 0 if there is no target.
    /// - "distance": Fractional distance from the source entity to the target entity.
    /// - "baseDamage": Result of the dice roll (only inside <see cref="FinalDamage"/>).
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectBaseDamageDiceComponent : Component
    {
        [DataField]
        public Formula DiceX { get; set; } = new("0");

        [DataField]
        public Formula DiceY { get; set; } = new("0");

        [DataField]
        public Formula Bonus { get; set; } = new("0");

        [DataField]
        public Formula ElementPower { get; set; } = new("0");

        [DataField]
        public Formula FinalDamage { get; set; } = new("baseDamage");

        [DataField]
        public Dictionary<string, IFormulaVariable> ExtraVariables { get; } = new();
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IFormulaVariable
    {
        public double Calculate(EntityUid effect, EntityUid source, EntityUid? target);
    }

    public sealed class SkillLevelFormulaVariable : IFormulaVariable
    {
        [Dependency] private readonly ISkillsSystem _skills = default!;

        [DataField(required: true)]
        public PrototypeId<SkillPrototype> SkillID { get; set; }

        public double Calculate(EntityUid effect, EntityUid source, EntityUid? target)
        {
            return _skills.Level(source, SkillID);
        }
    }

    /// <summary>
    /// Used by Prayer of Jure.
    /// </summary>
    public sealed class PietyFormulaVariable : IFormulaVariable
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public double Calculate(EntityUid effect, EntityUid source, EntityUid? target)
        {
            return _entityManager.GetComponentOrNull<ReligionComponent>(source)?.Piety ?? 0;
        }
    }

    public enum CurrencyType
    {
        Gold,
        Platinum
    }

    public enum VariableSubject
    {
        Source,
        Target
    }

    /// <summary>
    /// Used by Suspicious Hand.
    /// </summary>
    public sealed class CurrencyFormulaVariable : IFormulaVariable
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        [DataField]
        public VariableSubject Subject { get; set; }

        [DataField]
        public CurrencyType Type { get; set; }

        public double Calculate(EntityUid effect, EntityUid source, EntityUid? target)
        {
            MoneyComponent? comp;

            switch (Subject)
            {
                case VariableSubject.Source:
                default:
                    comp = _entityManager.GetComponentOrNull<MoneyComponent>(source);
                    break;
                case VariableSubject.Target:
                    if (!_entityManager.IsAlive(target))
                        return 0;
                    comp = _entityManager.GetComponentOrNull<MoneyComponent>(target.Value);
                    break;
            }

            if (comp == null)
                return 0;

            return Type switch
            {
                CurrencyType.Gold => comp.Gold,
                CurrencyType.Platinum => comp.Platinum,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Modifies damage based on the caster's Control Magic level.
    /// Can cancel the effect entirely if the level is high enough.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageControlMagicComponent : Component
    {
    }

    /// <summary>
    /// Reduces the effect's power if it was cast via rapid magic.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageCastByRapidMagicComponent : Component
    {
        [DataField]
        public int TotalAttackCount { get; set; }

        [DataField]
        public int CurrentAttackCount { get; set; }
    }

    public enum CastInsteadCriteria
    {
        Any,
        Player,
        PlayerOrAlly,
        NotPlayer,
        Other,
        Mount
    }

    /// <summary>
    /// Casts a different effect or stops the effect if the criteria is met.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageCastInsteadComponent : Component
    {
        [DataField]
        public CastInsteadCriteria IfSource { get; set; } = CastInsteadCriteria.Any;

        [DataField]
        public CastInsteadCriteria IfTarget { get; set; } = CastInsteadCriteria.Any;

        /// <summary>
        /// If null, then "nothing happens..."
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype>? EffectID { get; set; }
    }

    public enum EffectRetargetRule
    {
        AlwaysRider,
        AlwaysMount
    }

    /// <summary>
    /// Changes the target if it matches the criteria.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageRetargetComponent : Component
    {
        [DataField]
        public List<EffectRetargetRule> Rules { get; set; } = new();
    }

    /// <summary>
    /// Randomly fails the spell.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageSuccessRateComponent : Component
    {
        [DataField]
        [Obsolete("TODO move to EffectAreaMessageComponent")]
        public LocaleKey? MessageKey { get; set; }

        [DataField]
        public Formula SuccessRate { get; set; } = new("1");
    }

    /// <summary>
    /// Ignores targets in the AoE that have the specified relations.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageRelationsComponent : Component
    {
        /// <summary>
        /// Relation range to use.
        /// </summary>
        [DataField]
        public EnumRange<Relation> ValidRelations { get; }
    }

    /// <summary>
    /// Controls how the damage message is displayed when combined with
    /// <see cref="IDamageSystem.DamageHP"/>.
    /// Generally speaking, if you use an EffectDamage component that
    /// damages HP, you should also include this one so that the
    /// damage message is formatted correctly.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageMessageComponent : Component
    {
        /// <summary>
        /// Root key to use. It should have "Ally" and "Other"
        /// messages for attacks that hit allies and non-allies.
        /// </summary>
        [DataField]
        public LocaleKey RootKey { get; set; } = "Elona.Magic.Message.Generic";

        [DataField]
        public Color Color { get; set; } = UiColors.MesWhite;
    }

    /// <summary>
    /// Causes elemental damage to targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageElementalComponent : Component
    {
        [DataField]
        public PrototypeId<ElementPrototype>? Element { get; set; } = null;

        // TODO remove?
        // may need to be replaced with a BeforeApplyEffectDamage event to potentially cancel
        // effects. then all effect damage components can be combined easily.
        // then ApplyEffectDamageEvent will be made non-cancellable.
        [DataField]
        public bool HandleEvent { get; set; } = true;
    }

    /// <summary>
    /// Causes healing "damage" to targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageHealingComponent : Component
    {
        /// <summary>
        /// Root key to use.
        /// </summary>
        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Effect.Heal.Normal";

        // TODO remove?
        [DataField]
        public bool HandleEvent { get; set; } = true;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageMPComponent : Component
    {
        /// <summary>
        /// Root key to use.
        /// </summary>
        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Effect.DamageMP.Normal";

        // TODO remove?
        [DataField]
        public bool HandleEvent { get; set; } = true;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageMObjComponent : Component
    {
        /// <summary>
        /// Root key to use.
        /// </summary>
        [DataField("mObjID")]
        public PrototypeId<EntityPrototype> MObjID { get; set; }

        // TODO remove?
        [DataField]
        public bool HandleEvent { get; set; } = true;
    }

    /// <summary>
    /// Causes healing "damage" to targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageHealMPComponent : Component
    {
        /// <summary>
        /// Root key to use.
        /// </summary>
        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Effect.HealMP.Normal";

        // TODO remove?
        [DataField]
        public bool HandleEvent { get; set; } = true;
    }

    /// <summary>
    /// Causes sanity healing "damage" to targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageHealSanityComponent : Component
    {
        /// <summary>
        /// Root key to use.
        /// </summary>
        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Effect.HealSanity.RainOfSanity";

        // TODO remove?
        [DataField]
        public bool HandleEvent { get; set; } = true;
    }

    /// <summary>
    /// Spawns map effects at the tile positions.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageMefComponent : Component
    {
        [DataField]
        public PrototypeId<EntityPrototype> MefID { get; set; } = Protos.Mef.Web;

        /// <summary>
        /// Number of turns the mef should last for.
        /// If <c>null</c>, then the mef will last forever.
        /// </summary>
        [DataField]
        public Formula? Turns { get; set; }

        // TODO remove?
        [DataField]
        public bool HandleEvent { get; set; } = true;
    }

    /// <summary>
    /// Damages tiles on the ground with the specified element.
    /// </summary>
    /// <remarks>
    /// Similar to <see cref="EffectDamageElementalComponent"/> but
    /// without entity damage.
    /// </remarks>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTileDamageElementalComponent : Component
    {
        [DataField]
        public PrototypeId<ElementPrototype> Element { get; set; }
    }

    [DataDefinition]
    public sealed class EffectStatusEffect
    {
        [DataField(required: true)]
        public PrototypeId<StatusEffectPrototype> ID { get; set; }

        [DataField]
        public Formula Power { get; set; } = new("power");
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageStatusEffectsComponent : Component
    {
        [DataField]
        public List<EffectStatusEffect> StatusEffects { get; } = new();

        // TODO remove? replace with event cancellation system?
        [DataField]
        public bool HandleEvent { get; set; } = true;
    }

    /// <summary>
    /// Inflicts hand damage with the given damage type.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageTouchComponent : Component
    {
        [DataField(required: true)]
        public PrototypeId<ElementPrototype> ElementID { get; set; }

        [DataField]
        public bool ApplyDamage { get; set; } = true;

        // TODO remove? replace with event cancellation system?
        [DataField]
        public bool HandleEvent { get; set; } = true;

        [DataField]
        public LocaleKey? MessageKey { get; set; } = new("Elona.Magic.Message.Touch");
    }
}