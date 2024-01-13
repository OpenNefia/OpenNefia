using OpenNefia.Content.Enchantments;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;
using OpenNefia.Content.World;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Game;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Karma;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Utility;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomEvent;
using OpenNefia.Core.Random;
using OpenNefia.Content.TurnOrder;
using static OpenNefia.Content.Prototypes.Protos;
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
        void HealAllBuffs(EntityUid entity, BuffsComponent? buffs = null);

        /// <summary>
        /// Directly adds a buff, ignoring resistance and duplicates.
        /// </summary>
        void AddBuffRaw(EntityUid target, EntityUid buff, BuffsComponent? buffs = null);

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
        bool AddBuff(EntityUid target, PrototypeId<EntityPrototype> id, int power, int? duration = null, EntityUid? source = null, CurseState curseState = CurseState.Normal, BuffsComponent? buffs = null);

        bool TryGetBuff(EntityUid ent, PrototypeId<EntityPrototype> id, [NotNullWhen(true)] out BuffComponent? buff, BuffsComponent? buffs = null);
        bool TryGetBuff<T>(EntityUid ent, [NotNullWhen(true)] out BuffComponent? buff, BuffsComponent? buffs = null) where T : class, IComponent;

        bool HasBuff(EntityUid ent, PrototypeId<EntityPrototype> id, BuffsComponent? buffs = null);
        bool HasBuff<T>(EntityUid ent, BuffsComponent? buffs = null)
            where T : class, IComponent;
        bool RemoveBuff(EntityUid entity, EntityUid buffEnt, bool refresh = true, BuffsComponent? buffs = null);
        IEnumerable<BuffComponent> EnumerateBuffs(EntityUid entity, BuffsComponent? buffs = null);

        string GetBuffDescription(EntityUid buff, BuffComponent? buffComp = null);
    }

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

        public override void Initialize()
        {
            SubscribeComponent<BuffsComponent, EntityRefreshEvent>(ApplyBuffs, priority: EventPriorities.High);
            SubscribeComponent<BuffPowerComponent, BeforeBuffAddedEvent>(AdjustBuffDuration, priority: EventPriorities.VeryHigh);
            SubscribeComponent<BuffsComponent, EntityPassTurnEventArgs>(UpdateBuffs, priority: EventPriorities.High);
            SubscribeComponent<BuffResistableComponent, BeforeBuffAddedEvent>(ProcResistableBuff, priority: EventPriorities.High);
        }

        private void AdjustBuffDuration(EntityUid uid, BuffPowerComponent component, BeforeBuffAddedEvent args)
        {
            var vars = new Dictionary<string, double>();
            vars["basePower"] = args.OutPower;

            // Turn duration passed to AddBuff overrides that calculated by default formula
            if (args.OutTurns == null)
                args.OutTurns = (int)_formulas.Calculate(component.Turns, vars, 10);

            args.OutPower = (int)_formulas.Calculate(component.Power, vars, args.OutPower);
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

        public void HealAllBuffs(EntityUid entity, BuffsComponent? buffs = null)
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

        public bool RemoveBuff(EntityUid entity, EntityUid buffEnt, bool refresh = true, BuffsComponent? buffs = null)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:583 #deffunc delBuff int tc,int id ...
            if (!Resolve(entity, ref buffs))
                return false;

            if (!buffs.Container.Contains(buffEnt))
            {
                Logger.ErrorS("buff", $"Entity {entity} did not contain buff {buffEnt}!");
                return false;
            }

            if (_gameSession.IsPlayer(entity) && _visibilities.PlayerCanSeeEntity(entity))
            {
                _mes.Display(Loc.GetString("Elona.Buff  .Ends", ("entity", entity), ("buff", buffEnt)), color: UiColors.MesPurple);
            }

            var ev = new BeforeBuffRemovedEvent(entity);
            RaiseEvent(buffEnt, ev);

            buffs.Container.ForceRemove(buffEnt);
            EntityManager.DeleteEntity(buffEnt);

            if (refresh)
                _refresh.Refresh(entity);

            return true;
            // <<<<<<<< elona122/shade2/chara_func.hsp:602 	return ...
        }

        private void ProcResistableBuff(EntityUid uid, BuffResistableComponent component, BeforeBuffAddedEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:561 		f=false ...
            if (args.Cancelled)
                return;

            var magicResist = _resists.Level(args.Target, Protos.Element.Magic);
            var quality = _qualities.GetQuality(args.Target);
            var buffPower = Comp<BuffComponent>(uid).Power;

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

            if (TryComp<BuffResistableQualityComponent>(uid, out var resistQuality))
            {
                if (quality >= resistQuality.ResistQuality)
                {
                    resisted = true;
                }
            }

            if (TryGetBuff<BuffHolyVeilComponent>(uid, out var theirBuff))
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

        public bool AddBuff(EntityUid target, PrototypeId<EntityPrototype> id, int power, int? duration = null, EntityUid? source = null, CurseState curseState = CurseState.Normal, BuffsComponent? buffs = null)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:549 #deffunc addBuff int tc,int buff,int power,int dur ...
            if (!Resolve(target, ref buffs) || duration <= 0)
                return false;

            if (HasBuff(target, id, buffs))
            {
                _mes.Display(Loc.GetString("Elona.Buff.Apply.NoEffect"));
                return false;
            }

            var buff = _entityGen.SpawnEntity(id, MapCoordinates.Global);
            if (!IsAlive(buff) || !TryComp<BuffComponent>(buff.Value, out var buffComp))
            {
                _mes.Display(Loc.GetString("Elona.Buff.Apply.NoEffect"));
                return false;
            }

            var ev = new BeforeBuffAddedEvent(target, power, duration, curseState, source);
            RaiseEvent(buff.Value, ev);

            if (ev.OutTurns == null)
            {
                _mes.Display(Loc.GetString("Elona.Buff.Apply.NoEffect"));
                return false;
            }

            if (ev.Cancelled)
            {
                if (ev.OutShowResistedMessage)
                    _mes.Display(Loc.GetString("Elona.Buff.Apply.Resists", ("target", target)));

                EntityManager.DeleteEntity(buff.Value);

                return false;
            }

            // >>>>>>>> elona122/shade2/chara_func.hsp:574 		if cc@=pc:hostileAction pc,tc ...
            if (IsAlive(source) && _factions.IsPlayer(source.Value) && buffComp.Alignment == BuffAlignment.Negative)
            {
                _factions.ActHostileTowards(source.Value, target);
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:574 		if cc@=pc:hostileAction pc,tc ...

            buffComp.BasePower = power;
            buffComp.Power = ev.OutPower;
            buffComp.TurnsRemaining = ev.OutTurns.Value;
            buffComp.Source = source;

            AddBuffRaw(target, buff.Value, buffs);
            return true;
            // <<<<<<<< elona122/shade2/chara_func.hsp:581 	return ...
        }

        public void AddBuffRaw(EntityUid target, EntityUid buff, BuffsComponent? buffs = null)
        {
            if (!Resolve(target, ref buffs))
                return;

            var protoID = MetaData(buff).EntityPrototype?.GetStrongID();
            if (protoID != null && Loc.TryGetPrototypeString(protoID.Value, "Buff.Apply", out var mes, ("target", target)))
                _mes.Display(mes, entity: target);

            buffs.Container.Insert(buff);

            _refresh.Refresh(target);
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

        public string GetBuffDescription(EntityUid buff, BuffComponent? buffComp = null)
        {
            var protoID = MetaData(buff).EntityPrototype?.GetStrongID();
            if (protoID == null)
                return "???";

            var args = new List<LocaleArg>()
            {
                ("buff", buff)
            };

            return Loc.GetPrototypeString(protoID.Value, "Buff.Description", args.ToArray());
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
        public EntityUid Target { get; }
        public EntityUid? Source { get; }
        public CurseState CurseState { get; }

        public int OutPower { get; set; }
        public int? OutTurns { get; set; }
        public bool OutShowResistedMessage { get; set; } = false;

        public BeforeBuffAddedEvent(EntityUid target, int power, int? duration, CurseState curseState, EntityUid? source)
        {
            OutPower = power;
            OutTurns = duration;
            Source = source;
            Target = target;
            CurseState = curseState;
        }
    }
}
