using OpenNefia.Analyzers;
using OpenNefia.Content.Charas;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.DisplayName
{
    public class ObjectDisplayNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeComponent<CharaComponent, GetDisplayNameEventArgs>(GetCharaName, priority: EventPriorities.Highest);
            SubscribeComponent<ItemComponent, GetDisplayNameEventArgs>(GetItemName, priority: EventPriorities.Highest);
            SubscribeComponent<CustomNameComponent, GetDisplayNameEventArgs>(GetCustomName, priority: EventPriorities.VeryHigh);
        }

        public void GetItemName(EntityUid uid, ItemComponent component, ref GetDisplayNameEventArgs args)
        {
            var ev = new GetItemNameEvent();
            EntityManager.EventBus.RaiseEvent(uid, ref ev);
            args.OutName = ev.ItemName;
        }

        public void GetCharaName(EntityUid uid, CharaComponent component, ref GetDisplayNameEventArgs args)
        {
            args.OutName = $"the {args.BaseName}";
        }

        private void GetCustomName(EntityUid uid, CustomNameComponent component, ref GetDisplayNameEventArgs args)
        {
            args.OutName = args.BaseName;
        }
    }

    [ByRefEvent]
    public struct GetItemNameEvent
    {
        public string ItemName = string.Empty;

        public GetItemNameEvent()
        {
        }
    }
}
