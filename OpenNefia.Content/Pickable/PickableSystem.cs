using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Log;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Game;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.Pickable
{
    public interface IPickableSystem : IEntitySystem
    {
        bool CheckNoDropAndMessage(EntityUid item, PickableComponent? pickable = null);
        bool CheckPickableOwnStateAndMessage(EntityUid item, PickableComponent? pickable = null);
        TurnResult PickUp(EntityUid picker, EntityUid item, PickableComponent? pickable = null);
        TurnResult Drop(EntityUid picker, EntityUid item, PickableComponent? pickable = null);
    }

    public class PickableSystem : EntitySystem, IPickableSystem
    {
        public const string VerbTypePickUp = "Elona.PickUp";
        public const string VerbTypeDrop = "Elona.Drop";

        [Dependency] private readonly IContainerSystem _containerSystem = default!;
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        // TODO: SoundSpecifier instead
        public static readonly PrototypeId<SoundPrototype>[] GetSounds = new[]
        {
            Protos.Sound.Get1,
            Protos.Sound.Get2
        };

        public override void Initialize()
        {
            SubscribeComponent<PickableComponent, GetVerbsEventArgs>(HandleGetVerbs, priority: EventPriorities.VeryHigh);
            SubscribeComponent<PickableComponent, EntityBeingGeneratedEvent>(HandleBeingGenerated);
        }

        private void HandleBeingGenerated(EntityUid uid, PickableComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (args.GenArgs.TryGet<ItemGenArgs>(out var itemGenArgs))
                component.OwnState = itemGenArgs.OwnState;
        }

        private void HandleGetVerbs(EntityUid uid, PickableComponent pickable, GetVerbsEventArgs args)
        {
            if (EntityManager.HasComponent<InventoryComponent>(args.Source))
            {
                if (_containerSystem.ContainsEntity(args.Source, uid))
                    args.OutVerbs.Add(new Verb(VerbTypeDrop, "Drop Item", () => Drop(args.Source, args.Target)));
                else
                    args.OutVerbs.Add(new Verb(VerbTypePickUp, "Pick Up Item", () => PickUp(args.Source, args.Target)));
            }
        }

        public bool CheckNoDropAndMessage(EntityUid item, PickableComponent? pickable = null)
        {
            if (!Resolve(item, ref pickable))
                return true;

            if (pickable.IsNoDrop)
            {
                Sounds.Play(Protos.Sound.Fail1);
                _mes.Display(Loc.GetString("Elona.Inventory.Common.SetAsNoDrop"));
                return false;
            }

            return true;
        }

        public bool CheckPickableOwnStateAndMessage(EntityUid item, PickableComponent? pickable = null)
        {
            if (!Resolve(item, ref pickable))
            {
                _sounds.Play(Protos.Sound.Fail1);
                _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.CannotCarry"));
                return false;
            }

            switch (pickable.OwnState)
            {
                case OwnState.NPC:
                    _sounds.Play(Protos.Sound.Fail1);
                    _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.NotOwned"));
                    return false;
                case OwnState.Shop:
                    _sounds.Play(Protos.Sound.Fail1);
                    _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.CannotCarry"));
                    return false;
                default:
                    return true;
            }
        }

        public TurnResult PickUp(EntityUid picker, EntityUid item, PickableComponent? pickable = null)
        {
            if (!Resolve(item, ref pickable))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent<InventoryComponent>(picker, out var pickerInv))
                return TurnResult.Failed;

            if (!CheckPickableOwnStateAndMessage(item, pickable))
                return TurnResult.Failed;

            var success = pickerInv.Container.Insert(item);

            if (success)
            {
                _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.PicksUp", ("entity", picker), ("target", item)));

                var sound = _random.Pick(GetSounds);
                _sounds.Play(sound, picker);

                var ev = new AfterItemPickedUpEvent(picker);
                EntityManager.EventBus.RaiseEvent(item, ev);

                var showMessage = _gameSession.IsPlayer(picker);
                _stackSystem.TryStackAtSamePos(item, showMessage: showMessage);

                return TurnResult.Succeeded;
            }
            else
            {
                Logger.WarningS("sys.pickable", "Failed to take item");
                return TurnResult.Failed;
            }
        }

        public TurnResult Drop(EntityUid picker, EntityUid item, PickableComponent? pickable = null)
        {
            if (!Resolve(item, ref pickable))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent<InventoryComponent>(picker, out var pickerInv))
                return TurnResult.Failed;

            var success = pickerInv.Container.Remove(item, EntityManager);

            if (success)
            {
                _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.Drops", ("entity", picker), ("target", item)));

                _sounds.Play(Protos.Sound.Drop1, picker);

                var ev = new AfterItemDroppedEvent(picker);
                EntityManager.EventBus.RaiseEvent(item, ev);

                return TurnResult.Succeeded;
            }
            else
            {
                Logger.WarningS("sys.pickable", "Failed to drop item");
                return TurnResult.Failed;
            }
        }
    }

    public sealed class AfterItemPickedUpEvent : EntityEventArgs
    {
        public AfterItemPickedUpEvent(EntityUid picker)
        {
            Picker = picker;
        }

        public EntityUid Picker { get; }
    }

    public sealed class AfterItemDroppedEvent : EntityEventArgs
    {
        public AfterItemDroppedEvent(EntityUid picker)
        {
            Picker = picker;
        }

        public EntityUid Picker { get; }
    }
}
