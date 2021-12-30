using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.DisplayName
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
            if (Loc.Language == LanguagePrototypeOf.Japanese)
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
            StackComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return string.Empty;

            var basename = meta.DisplayName;

            if (stack.Count == 1)
                return $"{basename}";

            return $"{stack.Count}個の{basename}";
        }

        public string BasicNameEN(EntityUid uid,
            ItemComponent? item = null,
            MetaDataComponent? meta = null,
            StackComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return string.Empty;

            var basename = meta.DisplayName;

            if (stack.Count == 1)
                return $"a {basename}";

            return $"{stack.Count} {basename}s";
        }
    }
}
