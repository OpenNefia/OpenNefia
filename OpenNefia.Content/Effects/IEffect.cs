using OpenNefia.Content.CurseStates;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Effects
{
    [ImplicitDataDefinitionForInheritors]
    public interface IEffect
    {
        public TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args);
    }
    
    public abstract class Effect : IEffect
    {
        [Dependency] protected readonly IEntityManager EntityManager = default!;
        
        public abstract TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args);
    }

    public sealed class NullEffect : Effect
    {
        public override TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            return TurnResult.Succeeded;
        }
    }
    
    [ImplicitDataDefinitionForInheritors]
    public abstract class EffectArgs
    {
    }

    [DataDefinition]
    public sealed class EffectArgSet : Blackboard<EffectArgs>
    {
        public int Power { get; set; } = 1;
        public CurseState CurseState { get; set; } = CurseState.Normal;

        public static EffectArgSet Make(params EffectArgs[] rest)
        {
            var result = new EffectArgSet();

            foreach (var param in rest)
                result.Add(param);

            return result;
        }
    }

    public sealed class EffectCommonArgs : EffectArgs
    {
        /// <summary>
        /// How this effect was triggered initially (e.g. by casting a spell, drinking a potion, traps, etc.). This is mostly used for message display.
        /// </summary>
        public string EffectSource { get; set; } = EffectSources.Default;
        
        /// <summary>
        /// How many tiles this spell can reach. Used by ball magic.
        /// </summary>
        public int TileRange { get; set; } = 1;

        /// <summary>
        /// If set to true after casting a spell, the thing holding the spell should be identified.
        /// </summary>
        public bool Obvious { get; set; } = true;
    }

    public static class EffectSources
    {
        public const string Default = "Default";
        public const string Spell = "Spell";
        public const string Action = "Action";
        public const string Scroll = "Scroll";
        public const string Wand = "Wand";
        public const string PotionDrunk = "PotionDrunk";
        public const string PotionThrown = "PotionThrown";
        public const string PotionSpilt = "PotionSpilt";
        public const string Trap = "Trap";
    }
}
