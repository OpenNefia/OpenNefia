using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots.Events;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Random;
using OpenNefia.Content.CharaMake;
using OpenNefia.Content.Qualities;
using System.ComponentModel;
using Pidgin;

namespace OpenNefia.Content.CurseStates
{
    public interface ICurseStateSystem : IEntitySystem
    {
        bool IsBlessed(EntityUid ent, CurseStateComponent? curseState = null);
        bool IsCursed(EntityUid ent, CurseStateComponent? curseState = null);
        bool IsBlessed(CurseState state);
        bool IsCursed(CurseState state);
        CurseState GetDefaultCurseState(EntityUid uid);
        CurseState PickRandomCurseState(EntityUid uid);
    }

    public class CurseStateSystem : EntitySystem, ICurseStateSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public override void Initialize()
        {
            SubscribeComponent<CurseStateComponent, EntityBeingGeneratedEvent>(SetRandomCurseState, priority: EventPriorities.High);
            SubscribeComponent<CurseStateComponent, WasEquippedInMenuEvent>(OnEquippedInMenu, priority: EventPriorities.High);
            SubscribeComponent<CurseStateComponent, BeingUnequippedAttemptEvent>(OnBeingUnequipped, priority: EventPriorities.High);
        }

        public CurseState GetDefaultCurseState(EntityUid uid)
        {
            if (CompOrNull<QualityComponent>(uid)?.Quality == Quality.Unique)
                return CurseState.Normal;

            if (TryComp<TagComponent>(uid, out var tags))
            {
                foreach (var tag in tags.Tags)
                {
                    if (_protos.TryGetExtendedData<TagPrototype, ExtDefaultCurseState>(tag, out var def))
                    {
                        return def.CurseState;
                    }
                }
            }

            // No default curse state should be set at this point, so we can go ahead and randomize.
            return PickRandomCurseState(uid);
        }

        public CurseState PickRandomCurseState(EntityUid uid)
        {
            var curseState = CurseState.Normal;
            if (_rand.OneIn(12))
            {
                curseState = CurseState.Blessed;
            }
            if (_rand.OneIn(13))
            {
                curseState = CurseState.Cursed;
                if (HasComp<EquipmentComponent>(uid) && _rand.OneIn(4))
                    curseState = CurseState.Doomed;
            }
            return curseState;
        }

        private void SetRandomCurseState(EntityUid uid, CurseStateComponent curseState, ref EntityBeingGeneratedEvent args)
        {
            if (curseState.NoRandomizeCurseState)
                return;

            if (args.GenArgs.Has<CharaMakeGenArgs>())
                return;

            curseState.CurseState = GetDefaultCurseState(uid);
        }

        private void OnEquippedInMenu(EntityUid item, CurseStateComponent component, WasEquippedInMenuEvent args)
        {
            string? key = component.CurseState switch
            {
                CurseState.Cursed => "Elona.CurseState.Equipped.Cursed",
                CurseState.Doomed => "Elona.CurseState.Equipped.Doomed",
                CurseState.Blessed => "Elona.CurseState.Equipped.Blessed",
                _ => null
            };

            if (key != null)
            {
                _mes.Display(Loc.GetString(key, ("actor", args.Equipee), ("target", args.EquipTarget), ("item", item)));
            }
        }

        private void OnBeingUnequipped(EntityUid uid, CurseStateComponent component, BeingUnequippedAttemptEvent args)
        {
            if (IsCursed(uid))
            {
                args.Reason = Loc.GetString("Elona.CurseState.CannotBeTakenOff", ("entity", uid));
                args.Cancel();
            }
        }

        public bool IsBlessed(EntityUid ent, CurseStateComponent? curseState = null)
        {
            if (!Resolve(ent, ref curseState))
                return false;

            return IsBlessed(curseState.CurseState);
        }

        public bool IsCursed(EntityUid ent, CurseStateComponent? curseState = null)
        {
            if (!Resolve(ent, ref curseState))
                return false;

            return IsCursed(curseState.CurseState);
        }

        public bool IsBlessed(CurseState state)
        {
            return state == CurseState.Blessed;
        }

        public bool IsCursed(CurseState state)
        {
            return state == CurseState.Cursed
                || state == CurseState.Doomed;
        }
    }

    /// <summary>
    /// When attached to a tag prototype, indicates that items with this tag should
    /// be inititialized with a default curse state instead of <see cref="CurseState.None"/>.
    /// </summary>
    public sealed class ExtDefaultCurseState : IPrototypeExtendedData<TagPrototype>
    {
        [DataField]
        public CurseState CurseState { get; }
    }
}
