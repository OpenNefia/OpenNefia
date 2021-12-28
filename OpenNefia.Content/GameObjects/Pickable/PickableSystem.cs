using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Log;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Locale;

namespace OpenNefia.Content.GameObjects.Pickable
{
    public class PickableSystem : EntitySystem
    {
        public const string VerbIDPickUp = "Elona.PickUp";
        public const string VerbIDDrop = "Elona.Drop";

        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly IAudioSystem _sounds = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;

        private readonly PrototypeId<SoundPrototype>[] GetSounds = new[]
        {
            Protos.Sound.Get1,
            Protos.Sound.Get2
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<PickableComponent, GetVerbsEventArgs>(HandleGetVerbs, nameof(HandleGetVerbs));
            SubscribeLocalEvent<ExecuteVerbEventArgs>(HandleExecuteVerb, nameof(HandleExecuteVerb));
            SubscribeLocalEvent<PickableComponent, DoPickUpEventArgs>(HandleDoPickUp, nameof(HandleDoPickUp));
            SubscribeLocalEvent<PickableComponent, DoDropEventArgs>(HandleDoDrop, nameof(HandleDoDrop));
        }

        private void HandleGetVerbs(EntityUid uid, PickableComponent pickable, GetVerbsEventArgs args)
        {
            if (EntityManager.HasComponent<InventoryComponent>(args.Source))
            {
                if (_containerSystem.ContainsEntity(args.Source, uid))
                    args.Verbs.Add(new Verb(VerbIDDrop));
                else
                    args.Verbs.Add(new Verb(VerbIDPickUp));
            }
        }

        private void HandleExecuteVerb(ExecuteVerbEventArgs args)
        {
            if (args.Handled)
                return;

            switch (args.Verb.ID)
            {
                case VerbIDPickUp:
                    Raise(args.Target, new DoPickUpEventArgs(args.Source), args);
                    break;
                case VerbIDDrop:
                    Raise(args.Target, new DoDropEventArgs(args.Source), args);
                    break;
            }
        }

        private void HandleDoPickUp(EntityUid target, PickableComponent pickable, DoPickUpEventArgs args)
        {
            args.Handle(PickUp(target, args.Picker, pickable));
        }

        private void HandleDoDrop(EntityUid target, PickableComponent pickable, DoDropEventArgs args)
        {
            args.Handle(Drop(target, args.Dropper, pickable));
        }

        private bool CheckPickableOwnState(PickableComponent pickable)
        {
            switch (pickable.OwnState)
            {
                case OwnState.NPC:
                    _sounds.Play(Protos.Sound.Fail1);
                    Mes.Display(Loc.Get("Elona.GameObjects.Pickable.NotOwned"));
                    return false;
                case OwnState.Shop:
                    _sounds.Play(Protos.Sound.Fail1);
                    Mes.Display(Loc.Get("Elona.GameObjects.Pickable.CannotCarry"));
                    return false;
                default:
                    return true;
            }
        }

        public TurnResult PickUp(EntityUid target, EntityUid picker, PickableComponent? pickable = null)
        {
            if (!Resolve(target, ref pickable))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent<InventoryComponent>(picker, out var pickerInv))
                return TurnResult.Failed;

            if (!CheckPickableOwnState(pickable))
                return TurnResult.Failed;

            var success = pickerInv.Container.Insert(target);

            if (success)
            {
                Mes.Display(Loc.Get("Elona.GameObjects.Pickable.PicksUp", ("entity", picker), ("target", target)));

                var sound = _random.Pick(GetSounds);
                _sounds.Play(sound, picker);

                _stackSystem.TryStackAtSamePos(target);

                return TurnResult.Succeeded;
            }
            else
            {
                Logger.WarningS("sys.pickable", "Failed to take item");
                return TurnResult.Failed;
            }
        }

        public TurnResult Drop(EntityUid target, EntityUid picker, PickableComponent? pickable = null)
        {
            if (!Resolve(target, ref pickable))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent<InventoryComponent>(picker, out var pickerInv))
                return TurnResult.Failed;

            var success = pickerInv.Container.Remove(target, EntityManager);

            if (success)
            {
                Mes.Display(Loc.Get("Elona.GameObjects.Pickable.Drops", ("entity", picker), ("target", target)));

                _sounds.Play(Protos.Sound.Drop1, picker);

                return TurnResult.Succeeded;
            }
            else
            {
                Logger.WarningS("sys.pickable", "Failed to drop item");
                return TurnResult.Failed;
            }
        }
    }

    public class DoPickUpEventArgs : TurnResultEntityEventArgs
    {
        public readonly EntityUid Picker;

        public DoPickUpEventArgs(EntityUid picker)
        {
            Picker = picker;
        }
    }

    public class DoDropEventArgs : TurnResultEntityEventArgs
    {
        public readonly EntityUid Dropper;

        public DoDropEventArgs(EntityUid dropper)
        {
            Dropper = dropper;
        }
    }
}
