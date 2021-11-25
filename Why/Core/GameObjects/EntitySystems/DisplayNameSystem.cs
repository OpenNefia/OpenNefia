using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.IoC;
using Why.Core.Log;
using Why.Core.Maps;
using Why.Core.Utility;

namespace Why.Core.GameObjects
{
    public class DisplayNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CharaComponent, GetDisplayNameEvent>(GetCharaName);
            SubscribeLocalEvent<ItemComponent, GetDisplayNameEvent>(GetItemName);
        }

        public void GetCharaName(EntityUid uid, CharaComponent component, GetDisplayNameEvent args)
        {
            if (args.Handled)
                return;

            args.Name = component.DisplayName;
            args.Handled = true;
        }

        public void GetItemName(EntityUid uid, ItemComponent component, GetDisplayNameEvent args)
        {
            if (args.Handled)
                return;

            var ev = new GetItemNameEvent();
            EntityManager.EventBus.RaiseLocalEvent(uid, ev);

            args.Name = ev.ItemName;
            args.Handled = true;
        }

        public static string GetDisplayName(EntityUid uid)
        {
            var ev = new GetDisplayNameEvent();
            Get<DisplayNameSystem>().EntityManager.EventBus.RaiseLocalEvent(uid, ev);
            return ev.Name;
        }
    }

    public sealed class GetDisplayNameEvent : HandledEntityEventArgs
    {
        public string Name = string.Empty;

        public GetDisplayNameEvent()
        {
        }
    }

    public sealed class GetItemNameEvent : EntityEventArgs
    {
        public string ItemName = string.Empty;

        public GetItemNameEvent()
        {
        }
    }
}
