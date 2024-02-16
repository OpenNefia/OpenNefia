using Microsoft.CodeAnalysis.Tags;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System.Collections.Immutable;
using AssetPrototypeID = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Rendering.AssetPrototype>;

namespace OpenNefia.Content.Repl
{
    internal static class ReplIconsProtos
    {
        #pragma warning disable format
        public static AssetPrototypeID ReplCompletionIcon_Array            = new($"Elona.{nameof(ReplCompletionIcon_Array)}");
        public static AssetPrototypeID ReplCompletionIcon_Boolean          = new($"Elona.{nameof(ReplCompletionIcon_Boolean)}");
        public static AssetPrototypeID ReplCompletionIcon_Class            = new($"Elona.{nameof(ReplCompletionIcon_Class)}");
        public static AssetPrototypeID ReplCompletionIcon_Color            = new($"Elona.{nameof(ReplCompletionIcon_Color)}");
        public static AssetPrototypeID ReplCompletionIcon_Constant         = new($"Elona.{nameof(ReplCompletionIcon_Constant)}");
        public static AssetPrototypeID ReplCompletionIcon_Document         = new($"Elona.{nameof(ReplCompletionIcon_Document)}");
        public static AssetPrototypeID ReplCompletionIcon_EnumeratorMember = new($"Elona.{nameof(ReplCompletionIcon_EnumeratorMember)}");
        public static AssetPrototypeID ReplCompletionIcon_Enumerator       = new($"Elona.{nameof(ReplCompletionIcon_Enumerator)}");
        public static AssetPrototypeID ReplCompletionIcon_Event            = new($"Elona.{nameof(ReplCompletionIcon_Event)}");
        public static AssetPrototypeID ReplCompletionIcon_Field            = new($"Elona.{nameof(ReplCompletionIcon_Field)}");
        public static AssetPrototypeID ReplCompletionIcon_Folder           = new($"Elona.{nameof(ReplCompletionIcon_Folder)}");
        public static AssetPrototypeID ReplCompletionIcon_Interface        = new($"Elona.{nameof(ReplCompletionIcon_Interface)}");
        public static AssetPrototypeID ReplCompletionIcon_Key              = new($"Elona.{nameof(ReplCompletionIcon_Key)}");
        public static AssetPrototypeID ReplCompletionIcon_Keyword          = new($"Elona.{nameof(ReplCompletionIcon_Keyword)}");
        public static AssetPrototypeID ReplCompletionIcon_Library          = new($"Elona.{nameof(ReplCompletionIcon_Library)}");
        public static AssetPrototypeID ReplCompletionIcon_LocalVariable    = new($"Elona.{nameof(ReplCompletionIcon_LocalVariable)}");
        public static AssetPrototypeID ReplCompletionIcon_Method           = new($"Elona.{nameof(ReplCompletionIcon_Method)}");
        public static AssetPrototypeID ReplCompletionIcon_Misc             = new($"Elona.{nameof(ReplCompletionIcon_Misc)}");
        public static AssetPrototypeID ReplCompletionIcon_Namespace        = new($"Elona.{nameof(ReplCompletionIcon_Namespace)}");
        public static AssetPrototypeID ReplCompletionIcon_Numeric          = new($"Elona.{nameof(ReplCompletionIcon_Numeric)}");
        public static AssetPrototypeID ReplCompletionIcon_Operator         = new($"Elona.{nameof(ReplCompletionIcon_Operator)}");
        public static AssetPrototypeID ReplCompletionIcon_Parameter        = new($"Elona.{nameof(ReplCompletionIcon_Parameter)}");
        public static AssetPrototypeID ReplCompletionIcon_Property         = new($"Elona.{nameof(ReplCompletionIcon_Property)}");
        public static AssetPrototypeID ReplCompletionIcon_Ruler            = new($"Elona.{nameof(ReplCompletionIcon_Ruler)}");
        public static AssetPrototypeID ReplCompletionIcon_Snippet          = new($"Elona.{nameof(ReplCompletionIcon_Snippet)}");
        public static AssetPrototypeID ReplCompletionIcon_String           = new($"Elona.{nameof(ReplCompletionIcon_String)}");
        public static AssetPrototypeID ReplCompletionIcon_Structure        = new($"Elona.{nameof(ReplCompletionIcon_Structure)}");
        public static AssetPrototypeID ReplCompletionIcon_Unlink           = new($"Elona.{nameof(ReplCompletionIcon_Unlink)}");
        public static AssetPrototypeID ReplCompletionIcon_Variable         = new($"Elona.{nameof(ReplCompletionIcon_Variable)}");
        #pragma warning restore format
    }

    internal class ReplCompletionIcons
    {
        private static IAssetManager _assets => IoCManager.Resolve<IAssetManager>();

        public static Dictionary<string, AssetPrototypeID> RoslynTagToIconAssetPrototypeID = new Dictionary<string, AssetPrototypeID>()
        {
            { WellKnownTags.Public,            ReplIconsProtos.ReplCompletionIcon_Keyword },
            { WellKnownTags.Protected,         ReplIconsProtos.ReplCompletionIcon_Keyword },
            { WellKnownTags.Private,           ReplIconsProtos.ReplCompletionIcon_Keyword },
            { WellKnownTags.Internal,          ReplIconsProtos.ReplCompletionIcon_Keyword },
            { WellKnownTags.File,              ReplIconsProtos.ReplCompletionIcon_Document },
            { WellKnownTags.Project,           ReplIconsProtos.ReplCompletionIcon_Document },
            { WellKnownTags.Folder,            ReplIconsProtos.ReplCompletionIcon_Folder },
            { WellKnownTags.Assembly,          ReplIconsProtos.ReplCompletionIcon_Library },
            { WellKnownTags.Class,             ReplIconsProtos.ReplCompletionIcon_Class },
            { WellKnownTags.Constant,          ReplIconsProtos.ReplCompletionIcon_Constant },
            { WellKnownTags.Delegate,          ReplIconsProtos.ReplCompletionIcon_Method },
            { WellKnownTags.Enum,              ReplIconsProtos.ReplCompletionIcon_Enumerator },
            { WellKnownTags.EnumMember,        ReplIconsProtos.ReplCompletionIcon_EnumeratorMember },
            { WellKnownTags.Event,             ReplIconsProtos.ReplCompletionIcon_Event },
            { WellKnownTags.ExtensionMethod,   ReplIconsProtos.ReplCompletionIcon_Method },
            { WellKnownTags.Field,             ReplIconsProtos.ReplCompletionIcon_Field },
            { WellKnownTags.Interface,         ReplIconsProtos.ReplCompletionIcon_Interface },
            { WellKnownTags.Intrinsic,         ReplIconsProtos.ReplCompletionIcon_Misc },
            { WellKnownTags.Keyword,           ReplIconsProtos.ReplCompletionIcon_Keyword },
            { WellKnownTags.Label,             ReplIconsProtos.ReplCompletionIcon_Misc },
            { WellKnownTags.Local,             ReplIconsProtos.ReplCompletionIcon_LocalVariable },
            { WellKnownTags.Namespace,         ReplIconsProtos.ReplCompletionIcon_Namespace },
            { WellKnownTags.Method,            ReplIconsProtos.ReplCompletionIcon_Method },
            { WellKnownTags.Module,            ReplIconsProtos.ReplCompletionIcon_Library },
            { WellKnownTags.Operator,          ReplIconsProtos.ReplCompletionIcon_Operator },
            { WellKnownTags.Parameter,         ReplIconsProtos.ReplCompletionIcon_Parameter },
            { WellKnownTags.Property,          ReplIconsProtos.ReplCompletionIcon_Property },
            { WellKnownTags.RangeVariable,     ReplIconsProtos.ReplCompletionIcon_Variable },
            { WellKnownTags.Reference,         ReplIconsProtos.ReplCompletionIcon_Unlink },
            { WellKnownTags.Structure,         ReplIconsProtos.ReplCompletionIcon_Structure },
            { WellKnownTags.TypeParameter,     ReplIconsProtos.ReplCompletionIcon_Parameter },
            { WellKnownTags.Snippet,           ReplIconsProtos.ReplCompletionIcon_Snippet },
            { WellKnownTags.Error,             ReplIconsProtos.ReplCompletionIcon_Misc },
            { WellKnownTags.Warning,           ReplIconsProtos.ReplCompletionIcon_Misc },
            // { WellKnownTags.StatusInformation, ReplIconsAssetProtos.ReplCompletionIcon_Misc },
            // { WellKnownTags.AddReference,      ReplIconsAssetProtos.ReplCompletionIcon_Misc },
            // { WellKnownTags.NuGet,             ReplIconsAssetProtos.ReplCompletionIcon_Misc }
        };

        private Dictionary<AssetPrototypeID, IAssetInstance> Drawables = new();

        public IAssetInstance GetIcon(ImmutableArray<string> roslynTags)
        {
            foreach (var tag in roslynTags)
            {
                if (RoslynTagToIconAssetPrototypeID.TryGetValue(tag, out var id))
                {
                    return _assets.GetAsset(id);
                }
            }

            return _assets.GetAsset(ReplIconsProtos.ReplCompletionIcon_Misc);
        }
    }
}
