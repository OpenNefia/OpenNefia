using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using System.Text;

namespace OpenNefia.Content.GameObjects
{
    public class TargetTextSystem : EntitySystem
    {
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

            foreach (var entity in targetPos.GetEntities())
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

            if (!meta.IsAliveAndPrimary)
                return;

            if (Get<VisibilitySystem>().CanSeeEntity(args.Onlooker, target)
                && EntityManager.TryGetComponent(args.Onlooker, out SpatialComponent? onlookerSpatial))
            {
                var dist = (int)onlookerSpatial.Coords.DistanceTo(spatial.Coords);
                var targetLevelText = GetTargetDangerText(args.Onlooker, target);
                args.TargetTexts.Add("You are targeting " + DisplayNameSystem.GetDisplayName(target) + " (distance " + dist + ")");
                args.TargetTexts.Add(targetLevelText);
            }
        }

        public string GetTargetDangerText(EntityUid onlooker, EntityUid target)
        {
            return "(danger text)";
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
