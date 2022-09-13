using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using System.Text;

namespace OpenNefia.Content.GameObjects
{
    public interface ITargetTextSystem : IEntitySystem
    {
        string? GetItemOnCellText(EntityUid player, MapCoordinates newPosition);
        string GetTargetDangerText(EntityUid onlooker, EntityUid target);
        bool GetTargetText(EntityUid onlooker, MapCoordinates targetPos, out string text, bool visibleOnly);
    }

    public class TargetTextSystem : EntitySystem, ITargetTextSystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaComponent, GetTargetTextEventArgs>(HandleTargetTextChara);
        }

        public bool GetTargetText(EntityUid onlooker, MapCoordinates targetPos, out string text, bool visibleOnly)
        {
            if (!Get<VisibilitySystem>().HasLineOfSight(onlooker, targetPos))
            {
                text = Loc.GetString("Elona.TargetText.CannotSeeLocation");
                return false;
            }

            var sb = new StringBuilder();

            foreach (var spatial in _lookup.GetLiveEntitiesAtCoords(targetPos))
            {
                var ev = new GetTargetTextEventArgs(onlooker, visibleOnly);
                RaiseEvent(spatial.Owner, ev);
                foreach (var line in ev.TargetTexts)
                {
                    sb.AppendLine(Loc.Capitalize(line));
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

            if (Get<VisibilitySystem>().CanSeeEntity(target, args.Onlooker)
                && EntityManager.TryGetComponent(args.Onlooker, out SpatialComponent? onlookerSpatial))
            {
                onlookerSpatial.MapPosition.TryDistanceTiled(spatial.MapPosition, out var dist);
                var targetLevelText = GetTargetDangerText(args.Onlooker, target);
                args.TargetTexts.Add("You are targeting " + _displayNames.GetDisplayName(target) + " (distance " + dist + ")");
                args.TargetTexts.Add(targetLevelText);
            }
        }

        public string GetTargetDangerText(EntityUid onlooker, EntityUid target)
        {
            var ourLevel = _levels.GetLevel(onlooker);
            var theirLevel = _levels.GetLevel(target);

            int dangerLevel; // higher is more dangerous

#pragma warning disable format
            if      (ourLevel * 20    < theirLevel) dangerLevel = 10;
            else if (ourLevel * 10    < theirLevel) dangerLevel = 9;
            else if (ourLevel * 5     < theirLevel) dangerLevel = 8;
            else if (ourLevel * 3     < theirLevel) dangerLevel = 7;
            else if (ourLevel * 2     < theirLevel) dangerLevel = 6;
            else if (ourLevel * 3 / 2 < theirLevel) dangerLevel = 5;
            else if (ourLevel         < theirLevel) dangerLevel = 4;
            else if (ourLevel / 3 * 2 < theirLevel) dangerLevel = 3;
            else if (ourLevel / 2     < theirLevel) dangerLevel = 2;
            else if (ourLevel / 3     < theirLevel) dangerLevel = 1;
            else                                    dangerLevel = 0;
#pragma warning restore format

            return Loc.GetString($"Elona.TargetText.DangerLevel.{dangerLevel}", ("target", target));
        }

        /// <hsp>txtItemOnCell(x, y)</hsp>
        public string? GetItemOnCellText(EntityUid player, MapCoordinates newPosition)
        {
            var ents = _lookup.GetLiveEntitiesAtCoords(newPosition).ToList();

            var items = ents.Where(ent => EntityManager.HasComponent<PickableComponent>(ent.Owner)).ToList();

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
                mes.Append(_displayNames.GetDisplayName(item.Owner));
            }

            var ownState = OwnState.None;
            if (EntityManager.TryGetComponent(items.First().Owner, out PickableComponent? pickable))
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

    public sealed class GetTargetTextEventArgs : HandledEntityEventArgs
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
