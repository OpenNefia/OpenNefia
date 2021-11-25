using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Why.Core.GameObjects
{
    public class ItemNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ItemComponent, GetItemNameEvent>(BasicName);
        }

        public void BasicName(EntityUid uid, ItemComponent component, GetItemNameEvent args)
        {
            var jp = false;
            if (jp)
            {
                args.ItemName += BasicNameJP(uid, component);
            }
            else
            {
                args.ItemName += BasicNameEN(uid, component);
            }
        }

        public string BasicNameJP(EntityUid uid,
            ItemComponent? item = null,
            StackableComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref stack))
                return string.Empty;

            if (stack.Amount == 1)
                return $"{uid}";

            return $"{stack.Amount}個の{uid}";
        }

        public string BasicNameEN(EntityUid uid, 
            ItemComponent? item = null,
            StackableComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref stack))
                return string.Empty;

            if (stack.Amount == 1)
                return $"a {uid}";

            return $"{stack.Amount} {uid}s";
        }
    }
}
