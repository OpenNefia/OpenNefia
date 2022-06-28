using OpenNefia.Analyzers;
using OpenNefia.Content.Charas;
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

            SubscribeLocalEvent<CharaComponent, GetDisplayNameEventArgs>(GetCharaName, nameof(GetCharaName),
                after: new [] { QualitySystem.HandlerAddQualityBrackets });
            SubscribeLocalEvent<ItemComponent, GetDisplayNameEventArgs>(GetItemName, nameof(GetItemName));
        }

        public void GetCharaName(EntityUid uid, CharaComponent component, ref GetDisplayNameEventArgs args)
        {
            // This is called after DisplayNameSystem puts the base name of the
            // entity in the event args.
            args.Name = $"the {args.Name}";
        }

        public void GetItemName(EntityUid uid, ItemComponent component, ref GetDisplayNameEventArgs args)
        {
            var ev = new GetItemNameEvent();
            EntityManager.EventBus.RaiseLocalEvent(uid, ref ev);
            args.Name = ev.ItemName;
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
