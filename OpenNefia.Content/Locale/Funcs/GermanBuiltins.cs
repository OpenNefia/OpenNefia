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
            [Dependency] private readonly IEntityManager _entMan = default!;
            private string Article { get; set; } = "";
            private string Adjective { get; set; } = "";
            public string Noun { get; set; } = "";
            
            EntityLocData Data { get; set; } = default!;

            public EntityDisplayData(EntityUid entity, string noun)
            {
                IoCManager.InjectDependencies(this);
                var prototype = _entMan.GetComponent<MetaDataComponent>(entity).EntityPrototype;
                Data = Loc.GetLocalizationData(prototype?.ID!);
                Noun = noun;
            }

            private void Init(bool direct, bool plural)
            {
                Article = BaseArticle;
                Adjective = string.Empty;
                if (plural)
                {
                    if (Data.Attributes.TryGetValue(AdjectivePluralAttributeName, out var adj))
                        Adjective = adj;
                }
                else
                {
                    if (Data.Attributes.TryGetValue(direct ? AdjectiveDirectAttributeName : AdjectiveAttributeName, out var adj))
                        Adjective = adj;
                }

                if ((Data.Attributes.TryGetValue(PluralNameAttributeName, out var plrRule) && plrRule == PluralNameAlways)
                    || plural)
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
            }

            private string GetName(bool direct, int count)
            {
                Init(direct, count > 1);
                var parts = new List<string>();

                if (count <= 1)
                {
                    if (!string.IsNullOrEmpty(Article))
                        parts.Add(Article);
                }
                else
                {
                    parts.Add(count.ToString());
                }

                if (!string.IsNullOrEmpty(Adjective))
                    parts.Add(Adjective);
                parts.Add(Noun);
                return string.Join(" ", parts);
            }

            public string GetDirectName(int count) => GetName(true, count);

            public string GetIndirectName(int count) => GetName(false, count);
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

            var entMan = IoCManager.Resolve<IEntityManager>();
            if (!entMan.TryGetComponent<MetaDataComponent>(entity, out var meta))
            {

            }
            if (!entMan.TryGetComponent<StackComponent>(entity, out var stack))
            {

            }
            return GetDisplayData(entity, meta.DisplayName!).GetDirectName(stack?.Count ?? 1);
        }

        public static EntityDisplayData GetDisplayData(EntityUid entity, string noun)
        {
            return new EntityDisplayData(entity, noun);
        }
    }
}
