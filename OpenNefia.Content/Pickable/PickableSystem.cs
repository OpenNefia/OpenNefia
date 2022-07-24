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
using OpenNefia.Core.Locale;
using OpenNefia.Core.Game;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.Pickable
{
    public class PickableSystem : EntitySystem
    {
        public const string VerbTypePickUp = "Elona.PickUp";
        public const string VerbTypeDrop = "Elona.Drop";

        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        private readonly PrototypeId<SoundPrototype>[] GetSounds = new[]
        {
            Protos.Sound.Get1,
            Protos.Sound.Get2
        };

        public override void Initialize()
        {
            SubscribeComponent<PickableComponent, GetVerbsEventArgs>(HandleGetVerbs);
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
                    args.Verbs.Add(new Verb(VerbTypeDrop, "Drop Item", () => Drop(args.Source, args.Target)));
                else
                    args.Verbs.Add(new Verb(VerbTypePickUp, "Pick Up Item", () => PickUp(args.Source, args.Target)));
            }
        }

        private bool CheckPickableOwnState(PickableComponent pickable)
        {
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

            if (!CheckPickableOwnState(pickable))
                return TurnResult.Failed;

            var success = pickerInv.Container.Insert(item);

            if (success)
            {
                _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.PicksUp", ("entity", picker), ("target", item)));

                var sound = _random.Pick(GetSounds);
                _sounds.Play(sound, picker);

                var showMessage = _gameSession.IsPlayer(picker);
                _stackSystem.TryStackAtSamePos(item, showMessage: showMessage);

                // Don't exit the inventory screen (like if TurnResult.Success were used here)
                return TurnResult.NoResult;
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

                return TurnResult.Succeeded;
            }
            else
            {
                Logger.WarningS("sys.pickable", "Failed to drop item");
                return TurnResult.Failed;
            }
        }
    }
}
