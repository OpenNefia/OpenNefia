using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Locale;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Utility;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Random;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.CurseStates;

namespace OpenNefia.Content.Buffs
{
    public interface IBuffSystem : IEntitySystem
    {
        /// <summary>
        /// Tries to heal all buffs. Hexes like Punishment that cannot be healed
        /// will be ignored (see <see cref="BuffResistRemovalComponent"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="buffs"></param>
        void RemoveAllBuffs(EntityUid entity, BuffsComponent? buffs = null);

        /// <summary>
        /// Directly adds a buff, ignoring resistance and duplicates.
        /// </summary>
        bool TryAddBuffRaw(EntityUid target, EntityUid buff, BuffsComponent? buffs = null);

        /// <summary>
        /// Attempts to add a buff, first checking if there are any buffs with the same
        /// ID and potentially resisting if it's a hex.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="id"></param>
        /// <param name="power"></param>
        /// <param name="duration"></param>
        /// <param name="source"></param>
        /// <param name="buffs"></param>
        bool TryAddBuff(EntityUid target, PrototypeId<EntityPrototype> id, int power, [NotNullWhen(true)] out BuffComponent? buff, int? duration = null, EntityUid? source = null, CurseState curseState = CurseState.Normal, BuffsComponent? buffs = null);
        bool TryAddBuff(EntityUid target, PrototypeId<EntityPrototype> id, int power, int? duration = null, EntityUid? source = null, CurseState curseState = CurseState.Normal, BuffsComponent? buffs = null);

        bool TryGetBuff(EntityUid ent, PrototypeId<EntityPrototype> id, [NotNullWhen(true)] out BuffComponent? buff, BuffsComponent? buffs = null);
        bool TryGetBuff<T>(EntityUid ent, [NotNullWhen(true)] out BuffComponent? buff, BuffsComponent? buffs = null) where T : class, IComponent;

        bool HasBuff(EntityUid ent, PrototypeId<EntityPrototype> id, BuffsComponent? buffs = null);
        bool HasBuff<T>(EntityUid ent, BuffsComponent? buffs = null)
            where T : class, IComponent;
        bool RemoveBuff(EntityUid entity, EntityUid buffEnt, bool refresh = true, BuffsComponent? buffs = null);
        IEnumerable<BuffComponent> EnumerateBuffs(EntityUid entity, BuffsComponent? buffs = null);

        string LocalizeBuffDescription(EntityUid buff, BuffComponent? buffComp = null);

        IDictionary<string, double> GetBuffFormulaArgs(int basePower);
        BuffAdjustedPowerAndTurns CalcBuffPowerAndTurns(PrototypeId<EntityPrototype> buffID, int power, int? turns = null);
    }

    public sealed record class BuffAdjustedPowerAndTurns(int Power, int Turns);

    public sealed class BuffSystem : EntitySystem, IBuffSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IVisibilitySystem _visibilities = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IResistsSystem _resists = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IFormulaEngine _formulas = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<BuffsComponent, EntityRefreshEvent>(ApplyBuffs, priority: EventPriorities.High);
            SubscribeComponent<BuffPowerComponent, BeforeBuffAddedEvent>(AdjustBuffDuration, priority: EventPriorities.VeryHigh);
            SubscribeComponent<BuffsComponent, EntityPassTurnEventArgs>(UpdateBuffs, priority: EventPriorities.High);
            SubscribeComponent<BuffResistableComponent, BeforeBuffAddedEvent>(ProcResistableBuff, priority: EventPriorities.High);
        }

        public BuffAdjustedPowerAndTurns CalcBuffPowerAndTurns(PrototypeId<EntityPrototype> buffID, int power, int? turns = null)
        {
            var buffProto = _protos.Index(buffID);
            if (!buffProto.Components.TryGetComponent<BuffPowerComponent>(out var buffPower))
                return new(power, turns ?? 10);

            return CalcBuffPowerAndTurns(buffPower, power, turns);
        }

        public IDictionary<string, double> GetBuffFormulaArgs(int basePower)
        {
            var args = new Dictionary<string, double>();
            args["basePower"] = basePower;
            return args;
        }

        private BuffAdjustedPowerAndTurns CalcBuffPowerAndTurns(BuffPowerComponent buffPower, int power, int? turns = null)
        {
            var vars = GetBuffFormulaArgs(power);

            // Turn duration passed to AddBuff overrides that calculated by default formula
            if (turns == null)
                turns = (int)_formulas.Calculate(buffPower.Turns, vars, 10);

            power = (int)_formulas.Calculate(buffPower.Power, vars, power);

            return new(power, turns.Value);
        }

        private void AdjustBuffDuration(EntityUid uid, BuffPowerComponent buffPower, BeforeBuffAddedEvent args)
        {
            var adjusted = CalcBuffPowerAndTurns(buffPower, args.BasePower, args.OutTurns);
            args.OutTurns = adjusted.Turns;
            args.Buff.Power = adjusted.Power;
        }

        private void ApplyBuffs(EntityUid uid, BuffsComponent component, ref EntityRefreshEvent args)
        {
            foreach (var ent in component.Container.ContainedEntities.ToList())
            {
                if (!TryComp<BuffComponent>(ent, out var buff))
                {
                    component.Container.ForceRemove(ent);
                    EntityManager.DeleteEntity(ent);
                    continue;
                }

                if (buff.TurnsRemaining > 0)
                {
                    var ev = new ApplyBuffOnRefreshEvent(uid, buff);
                    RaiseEvent(ent, ref ev);
                }
            }
        }

        private void UpdateBuffs(EntityUid uid, BuffsComponent component, EntityPassTurnEventArgs args)
        {
            // >>>>>>>> shade2/main.hsp:772 	if cBuff(0,cc)!0{ ..
            var doRefresh = false;

            foreach (var buff in component.Container.ContainedEntities.ToList())
            {
                if (!IsAlive(buff) || !TryComp<BuffComponent>(buff, out var buffComp))
                {
                    doRefresh = true;
                    component.Container.ForceRemove(buff);
                    EntityManager.DeleteEntity(buff);
                    continue;
                }

                buffComp.TurnsRemaining -= 1;
                if (buffComp.TurnsRemaining <= 0)
                {
                    doRefresh = true;
                    var ev = new OnBuffExpiredEvent(uid);
                    RaiseEvent(buff, ev);
                    RemoveBuff(uid, buff, refresh: false);
                    continue;
                }
            }

            if (doRefresh)
                _refresh.Refresh(uid);
            // <<<<<<<< shade2/main.hsp:782 		} ..
        }

        public void RemoveAllBuffs(EntityUid entity, BuffsComponent? buffs = null)
        {
            if (!Resolve(entity, ref buffs))
                return;

            foreach (var buffEnt in buffs.Container.ContainedEntities.ToList())
            {
                // skip Punishment hex if found
                if (TryComp<BuffResistRemovalComponent>(buffEnt, out var buffResRemove))
                {
                    if (buffResRemove.NoRemoveOnHeal)
                        continue;
                }

                RemoveBuff(entity, buffEnt, refresh: false);
            }

            _refresh.Refresh(entity);
        }

        public bool RemoveBuff(EntityUid target, EntityUid buffEnt, bool refresh = true, BuffsComponent? buffs = null)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:583 #deffunc delBuff int tc,int id ...
            if (!Resolve(target, ref buffs))
                return false;

            if (!buffs.Container.Contains(buffEnt))
            {
                Logger.ErrorS("buff", $"Entity {target} did not contain buff {buffEnt}!");
                return false;
            }

            if (_gameSession.IsPlayer(target) && _visibilities.PlayerCanSeeEntity(target))
            {
                _mes.Display(Loc.GetString("Elona.Buff.Ends", ("entity", target), ("buff", buffEnt)), color: UiColors.MesPurple);
            }

            var ev = new BeforeBuffRemovedEvent(target);
            RaiseEvent(buffEnt, ev);

            var buffComp = EnsureComp<BuffComponent>(buffEnt);
            var ev2 = new BeforeEntityLosesBuffEvent(buffComp);
            RaiseEvent(target, ev2);

            buffs.Container.ForceRemove(buffEnt);
            EntityManager.DeleteEntity(buffEnt);

            if (refresh)
                _refresh.Refresh(target);

            return true;
            // <<<<<<<< elona122/shade2/chara_func.hsp:602 	return ...
        }

        private void ProcResistableBuff(EntityUid buffEnt, BuffResistableComponent component, BeforeBuffAddedEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:561 		f=false ...
            if (args.Cancelled)
                return;

            var magicResist = _resists.Level(args.Target, Protos.Element.Magic);
            var quality = _qualities.GetQuality(args.Target);
            var buffPower = args.Buff.Power;

            var resisted = false;

            if (magicResist / 2 > _rand.Next(buffPower * 2 + 100))
            {
                resisted = true;
            }

            if (buffPower * 3 < magicResist)
            {
                resisted = true;
            }

            if (buffPower / 3 > magicResist)
            {
                resisted = false;
            }

            if (quality > Quality.Good)
            {
                if (_rand.OneIn(4))
                {
                    resisted = true;
                }
                else
                {
                    args.OutTurns = args.OutTurns / 5 + 1;
                }
            }

            if (TryComp<BuffResistableQualityComponent>(buffEnt, out var resistQuality))
            {
                if (quality >= resistQuality.ResistQuality
                    // Added in Elona+ (allies can still be affected by Death Word)
                    && _factions.GetRelationToPlayer(args.Target) < Relation.Ally)
                {
                    resisted = true;
                }
            }

            if (TryGetBuff<BuffHolyVeilComponent>(args.Target, out var theirBuff))
            {
                if (theirBuff.Power + 50 > buffPower * 5 / 2
                    || _rand.Next(theirBuff.Power + 50) > _rand.Next(buffPower + 1))
                {
                    args.Cancel();
                    args.OutShowResistedMessage = false;
                    _mes.Display(Loc.GetString("Elona.Buff.Apply.Repelled"));
                    return;
                }
            }

            if (resisted)
            {
                args.Cancel();
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:573 			} ...
        }

        public bool TryAddBuff(EntityUid target, PrototypeId<EntityPrototype> id, int power, [NotNullWhen(true)] out BuffComponent? buffComp, int? duration = null, EntityUid? source = null, CurseState curseState = CurseState.Normal, BuffsComponent? buffs = null)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:549 #deffunc addBuff int tc,int buff,int power,int dur ...
            if (!Resolve(target, ref buffs) || duration <= 0)
            {
                buffComp = null;
                return false;
            }

            if (HasBuff(target, id, buffs))
            {
                _mes.Display(Loc.GetString("Elona.Buff.Apply.NoEffect"));
                buffComp = null;
                return false;
            }

            var buff = _entityGen.SpawnEntity(id, MapCoordinates.Global);
            if (!IsAlive(buff) || !TryComp<BuffComponent>(buff.Value, out buffComp))
            {
                _mes.Display(Loc.GetString("Elona.Buff.Apply.NoEffect"));
                buffComp = null;
                return false;
            }

            buffComp.BasePower = power;
            buffComp.Power = power;

            var ev = new BeforeBuffAddedEvent(buffComp, target, power, duration, curseState, source);
            RaiseEvent(buff.Value, ev);

            if (ev.OutTurns == null)
            {
                _mes.Display(Loc.GetString("Elona.Buff.Apply.NoEffect"));

                EntityManager.DeleteEntity(buff.Value);
                buffComp = null;
                return false;
            }

            if (ev.Cancelled)
            {
                if (ev.OutShowResistedMessage)
                    _mes.Display(Loc.GetString("Elona.Buff.Apply.Resists", ("target", target)));

                EntityManager.DeleteEntity(buff.Value);
                buffComp = null;
                return false;
            }

            // >>>>>>>> elona122/shade2/chara_func.hsp:574 		if cc@=pc:hostileAction pc,tc ...
            if (IsAlive(source) && _factions.IsPlayer(source.Value) && buffComp.Alignment == BuffAlignment.Negative)
            {
                _factions.ActHostileTowards(source.Value, target);
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:574 		if cc@=pc:hostileAction pc,tc ...

            buffComp.TurnsRemaining = ev.OutTurns.Value;
            buffComp.Source = source;

            if (!TryAddBuffRaw(target, buff.Value, buffs))
            {
                buffComp = null;
                return false;
            }

            return true;
            // <<<<<<<< elona122/shade2/chara_func.hsp:581 	return ...
        }

        public bool TryAddBuff(EntityUid target, PrototypeId<EntityPrototype> id, int power, int? duration = null, EntityUid? source = null, CurseState curseState = CurseState.Normal, BuffsComponent? buffs = null)
            => TryAddBuff(target, id, power, out _, duration, source, curseState, buffs);

        public bool TryAddBuffRaw(EntityUid target, EntityUid buff, BuffsComponent? buffs = null)
        {
            if (!Resolve(target, ref buffs) || !TryComp<BuffComponent>(buff, out var buffComp))
                return false;

            var protoID = MetaData(buff).EntityPrototype?.GetStrongID();
            if (protoID != null && Loc.TryGetPrototypeString(protoID.Value, "Buff.Apply", out var mes, ("target", target)))
                _mes.Display(mes, entity: target);

            buffs.Container.Insert(buff);

            var ev = new AfterEntityReceivedBuffEvent(buffComp);
            RaiseEvent(target, ev);

            _refresh.Refresh(target);

            return true;
        }

        public bool TryGetBuff(EntityUid ent, PrototypeId<EntityPrototype> id, [NotNullWhen(true)] out BuffComponent? buff, BuffsComponent? buffs = null)
        {
            if (!Resolve(ent, ref buffs))
            {
                buff = null;
                return false;
            }

            if (!buffs.Container.ContainedEntities
                .TryFirstOrNull(b => MetaData(b).EntityPrototype?.GetStrongID() == id, out var buffEnt))
            {
                buff = null;
                return false;
            }

            return TryComp<BuffComponent>(buffEnt, out buff);
        }

        public bool TryGetBuff<T>(EntityUid ent, [NotNullWhen(true)] out BuffComponent? buff, BuffsComponent? buffs = null)
            where T : class, IComponent
        {
            if (!Resolve(ent, ref buffs))
            {
                buff = null;
                return false;
            }

            if (!buffs.Container.ContainedEntities
                .TryFirstOrNull(b => HasComp<T>(b), out var buffEnt))
            {
                buff = null;
                return false;
            }

            return TryComp<BuffComponent>(buffEnt, out buff);
        }

        public bool HasBuff(EntityUid ent, PrototypeId<EntityPrototype> id, BuffsComponent? buffs = null)
            => TryGetBuff(ent, id, out _, buffs);

        public bool HasBuff<T>(EntityUid ent, BuffsComponent? buffs = null)
            where T : class, IComponent
        {
            if (!Resolve(ent, ref buffs))
                return false;

            return buffs.Container.ContainedEntities.Any(b => HasComp<T>(b));
        }

        public IEnumerable<BuffComponent> EnumerateBuffs(EntityUid entity, BuffsComponent? buffs = null)
        {
            if (!Resolve(entity, ref buffs))
                yield break;

            foreach (var ent in buffs.Container.ContainedEntities.ToList())
            {
                if (TryComp<BuffComponent>(ent, out var buff))
                    yield return buff;
                else
                    Logger.ErrorS("buff", $"Buff entity {ent} (on: {entity}) did not have a {nameof(BuffComponent)}!");
            }
        }

        public string LocalizeBuffDescription(EntityUid buff, BuffComponent? buffComp = null)
        {
            var proto = MetaData(buff).EntityPrototype;
            if (proto == null || !Resolve(buff, ref buffComp))
                return "<no description>";

            var ev = new GetBuffDescriptionEvent(proto, buffComp.BasePower, buffComp.Power);
            RaiseEvent(ev);
            return ev.OutDescription;
        }
    }

    [EventUsage(EventTarget.Broadcast)]
    public sealed class GetBuffDescriptionEvent : HandledEntityEventArgs
    {
        public EntityPrototype BuffPrototype { get; }
        public int BasePower { get; }
        public int Power { get; }

        public string OutDescription { get; set; } = string.Empty;

        public GetBuffDescriptionEvent(EntityPrototype buffID, int basePower, int power)
        {
            BuffPrototype = buffID;
            BasePower = basePower;
            Power = power;
        }
    }

    [ByRefEvent]
    [EventUsage(EventTarget.Buff)]
    public struct ApplyBuffOnRefreshEvent
    {
        public EntityUid Target { get; }
        public BuffComponent Buff { get; }

        public ApplyBuffOnRefreshEvent(EntityUid target, BuffComponent buff)
        {
            Target = target;
            Buff = buff;
        }
    }

    /// <summary>
    /// Raised when the timer on this buff expires.
    /// </summary>
    [EventUsage(EventTarget.Buff)]
    public sealed class OnBuffExpiredEvent : EntityEventArgs
    {
        public EntityUid Target { get; }

        public OnBuffExpiredEvent(EntityUid target)
        {
            Target = target;
        }
    }

    /// <summary>
    /// Raised when this buff is removed, either via vanqish hex or its timer expired.
    /// </summary>
    [EventUsage(EventTarget.Buff)]
    public sealed class BeforeBuffRemovedEvent : EntityEventArgs
    {
        public EntityUid Target { get; }

        public BeforeBuffRemovedEvent(EntityUid target)
        {
            Target = target;
        }
    }

    [EventUsage(EventTarget.Buff)]
    public sealed class BeforeBuffAddedEvent : CancellableEntityEventArgs
    {
        public BuffComponent Buff { get; }
        public EntityUid Target { get; }
        public EntityUid? Source { get; }
        public CurseState CurseState { get; }
        public int BasePower { get; }

        public int? OutTurns { get; set; }
        public bool OutShowResistedMessage { get; set; } = true;

        public BeforeBuffAddedEvent(BuffComponent buff, EntityUid target, int power, int? duration, CurseState curseState, EntityUid? source)
        {
            Buff = buff;
            BasePower = power;
            OutTurns = duration;
            Source = source;
            Target = target;
            CurseState = curseState;
        }
    }

    [EventUsage(EventTarget.Buff)]
    public sealed class AfterEntityReceivedBuffEvent : EntityEventArgs
    {
        public BuffComponent Buff { get; }

        public AfterEntityReceivedBuffEvent(BuffComponent buff)
        {
            Buff = buff;
        }
    }

    [EventUsage(EventTarget.Buff)]
    public sealed class BeforeEntityLosesBuffEvent : EntityEventArgs
    {
        public BuffComponent Buff { get; }

        public BeforeEntityLosesBuffEvent(BuffComponent buff)
        {
            Buff = buff;
        }
    }
}
