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
        public class EntityDisplayData
        {
            private string Article { get; set; } = "";
            private string Adjective { get; set; } = "";
            public string Noun { get; set; } = "";
            
            EntityLocData Data { get; set; } = default!;

            public EntityDisplayData(string noun, EntityLocData data)
            {
                Noun = noun;
                Data = data;
            }

            private void Init(bool direct)
            {
                Article = BaseArticle;
                Adjective = string.Empty;
                if (Data.Attributes.TryGetValue(PluralNameAttributeName, out var plrRule)
                    && plrRule == PluralNameAlways)
                {
                    Article = string.Empty;
                }
                else if (Data.Attributes.TryGetValue(GenderAttributeName, out var gender))
                {
                    Article += gender switch
                    {
                        GenderNameMale => direct ? string.Empty : ArticleSuffixMale,
                        GenderNameFemale => ArticleSuffixFemale,
                        _ => string.Empty
                    };
                }
                if (Data.Attributes.TryGetValue(direct ? AdjectiveDirectAttributeName : AdjectiveAttributeName, out var adj))
                    Adjective = adj;
            }

            public string GetStackName(int count)
            {
                if (count <= 1)
                    return GetIndirectName();

                Init(false);
                var baseName = Noun;
                if (Data.Attributes.TryGetValue(PluralAttributeName, out var plural))
                {
                    baseName = plural;
                }
                var parts = new List<string>
                {
                    $"{count}"
                };
                if (Data.Attributes.TryGetValue(AdjectivePluralAttributeName, out var adj))
                    parts.Add(adj);
                parts.Add(baseName);
                return string.Join(" ", parts);
            }

            private string GetName(bool direct)
            {
                Init(direct);
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(Article))
                    parts.Add(Article);
                if (!string.IsNullOrEmpty(Adjective))
                    parts.Add(Adjective);
                parts.Add(Noun);
                return string.Join(" ", parts);
            }

            public string GetDirectName() => GetName(true);

            public string GetIndirectName() => GetName(false);
        }

        public const string BaseArticle = "ein";
        public const string GenderAttributeName = "Gender";
        public const string PluralAttributeName = "Plural";

        public const string PluralNameAttributeName = "PluralRule";
        public const string PluralNameAlways = "always";

        public const string GenderNameMale = "male";
        public const string GenderNameFemale = "female";
        public const string GenderNameNeutral = "neutral";

        public const string AdjectiveAttributeName = "Adjective";
        public const string AdjectivePluralAttributeName = "AdjectivePlural";
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

            return GetDisplayData(entity).GetDirectName();
        }

        public static EntityDisplayData GetDisplayData(EntityUid entity)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            var prototype = entMan.GetComponent<MetaDataComponent>(entity).EntityPrototype;
            return new EntityDisplayData(DisplayNameSystem.GetDisplayName(entity), Loc.GetLocalizationData(prototype?.ID!));
        }
    }
}
