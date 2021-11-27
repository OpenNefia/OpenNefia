using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    public class DisplayNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CharaComponent, GetDisplayNameEvent>(GetCharaName);
            SubscribeLocalEvent<ItemComponent, GetDisplayNameEvent>(GetItemName);
        }

        public void GetCharaName(EntityUid uid, CharaComponent component, ref GetDisplayNameEvent args)
        {
            args.Name = component.DisplayName;
        }

        public void GetItemName(EntityUid uid, ItemComponent component, ref GetDisplayNameEvent args)
        {
            var ev = new GetItemNameEvent();
            EntityManager.EventBus.RaiseLocalEvent(uid, ref ev);
            args.Name = ev.ItemName;
        }

        public static string GetDisplayName(EntityUid uid)
        {
            var ev = new GetDisplayNameEvent();
            Get<DisplayNameSystem>().EntityManager.EventBus.RaiseLocalEvent(uid, ref ev);
            return ev.Name;
        }
    }

    public struct GetDisplayNameEvent
    {
        public string Name = string.Empty;

        public GetDisplayNameEvent()
        {
        }
    }

    public struct GetItemNameEvent
    {
        public string ItemName = string.Empty;

        public GetItemNameEvent()
        {
        }
    }
}
