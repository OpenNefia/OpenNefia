using OpenNefia.Analyzers;
using OpenNefia.Content.Charas;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.Items;
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

            SubscribeComponent<CharaComponent, GetDisplayNameEventArgs>(GetCharaName, priority: EventPriorities.VeryLow);
            SubscribeComponent<ItemComponent, GetDisplayNameEventArgs>(GetItemName, priority: EventPriorities.Highest);
        }

        private void GetCharaName(EntityUid uid, CharaComponent component, ref GetDisplayNameEventArgs args)
        {
            // CustomNameComponent is applied before this.

            // TODO
            if (Loc.Language == LanguagePrototypeOf.English && !args.OutNoArticle)
                args.OutName = $"the {args.BaseName}";
        }

        public void GetItemName(EntityUid uid, ItemComponent component, ref GetDisplayNameEventArgs args)
        {
            var ev = new GetItemNameEvent(amount: args.AmountOverride, noArticle: args.OutNoArticle);
            EntityManager.EventBus.RaiseEvent(uid, ref ev);
            args.OutName = ev.OutItemName;
        }
    }
}