using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Locale.Funcs;
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
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ItemComponent, GetItemNameEvent>(BasicName, "BasicName");
        }

        public void BasicName(EntityUid uid, ItemComponent component, ref GetItemNameEvent args)
        {
            switch(Loc.Language)
            {
                case var jp when jp == LanguagePrototypeOf.Japanese:
                    args.ItemName += BasicNameJP(uid, component);
                    break;
                case var en when en == LanguagePrototypeOf.English:
                    args.ItemName += BasicNameEN(uid, component);
                    break;
                case var de when de == LanguagePrototypeOf.German:
                    args.ItemName += BasicNameDE(uid, component);
                    break;
            }
        }

        public string BasicNameJP(EntityUid uid,
            ItemComponent? item = null,
            MetaDataComponent? meta = null,
            StackComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return $"<item {uid}>";

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
                return $"<item {uid}>";

            var basename = meta.DisplayName;

            if (stack.Count == 1)
                return $"a {basename}";

            return $"{stack.Count} {basename}s";
        }

        public string BasicNameDE(EntityUid uid,
            ItemComponent? item = null,
            MetaDataComponent? meta = null,
            StackComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return $"<item {uid}>";

            return GermanBuiltins.GetDisplayData(uid, meta.DisplayName!).GetIndirectName(stack.Count);
        }
    }
}
