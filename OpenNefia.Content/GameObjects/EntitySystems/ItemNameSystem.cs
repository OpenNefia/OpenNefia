using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class ItemNameSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ItemComponent, GetItemNameEvent>(BasicName, "BasicName");
        }

        public void BasicName(EntityUid uid, ItemComponent component, ref GetItemNameEvent args)
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
            MetaDataComponent? meta = null,
            StackableComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return string.Empty;

            if (stack.Amount == 1)
                return $"{meta.DisplayName}";

            return $"{stack.Amount}個の{meta.DisplayName}";
        }

        public string BasicNameEN(EntityUid uid,
            ItemComponent? item = null,
            MetaDataComponent? meta = null,
            StackableComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return string.Empty;

            if (stack.Amount == 1)
                return $"a {meta.DisplayName}";

            return $"{stack.Amount} {meta.DisplayName}s";
        }
    }
}
