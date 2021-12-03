namespace OpenNefia.Core.GameObjects
{
    public class DisplayNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MetaDataComponent, GetDisplayNameEventArgs>(GetFallbackName);
        }

        public void GetFallbackName(EntityUid uid, MetaDataComponent component, ref GetDisplayNameEventArgs args)
        {
            if (args.Handled)
                return;

            args.Name = component.DisplayName ?? $"<entity {uid}>";
            args.Handled = true;
        }

        public static string GetDisplayName(EntityUid uid)
        {
            var ev = new GetDisplayNameEventArgs();
            Get<DisplayNameSystem>().EntityManager.EventBus.RaiseLocalEvent(uid, ref ev);
            return ev.Name;
        }
    }

    public class GetDisplayNameEventArgs : HandledEntityEventArgs
    {
        public string Name = string.Empty;
    }
}
