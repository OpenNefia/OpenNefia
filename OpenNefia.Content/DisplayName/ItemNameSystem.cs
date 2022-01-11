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
            
            string prefix = GermanBuiltins.BaseArticle;
            var locData = _localizationManager.GetEntityData(meta.EntityPrototype?.ID!);
            if (locData.Attributes.TryGetValue(GermanBuiltins.PluralNameAttributeName, out var plrRule) 
                && plrRule == GermanBuiltins.PluralNameAlways)
            {

            }
            else if (locData.Attributes.TryGetValue(GermanBuiltins.GenderAttributeName, out var gender))
            {
                prefix += gender switch
                {
                    GermanBuiltins.GenderNameMale => GermanBuiltins.ArticleSuffixMale,
                    GermanBuiltins.GenderNameFemale => GermanBuiltins.ArticleSuffixFemale,
                    _ => string.Empty
                };
            }

            if (locData.Attributes.TryGetValue(GermanBuiltins.AdjectiveAttributeName, out var adj))
                prefix += $" {adj}";

            var basename = meta.DisplayName;

            if (stack.Count == 1)
                return $"{prefix} {basename}";

            if (locData.Attributes.TryGetValue(GermanBuiltins.PluralAttributeName, out var plural))
            {
                basename = plural;
            }
            return $"{stack.Count} {basename}";
        }
    }
}
