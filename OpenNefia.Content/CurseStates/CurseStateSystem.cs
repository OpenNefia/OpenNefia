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

namespace OpenNefia.Content.CurseStates
{
    public interface ICurseStateSystem : IEntitySystem
    {
        bool IsBlessed(EntityUid ent, CurseStateComponent? curseState = null);
        bool IsCursed(EntityUid ent, CurseStateComponent? curseState = null);
    }

    public class CurseStateSystem : EntitySystem, ICurseStateSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<CurseStateComponent, EntityBeingGeneratedEvent>(TryRandomizeCurseState, priority: EventPriorities.High);
            SubscribeComponent<CurseStateComponent, GotEquippedInMenuEvent>(OnEquippedInMenu, priority: EventPriorities.High);
            SubscribeComponent<CurseStateComponent, BeingUnequippedAttemptEvent>(OnBeingUnequipped, priority: EventPriorities.High);
        }

        private void TryRandomizeCurseState(EntityUid uid, CurseStateComponent component, ref EntityBeingGeneratedEvent args)
        {

            if (component.NoRandomizeCurseState)
                return;

            // TODO

            if (TryComp<TagComponent>(uid, out var tags))
            {
                foreach (var tag in tags.Tags)
                {
                    if (_protos.TryGetExtendedData<TagPrototype, ExtDefaultCurseState>(tag, out var def))
                    {
                        component.CurseState = def.CurseState;
                    }
                }
            }
        }

        private void OnEquippedInMenu(EntityUid item, CurseStateComponent component, GotEquippedInMenuEvent args)
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

            return curseState.CurseState == CurseState.Blessed;
        }

        public bool IsCursed(EntityUid ent, CurseStateComponent? curseState = null)
        {
            if (!Resolve(ent, ref curseState))
                return false;

            return curseState.CurseState == CurseState.Cursed || curseState.CurseState == CurseState.Doomed;
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
