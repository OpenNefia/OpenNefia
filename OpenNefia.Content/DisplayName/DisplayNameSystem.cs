using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.DisplayName
{
    public class DisplayNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MetaDataComponent, GetDisplayNameEventArgs>(GetFallbackName, nameof(GetFallbackName));
        }

        public void GetFallbackName(EntityUid uid, MetaDataComponent component, ref GetDisplayNameEventArgs args)
        {
            if (args.Handled)
                return;

            args.Name = component.DisplayName ?? $"<entity {uid}>";
            args.Handled = true;
        }

        public string GetDisplayNameInner(EntityUid uid)
        {
            var ev = new GetDisplayNameEventArgs();
            EntityManager.EventBus.RaiseLocalEvent(uid, ref ev);
            return ev.Name;
        }

        public static string GetDisplayName(EntityUid uid)
        {
            return Get<DisplayNameSystem>().GetDisplayNameInner(uid);
        }
    }

    public class GetDisplayNameEventArgs : HandledEntityEventArgs
    {
        public string Name = string.Empty;
    }
}
