using OpenNefia.Core.Rendering;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Logic;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Core.Locale;

namespace OpenNefia.Content.UI.Layer
{
    public partial class FieldLayer
    {
        private void QueryAtlas(UiKeyInputEventArgs args)
        {
            var prompt = new Prompt<string>(new List<string>() { AtlasNames.Tile, AtlasNames.Chip });
            var result = prompt.Query();
            if (result.HasValue)
            {
                var atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(result.Value.ChoiceData);
                new PicViewLayer(atlas.Image).Query();
            }
        }

        private void QueryRepl(UiKeyInputEventArgs args)
        {
            _repl.Query();
        }

        private void MovePlayer(Direction dir)
        {
            var player = _gameSession.Player;

            if (player != null)
            {
                var oldPosition = player.Spatial.MapPosition;
                var newPosition = player.Spatial.MapPosition.Offset(dir.ToIntVec());
                var ev = new MoveEventArgs(oldPosition, newPosition);
                player.EntityManager.EventBus.RaiseLocalEvent(player.Uid, ev);
            }
        }

        private void RunVerbCommand(Verb verb, IEnumerable<Entity> ents)
        {
            var player = _gameSession.Player!;

            var verbSystem = EntitySystem.Get<VerbSystem>();

            foreach (var target in ents)
            {
                if (target.Uid != player.Uid)
                {
                    var verbs = verbSystem.GetLocalVerbs(player.Uid, target.Uid);
                    if (verbs.Contains(verb))
                    {
                        verbSystem.ExecuteVerb(player.Uid, target.Uid, verb);
                        break;
                    }
                }
            }

            RefreshScreen();
        }

        private IEnumerable<Entity> EntitiesUnderneath()
        {
            var player = _gameSession.Player!;
            var lookup = EntitySystem.Get<IEntityLookup>();
            return lookup.GetLiveEntitiesAtCoords(player.Spatial.MapPosition)
                .Where(e => e != player);
        }

        private IEnumerable<Entity> EntitiesInInventory()
        {
            var player = _gameSession.Player!;
            var inv = _entityManager.EnsureComponent<InventoryComponent>(player.Uid);
            return inv.Container.ContainedEntities.Select(uid => _entityManager.GetEntity(uid));
        }

        private void DrinkItem(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(DrinkableSystem.VerbIDDrink), EntitiesUnderneath());
        }

        private void ThrowItem(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(ThrowableSystem.VerbIDThrow), EntitiesUnderneath());
        }

        public void Ascend(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(StairsSystem.VerbIDAscend), EntitiesUnderneath());
        }

        public void Descend(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(StairsSystem.VerbIDDescend), EntitiesUnderneath());
        }

        public void Activate(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(StairsSystem.VerbIDActivate), EntitiesUnderneath());
        }

        private void PickUpItem(UiKeyInputEventArgs args)
        {
            var verb = new Verb(PickableSystem.VerbIDPickUp);
            var verbSystem = EntitySystem.Get<VerbSystem>();

            var ents = EntitiesUnderneath().ToList();

            if (ents.Count == 1)
            {
                var ent = ents.First();
                var result = verbSystem.ExecuteVerb(_gameSession.Player.Uid, ent.Uid, verb);

                if (result == TurnResult.NoResult)
                {
                    var mapEntity = _mapManager.GetMap(_gameSession.Player.Spatial.MapID);
                    result = verbSystem.ExecuteVerb(_gameSession.Player.Uid, mapEntity.MapEntityUid, verb);
                }
                if (result == TurnResult.NoResult)
                {
                    Mes.Display(Loc.Get("Elona.GameObjects.Pickable.GraspAtAir"));
                }
            }
            else
            {
                var context = new InventoryContext(_gameSession.Player!.Uid, new PickUpInventoryBehavior());
                var layer = new InventoryLayer(context);
                layer.Query();
            }

            RefreshScreen();
        }

        private void DropItem(UiKeyInputEventArgs args)
        {
            var context = new InventoryContext(_gameSession.Player!.Uid, new DropInventoryBehavior());
            var layer = new InventoryLayer(context);
            layer.Query();
        }

        private void Examine(UiKeyInputEventArgs args)
        {
            var context = new InventoryContext(_gameSession.Player!.Uid, new ExamineInventoryBehavior());
            var layer = new InventoryLayer(context);
            layer.Query();
        }

        public void PromptToQuit(UiKeyInputEventArgs args)
        {
            if (_playerQuery.YesOrNo("Quit to title screen?"))
                Cancel();
        }
    }
}
