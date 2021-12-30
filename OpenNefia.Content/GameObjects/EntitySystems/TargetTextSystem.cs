using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using System.Text;

namespace OpenNefia.Content.GameObjects
{
    public class TargetTextSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly DisplayNameSystem _displayNames = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<CharaComponent, GetTargetTextEventArgs>(HandleTargetTextChara, nameof(HandleTargetTextChara));
        }

        public bool GetTargetText(EntityUid onlooker, MapCoordinates targetPos, out string text, bool visibleOnly)
        {
            if (!Get<VisibilitySystem>().CanSeePosition(onlooker, targetPos))
            {
                text = "You can't see this position.";
                return false;
            }

            var sb = new StringBuilder();

            foreach (var entity in _lookup.GetLiveEntitiesAtCoords(targetPos))
            {
                var ev = new GetTargetTextEventArgs(onlooker, visibleOnly);
                RaiseLocalEvent(entity.Uid, ev);
                foreach (var line in ev.TargetTexts)
                {
                    sb.AppendLine(line);
                }
            }

            text = sb.ToString();
            return true;
        }

        private void HandleTargetTextChara(EntityUid uid, CharaComponent chara, GetTargetTextEventArgs args)
        {
            if (args.Handled)
                return;

            DoTargetTextChara(uid, args, chara);
        }

        private void DoTargetTextChara(EntityUid target, GetTargetTextEventArgs args, 
            CharaComponent? chara = null,
            SpatialComponent? spatial = null,
            MetaDataComponent? meta = null)
        {
            if (!Resolve(target, ref chara, ref spatial, ref meta))
                return;

            if (!meta.IsAlive)
                return;

            if (Get<VisibilitySystem>().CanSeeEntity(args.Onlooker, target)
                && EntityManager.TryGetComponent(args.Onlooker, out SpatialComponent? onlookerSpatial))
            {
                onlookerSpatial.MapPosition.TryDistance(spatial.MapPosition, out var dist);
                var targetLevelText = GetTargetDangerText(args.Onlooker, target);
                args.TargetTexts.Add("You are targeting " + _displayNames.GetDisplayNameInner(target) + " (distance " + (int)dist + ")");
                args.TargetTexts.Add(targetLevelText);
            }
        }

        public string GetTargetDangerText(EntityUid onlooker, EntityUid target)
        {
            return "(danger text)";
        }

        /// <hsp>txtItemOnCell(x, y)</hsp>
        public string? GetItemOnCellText(EntityUid player, MapCoordinates newPosition)
        {
            var ents = _lookup.GetLiveEntitiesAtCoords(newPosition).ToList();

            var items = ents.Where(ent => EntityManager.HasComponent<PickableComponent>(ent.Uid)).ToList();

            if (items.Count == 0)
                return null;

            if (items.Count > 3)
                return Loc.GetString("Elona.TargetText.ItemOnCell.MoreThanThree", ("itemCount", items.Count));

            var mes = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    mes.Append(Loc.GetString("Elona.TargetText.ItemOnCell.And"));
                }
                var item = items[i];
                mes.Append(_displayNames.GetDisplayNameInner(item.Uid));
            }

            var ownState = OwnState.None;
            if (EntityManager.TryGetComponent(items.First().Uid, out PickableComponent? pickable))
                ownState = pickable.OwnState;

            var itemNames = mes.ToString();

            switch (ownState)
            {
                case OwnState.None:
                    return Loc.GetString("Elona.TargetText.ItemOnCell.Item", ("itemNames", itemNames));
                case OwnState.Construct:
                    return Loc.GetString("Elona.TargetText.ItemOnCell.Construct", ("itemNames", itemNames));
                default:
                    return Loc.GetString("Elona.TargetText.ItemOnCell.NotOwned", ("itemNames", itemNames));
            }
        }
    }

    public class GetTargetTextEventArgs : HandledEntityEventArgs
    {
        public readonly List<string> TargetTexts = new();
        public readonly bool VisibleOnly;
        public readonly EntityUid Onlooker;

        public GetTargetTextEventArgs(EntityUid onlooker, bool visibleOnly)
        {
            VisibleOnly = visibleOnly;
            Onlooker = onlooker;
        }
    }
}
