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

namespace OpenNefia.Content.Buffs
{
    public interface IBuffSystem : IEntitySystem
    {
        void RemoveAllBuffs(EntityUid entity, BuffsComponent? buffs = null);
        void AddBuff(EntityUid target, PrototypeId<EntityPrototype> id, int power, int turns, EntityUid source, BuffsComponent? buffs = null);
        bool HasBuff<T>(EntityUid ent, BuffsComponent? buffs = null)
            where T : class, IComponent;
        bool RemoveBuff(EntityUid entity, EntityUid buffEnt, bool refresh = true, BuffsComponent? buffs = null);
        IEnumerable<BuffComponent> EnumerateBuffs(EntityUid entity, BuffsComponent? buffs = null);
    }

    public sealed class BuffSystem : EntitySystem, IBuffSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IVisibilitySystem _visibilities = default!;
        [Dependency] private readonly IRefreshSystem _refreshes = default!;

        public override void Initialize()
        {
            SubscribeComponent<BuffsComponent, EntityRefreshEvent>(ApplyBuffs, priority: EventPriorities.High);
        }

        private void ApplyBuffs(EntityUid uid, BuffsComponent component, ref EntityRefreshEvent args)
        {
            foreach (var ent in component.Container.ContainedEntities.ToList())
            {
                if (!TryComp<BuffComponent>(ent, out var buff))
                {
                    EntityManager.DeleteEntity(ent);
                    continue;
                }

                if (buff.TimeRemaining > GameTimeSpan.Zero)
                {
                    var ev = new ApplyBuffOnRefreshEvent(uid);
                    RaiseEvent(ent, ref ev);
                }
            }
        }

        public void RemoveAllBuffs(EntityUid entity, BuffsComponent? buffs = null)
        {
            if (!Resolve(entity, ref buffs))
                return;

            foreach (var buffEnt in buffs.Container.ContainedEntities.ToList())
            {
                RemoveBuff(entity, buffEnt, refresh: false);
            }

            _refreshes.Refresh(entity);
        }

        public bool RemoveBuff(EntityUid entity, EntityUid buffEnt, bool refresh = true, BuffsComponent? buffs = null)
        {
            if (!Resolve(entity, ref buffs))
                return false;

            if (!buffs.Container.Contains(buffEnt))
            {
                Logger.ErrorS("buff", $"Entity {entity} did not contain buff {buffEnt}!");
                return false;
            }

            if (_gameSession.IsPlayer(entity) && _visibilities.PlayerCanSeeEntity(entity))
            {
                _mes.Display(Loc.GetString("Elona.Buffs.Ends", ("entity", entity), ("buff", buffEnt)), color: UiColors.MesPurple);
            }

            var ev = new BeforeBuffRemovedEvent(entity);
            RaiseEvent(buffEnt, ref ev);

            EntityManager.DeleteEntity(buffEnt);

            if (refresh)
                _refreshes.Refresh(entity);

            return true;
        }

        public void AddBuff(EntityUid target, PrototypeId<EntityPrototype> id, int power, int turns, EntityUid source, BuffsComponent? buffs = null)
        {
            if (!Resolve(target, ref buffs))
                return;

            // TODO
            _mes.Display($"TODO: Add buff {id}", UiColors.MesYellow);
        }

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
    }

    [ByRefEvent]
    [EventUsage(EventTarget.Buff)]
    public struct ApplyBuffOnRefreshEvent
    {
        public EntityUid Target { get; }

        public ApplyBuffOnRefreshEvent(EntityUid target)
        {
            Target = target;
        }
    }

    [ByRefEvent]
    [EventUsage(EventTarget.Buff)]
    public struct BeforeBuffRemovedEvent
    {
        public EntityUid Target { get; }

        public BeforeBuffRemovedEvent(EntityUid target)
        {
            Target = target;
        }
    }
}
