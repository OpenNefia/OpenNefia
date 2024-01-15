using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Log;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Skills;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Combat;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Feats;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// New implementation of the effect system. This is an ECS-based version designed
    /// around combining components:
    /// - All effects are entities with an <see cref="EffectComponent"/>.
    /// - Effects should subscribe to <see cref="GetEffectPlayerTargetEvent"/> so they can
    ///   provide a default target if none is provided.
    ///   The <see cref="EffectTargetOtherComponent"/> targets an entity or the ground,
    ///   and <see cref="EffectTargetDirectionComponent"/> targets a cardinal direction.
    /// - You can customize how the effect is applied and its range with an "effect area"
    ///   component like <see cref="EffectAreaBallComponent"/> for AoE or just 
    ///   omit all area components to skip area targeting.
    /// - Effect area components should raise <see cref="ApplyEffectDamageEvent"/>, which applies
    ///   the effect to the target. In turn, "effect damage" components will listen for 
    ///   this event to apply the actual effect. These include
    ///   <see cref="EffectDamageElementalComponent"/> for elemental damage, among others.
    /// </summary>
    /// <remarks>
    /// To summarize:
    /// - <see cref="INewEffectSystem"/> raises <see cref="ApplyEffectAreaEvent"/>.
    /// - Event handlers of <see cref="ApplyEffectAreaEvent"/> should raise <see cref="ApplyEffectDamageEvent"/>
    ///   and possibly <see cref="ApplyEffectTileDamageEvent"/>.
    /// - Event handlers of <see cref="ApplyEffectDamageEvent"/> could call 
    /// <see cref="INewEffectSystem.Apply"/> recursively for even more complex effects.
    /// Following this convention allows you to mix and match different effect behaviors
    /// to easily create new effects from the existing parts.
    /// </remarks>
    public interface INewEffectSystem : IEntitySystem
    {
        bool TrySpawnEffect(PrototypeId<EntityPrototype> effectID, [NotNullWhen(true)] out EntityUid? effect, bool retainEffectEntity = false);

        /// <summary>
        /// Applies an effect.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="effectID"></param>
        /// <param name="args"></param>
        /// <param name="retainEffectEntity">If false, the effect entity will be deleted after the effect is applied.</param>
        /// <returns>Turn result from the effect.</returns>
        TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, PrototypeId<EntityPrototype> effectID, int? power = null, EffectArgSet? args = null, bool retainEffectEntity = false);

        TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, PrototypeId<EntityPrototype> effectID, EffectArgSet args, bool retainEffectEntity = false);

        TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, EntityUid effect, int? power = null, EffectArgSet? args = null);

        bool TryGetEffectTarget(EntityUid source, EntityUid value, EffectArgSet args, [NotNullWhen(true)] out EffectTarget? target);
        int CalcEffectAdjustedPower(EffectAlignment alignment, int power, CurseState curseState);

        IDictionary<string, double> GetEffectDamageFormulaArgs(EntityUid effectUid, EntityUid source, EntityUid? target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, int power, int skillLevel);
        IDictionary<string, double> GetEffectDamageFormulaArgs(EntityUid effectUid, ApplyEffectDamageEvent args);
        IDictionary<string, double> GetEffectDamageFormulaArgs(EntityUid effectUid, ApplyEffectAreaEvent args);

        /// <summary>
        /// Gets the dice of an effect. Useful for displaying it in the UI.
        /// The dice returned is the same as that used by the effect system with all
        /// modifiers due to feats/etc. (Itzpalt's' buff) applied.
        /// The effect should have a <see cref="EffectBaseDamageDiceComponent"/> for the
        /// output dice to be useful.
        /// </summary>
        /// <returns></returns>
        bool TryGetEffectDice(EntityUid source, EntityUid? target, EntityUid effectUid,
            int power, int skillLevel, [NotNullWhen(true)] out Dice? dice, [NotNullWhen(true)] out IDictionary<string, double>? formulaArgs, EntityCoordinates? sourceCoords = null, EntityCoordinates? targetCoords = null, EffectBaseDamageDiceComponent? effectDice = null);

        /// <summary>
        /// Gets the dice of an effect given its base X, Y and bonus.
        /// Applies modifiers to these parameters based on feats 
        /// (such as Itzpalt's' buff).
        /// </summary>
        /// <returns></returns>
        Dice GetEffectDice(EntityUid source, EntityUid? target, EntityUid effectUid,
            int diceX, int diceY, int bonus);
    }

    public sealed record class EffectTarget(EntityUid? Target, EntityCoordinates? Coords);

    public sealed class NewEffectSystem : EntitySystem, INewEffectSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IFormulaEngine _formulaEngine = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;

        public IDictionary<string, double> GetEffectDamageFormulaArgs(EntityUid effectUid, EntityUid source, EntityUid? target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, int power, int skillLevel)
        {
            var result = new Dictionary<string, double>();

            result["power"] = power;
            result["skillLevel"] = skillLevel;
            result["casterLevel"] = _levels.GetLevel(source);
            result["targetLevel"] = target != null ? _levels.GetLevel(target.Value) : 0;
            if (sourceCoords.TryDistanceFractional(EntityManager, targetCoords, out var dist))
            {
                result["distance"] = dist;
            }

            return result;
        }

        public IDictionary<string, double> GetEffectDamageFormulaArgs(EntityUid effectUid, ApplyEffectDamageEvent args)
            => GetEffectDamageFormulaArgs(effectUid, args.Source, args.InnerTarget, args.SourceCoords, args.TargetCoords, args.Args.Power, args.Args.SkillLevel);

        public IDictionary<string, double> GetEffectDamageFormulaArgs(EntityUid effectUid, ApplyEffectAreaEvent args)
            => GetEffectDamageFormulaArgs(effectUid, args.Source, args.Target, args.SourceCoords, args.TargetCoords, args.Args.Power, args.Args.SkillLevel);

        public bool TrySpawnEffect(PrototypeId<EntityPrototype> effectID, [NotNullWhen(true)] out EntityUid? effect, bool retainEffectEntity = false)
        {
            effect = _entityGen.SpawnEntity(effectID, MapCoordinates.Global);
            if (!IsAlive(effect) || !HasComp<EffectComponent>(effect.Value))
            {
                Logger.ErrorS("effect", $"Failed to cast event {effectID}, entity could not be spawned or has no {nameof(EffectComponent)}");
                if (effect != null && !retainEffectEntity)
                    EntityManager.DeleteEntity(effect.Value);
                return false;
            }

            MetaData(effect.Value).IsMapSavable = false;

            return true;
        }

        public TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, PrototypeId<EntityPrototype> effectID, int? power = null, EffectArgSet? args = null, bool retainEffectEntity = false)
        {
            if (!TrySpawnEffect(effectID, out var effect, retainEffectEntity))
                return TurnResult.Aborted;

            var result = Apply(source, target, targetCoords, effect.Value, power, args);

            if (IsAlive(effect) && !retainEffectEntity)
                EntityManager.DeleteEntity(effect.Value);

            return result;
        }

        public TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, PrototypeId<EntityPrototype> effectID, EffectArgSet args, bool retainEffectEntity = false)
            => Apply(source, target, targetCoords, effectID, null, args, retainEffectEntity);

        public TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, EntityUid effect, int? power = null, EffectArgSet? args = null)
        {
            if (target == null)
            {
                target = null;
                targetCoords ??= Spatial(source).Coordinates;
            }
            else
            {
                targetCoords = Spatial(target.Value).Coordinates;
            }

            var sourceCoords = Spatial(source).Coordinates;

            args ??= new EffectArgSet();
            var common = args.Ensure<EffectCommonArgs>();

            if (power != null)
                args.Power = power.Value;

            if (!common.NoInheritItemCurseState)
            {
                if (IsAlive(common.SourceItem) && TryComp<CurseStateComponent>(common.SourceItem, out var curseStateComp))
                    args.CurseState = curseStateComp.CurseState;
            }

            var effectComp = Comp<EffectComponent>(effect);
            args.Power = CalcEffectAdjustedPower(effectComp.Alignment, args.Power, args.CurseState);

            var ev2 = new ApplyEffectAreaEvent(source, target, sourceCoords, targetCoords.Value, args);
            Raise(effect, ev2);

            return ev2.TurnResult;
        }

        public int CalcEffectAdjustedPower(EffectAlignment alignment, int power, CurseState curseState)
        {
            switch (alignment)
            {
                case EffectAlignment.Neutral:
                default:
                    return power;
                case EffectAlignment.Positive:
                    switch (curseState)
                    {
                        case CurseState.Blessed:
                            return (int)(power * 1.5);
                        case CurseState.Cursed:
                        case CurseState.Doomed:
                            return 50;
                        default:
                            return power;
                    }
                case EffectAlignment.Negative:
                    switch (curseState)
                    {
                        case CurseState.Blessed:
                            return 50;
                        case CurseState.Cursed:
                        case CurseState.Doomed:
                            return (int)(power * 1.5);
                        default:
                            return power;
                    }
            }
        }

        public bool TryGetEffectTarget(EntityUid source, EntityUid effect, EffectArgSet args, [NotNullWhen(true)] out EffectTarget? target)
        {
            if (!IsAlive(effect))
            {
                target = null;
                return false;
            }

            var ev = new GetEffectPlayerTargetEvent(source, args);
            RaiseEvent(effect, ev);
            if (!ev.Handled || (ev.OutTarget == null && ev.OutCoords == null))
            {
                target = null;
                return false;
            }

            target = new(ev.OutTarget, ev.OutCoords);
            return true;
        }

        /// <inheritdoc/>
        public bool TryGetEffectDice(EntityUid source, EntityUid? target, EntityUid effectUid,
            int power, int skillLevel, [NotNullWhen(true)] out Dice? dice, [NotNullWhen(true)] out IDictionary<string, double>? formulaArgs, EntityCoordinates? sourceCoords = null, EntityCoordinates? targetCoords = null, EffectBaseDamageDiceComponent? effectDice = null)
        {
            if (!Resolve(effectUid, ref effectDice, logMissing: false))
            {
                dice = null;
                formulaArgs = null;
                return false;
            }

            sourceCoords ??= Spatial(source).Coordinates;
            targetCoords ??= IsAlive(target) ? Spatial(target.Value).Coordinates : sourceCoords.Value;

            formulaArgs = GetEffectDamageFormulaArgs(effectUid, source, target, sourceCoords.Value, targetCoords.Value, power, skillLevel);

            var diceX = int.Max((int)_formulaEngine.Calculate(effectDice.DiceX, formulaArgs, 0f), 0);
            var diceY = int.Max((int)_formulaEngine.Calculate(effectDice.DiceY, formulaArgs, 0f), 0);
            var bonus = (int)_formulaEngine.Calculate(effectDice.Bonus, formulaArgs, 0f);

            dice = GetEffectDice(source, target, effectUid, diceX, diceY, bonus);
            return true;
        }

        /// <inheritdoc/>
        public Dice GetEffectDice(EntityUid source, EntityUid? target, EntityUid effectUid,
            int diceX, int diceY, int bonus)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1688 	if cc=pc : if trait(traitGodElement):if (ele=rsRe ...
            // TODO move
            if (_feats.HasFeat(source, Protos.Feat.GodElement))
            {
                if (TryComp<EffectDamageElementalComponent>(effectUid, out var effElemental))
                {
                    if (effElemental.Element == Protos.Element.Fire
                        || effElemental.Element == Protos.Element.Cold
                        || effElemental.Element == Protos.Element.Lightning)
                    {
                        diceY = (int)(diceY * 1.25);
                    }
                }
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1688 	if cc=pc : if trait(traitGodElement):if (ele=rsRe ...

            // >>>>>>>> elona122/shade2/proc.hsp:1689 	if rapidMagic : efP=efP/2+1:dice1=dice1/2+1:dice2 ...
            // TODO move
            if (TryComp<EffectDamageCastByRapidMagicComponent>(effectUid, out var rapidMagic) && rapidMagic.TotalAttackCount > 1)
            {
                diceX = diceX / 2 + 1;
                diceY = diceY / 2 + 1;
                bonus = bonus / 2 + 1;
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1689 	if rapidMagic : efP=efP/2+1:dice1=dice1/2+1:dice2 ...

            return new Dice(diceX, diceY, bonus);
        }
    }

    /// <summary>
    /// Raised to retrieve the primary target or location of this effect,
    /// if it was <c>null</c>.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class GetEffectPlayerTargetEvent : HandledEntityEventArgs
    {
        public EntityUid Source { get; }
        public EffectArgSet Args { get; }

        public EntityUid? OutTarget { get; set; } = null;
        public EntityCoordinates? OutCoords { get; set; } = null;

        public GetEffectPlayerTargetEvent(EntityUid source, EffectArgSet args)
        {
            Source = source;
            Args = args;
        }

        public void Handle(EntityUid? target, EntityCoordinates? coords = null)
        {
            OutTarget = target;
            OutCoords = coords;
            Handled = true;
        }
    }

    /// <summary>
    /// Raised to retrieve the primary target if the AI is casting this effect.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class GetEffectAITargetEvent : HandledEntityEventArgs
    {
        public EntityUid Source { get; }
        public EffectArgSet Args { get; }

        public EntityUid? OutTarget { get; set; } = null;
        public EntityCoordinates? OutCoords { get; set; } = null;

        public GetEffectAITargetEvent(EntityUid source, EffectArgSet args)
        {
            Source = source;
            Args = args;
        }

        public void Handle(EntityUid? target, EntityCoordinates? coords = null)
        {
            OutTarget = target;
            OutCoords = coords;
            Handled = true;
        }
    }

    public interface IApplyEffectEvent
    {
        public EntityCoordinates SourceCoords { get; }
        public EntityCoordinates TargetCoords { get; }

        public MapCoordinates SourceCoordsMap { get; }
        public MapCoordinates TargetCoordsMap { get; }

        public IMap SourceMap { get; }
        public IMap TargetMap { get; }
    }

    /// <summary>
    /// Runs the actual logic of the event after checks like MP/stamina/targeting
    /// have been completed. Will run area-based logic for calculating effect targets
    /// like ball AoE and bolt piercing.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class ApplyEffectAreaEvent : TurnResultEntityEventArgs, IApplyEffectEvent
    {
        /// <summary>
        /// Person who casted the event.
        /// </summary>
        public EntityUid Source { get; }

        public EntityUid? Target { get; }

        /// <summary>
        /// Automatically set to the location of <see cref="Target"/> if
        /// not set manually.
        /// </summary>
        public EntityCoordinates SourceCoords { get; }
        public EntityCoordinates TargetCoords { get; }

        public MapCoordinates SourceCoordsMap => SourceCoords.ToMap(IoCManager.Resolve<IEntityManager>());
        public MapCoordinates TargetCoordsMap => TargetCoords.ToMap(IoCManager.Resolve<IEntityManager>());

        public IMap SourceMap => IoCManager.Resolve<IMapManager>().GetMap(SourceCoordsMap.MapId);
        public IMap TargetMap => IoCManager.Resolve<IMapManager>().GetMap(TargetCoordsMap.MapId);

        public EffectArgSet Args { get; }
        public EffectCommonArgs CommonArgs => Args.Ensure<EffectCommonArgs>();

        public ApplyEffectAreaEvent(EntityUid source, EntityUid? target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, EffectArgSet args)
        {
            Source = source;
            Target = target;
            SourceCoords = sourceCoords;
            TargetCoords = targetCoords;
            Args = args;
        }
    }

    /// <summary>
    /// Applies effect tile damage, like fire burning items on the ground.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class ApplyEffectTileDamageEvent : HandledEntityEventArgs
    {
        /// <summary>
        /// Person who casted the event.
        /// </summary>
        public EntityUid Source { get; }

        /// <summary>
        /// Automatically set to the location of <see cref="InnerTarget"/> if
        /// not set manually.
        /// </summary>
        public EntityCoordinates Coords { get; }

        public MapCoordinates CoordsMap => Coords.ToMap(IoCManager.Resolve<IEntityManager>());

        public IMap Map => IoCManager.Resolve<IMapManager>().GetMap(CoordsMap.MapId);

        public EffectArgSet Args { get; }

        public bool OutEffectWasObvious { get; set; } = false;

        public ApplyEffectTileDamageEvent(EntityUid source, EntityCoordinates coords, EffectArgSet args)
        {
            Source = source;
            Coords = coords;
            Args = args;
        }
    }

    /// <summary>
    /// Applies the effect to a single target. 
    /// Raised once for each entity affected by an AoE.
    /// 
    /// If this effect is not handled by any handler,
    /// then a "Nothing happens..." message is printed,
    /// and the effect result is marked as non-obvious (item will
    /// not be identified automatically)
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class ApplyEffectDamageEvent : TurnResultEntityEventArgs, IApplyEffectEvent
    {
        /// <summary>
        /// Person who casted the event.
        /// </summary>
        public EntityUid Source { get; }

        /// <summary>
        /// Target of the effect. May be different from the original target
        /// in the case of AoE. May be <c>null</c> if the effect targets the ground.
        /// </summary>
        public EntityUid? InnerTarget { get; set; }

        public EntityCoordinates SourceCoords { get; }

        /// <summary>
        /// Coordinates of the target entity or targeted position, 
        /// These are guaranteed to be available even if there is no <see cref="InnerTarget"/>.
        /// </summary>
        public EntityCoordinates TargetCoords { get; }

        public MapCoordinates SourceCoordsMap => SourceCoords.ToMap(IoCManager.Resolve<IEntityManager>());
        public MapCoordinates TargetCoordsMap => TargetCoords.ToMap(IoCManager.Resolve<IEntityManager>());

        public IMap SourceMap => IoCManager.Resolve<IMapManager>().GetMap(SourceCoordsMap.MapId);
        public IMap TargetMap => IoCManager.Resolve<IMapManager>().GetMap(TargetCoordsMap.MapId);

        public EffectArgSet Args { get; }
        public EffectCommonArgs CommonArgs => Args.Ensure<EffectCommonArgs>();

        /// <summary>
        /// Number of affected tiles if this event was invoked with an AoE.
        /// </summary>
        public int AffectedTileCount { get; }

        /// <summary>
        /// Index of the tile being affected, starting from 0.
        /// </summary>
        public int AffectedTileIndex { get; } = 0;

        /// <summary>
        /// A damage property for calculating damage somewhere in the effect chain.
        /// </summary>
        public int OutDamage { get; set; } = 0;

        /// <summary>
        /// A damage property for calculating elemental power somewhere in the effect chain.
        /// </summary>
        public int OutElementalPower { get; set; } = 0;

        /// <summary>
        /// Set to false if the effect failed so that the associated item is not identified.
        /// </summary>
        public bool OutEffectWasObvious { get; set; } = true;

        public ApplyEffectDamageEvent(EntityUid source, EntityUid? target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, EffectArgSet args, int affectedTiles, int affectedTileIndex)
        {
            Source = source;
            InnerTarget = target;
            SourceCoords = sourceCoords;
            TargetCoords = targetCoords;
            Args = args;
            AffectedTileCount = affectedTiles;
            AffectedTileIndex = affectedTileIndex;
        }
    }

    /// <summary>
    /// Retrieves extra details for the effect's description, typically dice/power.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class GetEffectDescriptionEvent : HandledEntityEventArgs
    {
        public EntityUid Caster { get; }
        public int Power { get; }
        public int SkillLevel { get; }

        public string OutDescription { get; set; } = string.Empty;

        public GetEffectDescriptionEvent(EntityUid caster, int power, int skillLevel, string description)
        {
            Caster = caster;
            Power = power;
            SkillLevel = skillLevel;
            OutDescription = description;
        }
    }
}