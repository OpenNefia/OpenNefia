using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Locale.Funcs
{
    [RegisterLocaleFunctions("de_DE")]
    public static class GermanBuiltins
    {
        public const string BaseArticle = "ein";
        public const string GenderAttributeName = "Gender";
        public const string PluralAttributeName = "Plural";

        public const string PluralNameAttributeName = "PluralName";
        public const string PluralNameAlways = "always";

        public const string GenderNameMale = "male";
        public const string GenderNameFemale = "female";
        public const string GenderNameNeutral = "neutral";

        public const string AdjectiveAttributeName = "Adjective";
        public const string AdjectiveDirectAttributeName = "AdjectiveDirect";

        public const string ArticleSuffixFemale = "e";
        public const string ArticleSuffixMale = "en";
        /// <summary>
        /// Function: name(entity, ignoreSight)
        /// </summary>
        /// <hsp>#defcfunc name int tc</hsp>
        [LocaleFunction("name")]
        public static string BuiltIn_name(object? obj, string? directness = null)
        {
            if (obj is not EntityUid entity)
            {
                return Loc.GetString("Elona.GameObjects.Common.Something");
            }

            var gameSession = IoCManager.Resolve<IGameSessionManager>();

            if (gameSession.IsPlayer(entity))
                return Loc.GetString("Elona.GameObjects.Common.You");

            var visibilitySys = EntitySystem.Get<VisibilitySystem>();

            if (!visibilitySys.CanSeeEntity(GameSession.Player, entity))
            {
                return Loc.GetString("Elona.GameObjects.Common.Something");
            }

            var name = DisplayNameSystem.GetDisplayName(entity);
            if (char.IsDigit(name.First()))
                return name;

            var parts = name.Split(' ');
            if (parts.Length <= 1)
                return name;

            var entMan = IoCManager.Resolve<IEntityManager>();
            var prototype = entMan.GetComponent<MetaDataComponent>(entity).EntityPrototype;
            var locData = Loc.GetLocalizationData(prototype?.ID!);
            if (locData.Attributes.TryGetValue(GenderAttributeName, out var gender))
            {
                var prefix = gender switch
                {
                    GenderNameFemale => BaseArticle + ArticleSuffixFemale,
                    _ => BaseArticle
                };

                if (locData.Attributes.TryGetValue(AdjectiveDirectAttributeName, out var adj))
                    prefix += $" {adj}";

                return $"{prefix} {parts.Last()}";
            }
            return name;
        }
    }
}
