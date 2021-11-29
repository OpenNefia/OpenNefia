using Microsoft.CodeAnalysis.Tags;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using System.Collections.Immutable;
using AssetPrototypeID = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Rendering.AssetPrototype>;

namespace OpenNefia.Core.UI.Layer.Repl
{
    internal static class ReplIconsAssetPrototypeIDs
    {
        public static AssetPrototypeID ReplCompletionIcon_Array            = new(nameof(ReplCompletionIcon_Array));
        public static AssetPrototypeID ReplCompletionIcon_Boolean          = new(nameof(ReplCompletionIcon_Boolean));
        public static AssetPrototypeID ReplCompletionIcon_Class            = new(nameof(ReplCompletionIcon_Class));
        public static AssetPrototypeID ReplCompletionIcon_Color            = new(nameof(ReplCompletionIcon_Color));
        public static AssetPrototypeID ReplCompletionIcon_Constant         = new(nameof(ReplCompletionIcon_Constant));
        public static AssetPrototypeID ReplCompletionIcon_Document         = new(nameof(ReplCompletionIcon_Document));
        public static AssetPrototypeID ReplCompletionIcon_EnumeratorMember = new(nameof(ReplCompletionIcon_EnumeratorMember));
        public static AssetPrototypeID ReplCompletionIcon_Enumerator       = new(nameof(ReplCompletionIcon_Enumerator));
        public static AssetPrototypeID ReplCompletionIcon_Event            = new(nameof(ReplCompletionIcon_Event));
        public static AssetPrototypeID ReplCompletionIcon_Field            = new(nameof(ReplCompletionIcon_Field));
        public static AssetPrototypeID ReplCompletionIcon_Folder           = new(nameof(ReplCompletionIcon_Folder));
        public static AssetPrototypeID ReplCompletionIcon_Interface        = new(nameof(ReplCompletionIcon_Interface));
        public static AssetPrototypeID ReplCompletionIcon_Key              = new(nameof(ReplCompletionIcon_Key));
        public static AssetPrototypeID ReplCompletionIcon_Keyword          = new(nameof(ReplCompletionIcon_Keyword));
        public static AssetPrototypeID ReplCompletionIcon_Library          = new(nameof(ReplCompletionIcon_Library));
        public static AssetPrototypeID ReplCompletionIcon_LocalVariable    = new(nameof(ReplCompletionIcon_LocalVariable));
        public static AssetPrototypeID ReplCompletionIcon_Method           = new(nameof(ReplCompletionIcon_Method));
        public static AssetPrototypeID ReplCompletionIcon_Misc             = new(nameof(ReplCompletionIcon_Misc));
        public static AssetPrototypeID ReplCompletionIcon_Namespace        = new(nameof(ReplCompletionIcon_Namespace));
        public static AssetPrototypeID ReplCompletionIcon_Numeric          = new(nameof(ReplCompletionIcon_Numeric));
        public static AssetPrototypeID ReplCompletionIcon_Operator         = new(nameof(ReplCompletionIcon_Operator));
        public static AssetPrototypeID ReplCompletionIcon_Parameter        = new(nameof(ReplCompletionIcon_Parameter));
        public static AssetPrototypeID ReplCompletionIcon_Property         = new(nameof(ReplCompletionIcon_Property));
        public static AssetPrototypeID ReplCompletionIcon_Ruler            = new(nameof(ReplCompletionIcon_Ruler));
        public static AssetPrototypeID ReplCompletionIcon_Snippet          = new(nameof(ReplCompletionIcon_Snippet));
        public static AssetPrototypeID ReplCompletionIcon_String           = new(nameof(ReplCompletionIcon_String));
        public static AssetPrototypeID ReplCompletionIcon_Structure        = new(nameof(ReplCompletionIcon_Structure));
        public static AssetPrototypeID ReplCompletionIcon_Unlink           = new(nameof(ReplCompletionIcon_Unlink));
        public static AssetPrototypeID ReplCompletionIcon_Variable         = new(nameof(ReplCompletionIcon_Variable));
    }

    internal class ReplCompletionIcons : IDisposable
    {
        private static IAssetManager _assets => IoCManager.Resolve<IAssetManager>();

        public static Dictionary<string, AssetPrototypeID> RoslynTagToIconAssetPrototypeID = new Dictionary<string, AssetPrototypeID>()
        {
            { WellKnownTags.Public,            ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Keyword },
            { WellKnownTags.Protected,         ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Keyword },
            { WellKnownTags.Private,           ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Keyword },
            { WellKnownTags.Internal,          ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Keyword },
            { WellKnownTags.File,              ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Document },
            { WellKnownTags.Project,           ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Document },
            { WellKnownTags.Folder,            ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Folder },
            { WellKnownTags.Assembly,          ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Library },
            { WellKnownTags.Class,             ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Class },
            { WellKnownTags.Constant,          ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Constant },
            { WellKnownTags.Delegate,          ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Method },
            { WellKnownTags.Enum,              ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Enumerator },
            { WellKnownTags.EnumMember,        ReplIconsAssetPrototypeIDs.ReplCompletionIcon_EnumeratorMember },
            { WellKnownTags.Event,             ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Event },
            { WellKnownTags.ExtensionMethod,   ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Method },
            { WellKnownTags.Field,             ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Field },
            { WellKnownTags.Interface,         ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Interface },
            { WellKnownTags.Intrinsic,         ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Misc },
            { WellKnownTags.Keyword,           ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Keyword },
            { WellKnownTags.Label,             ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Misc },
            { WellKnownTags.Local,             ReplIconsAssetPrototypeIDs.ReplCompletionIcon_LocalVariable },
            { WellKnownTags.Namespace,         ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Namespace },
            { WellKnownTags.Method,            ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Method },
            { WellKnownTags.Module,            ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Library },
            { WellKnownTags.Operator,          ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Operator },
            { WellKnownTags.Parameter,         ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Parameter },
            { WellKnownTags.Property,          ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Property },
            { WellKnownTags.RangeVariable,     ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Variable },
            { WellKnownTags.Reference,         ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Unlink },
            { WellKnownTags.Structure,         ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Structure },
            { WellKnownTags.TypeParameter,     ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Parameter },
            { WellKnownTags.Snippet,           ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Snippet },
            { WellKnownTags.Error,             ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Misc },
            { WellKnownTags.Warning,           ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Misc },
            // { WellKnownTags.StatusInformation, ReplIconsAssetPrototypeIDOf.ReplCompletionIcon_Misc },
            // { WellKnownTags.AddReference,      ReplIconsAssetPrototypeIDOf.ReplCompletionIcon_Misc },
            // { WellKnownTags.NuGet,             ReplIconsAssetPrototypeIDOf.ReplCompletionIcon_Misc }
        };

        private Dictionary<AssetPrototypeID, AssetDrawable> Drawables = new();

        public AssetDrawable GetIcon(ImmutableArray<string> roslynTags)
        {
            foreach (var tag in roslynTags)
            {
                if (RoslynTagToIconAssetPrototypeID.TryGetValue(tag, out var id))
                {
                    return _assets.GetAsset(id);
                }
            }

            return _assets.GetAsset(ReplIconsAssetPrototypeIDs.ReplCompletionIcon_Misc);
        }

        public void Dispose()
        {
            foreach (var drawable in Drawables.Values)
                drawable.Dispose();
        }
    }
}
