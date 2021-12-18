using OpenNefia.Analyzers;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.GameObjects
{
    public class ObjectDisplayNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CharaComponent, GetDisplayNameEventArgs>(GetCharaName, nameof(GetCharaName));
            SubscribeLocalEvent<ItemComponent, GetDisplayNameEventArgs>(GetItemName, nameof(GetItemName));
        }

        public void GetCharaName(EntityUid uid, CharaComponent component, ref GetDisplayNameEventArgs args)
        {
            if (args.Handled || !EntityManager.TryGetComponent(uid, out MetaDataComponent metaData))
                return;

            args.Name = $"the {metaData.DisplayName}";
            args.Handled = true;
        }

        public void GetItemName(EntityUid uid, ItemComponent component, ref GetDisplayNameEventArgs args)
        {
            var ev = new GetItemNameEvent();
            EntityManager.EventBus.RaiseLocalEvent(uid, ref ev);
            args.Name = ev.ItemName;
        }
    }

    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct GetItemNameEvent
    {
        public string ItemName = string.Empty;

        public GetItemNameEvent()
        {
        }
    }
}
