using OpenNefia.Analyzers;
using OpenNefia.Content.Charas;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;

namespace OpenNefia.Content.DisplayName
{
    public class ObjectDisplayNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeComponent<ItemComponent, GetDisplayNameEventArgs>(GetItemName, priority: EventPriorities.Highest);
        }

        public void GetItemName(EntityUid uid, ItemComponent component, ref GetDisplayNameEventArgs args)
        {
            var ev = new GetItemNameEvent();
            EntityManager.EventBus.RaiseEvent(uid, ref ev);
            args.OutName = ev.ItemName;
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